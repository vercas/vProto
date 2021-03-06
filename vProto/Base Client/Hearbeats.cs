﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace vProto
{
    using Packages;

    partial class BaseClient
    {
        /// <summary>
        /// Default interval at which heartbeats are sent.
        /// <para>30 seconds.</para>
        /// </summary>
        public static readonly TimeSpan HeartbeatInterval = new TimeSpan(0, 0, 30);

        /// <summary>
        /// Default value for heartbeat timeout.
        /// <para>10 seconds.</para>
        /// </summary>
        public static readonly TimeSpan DefaultHeartbeatTimeout = new TimeSpan(0, 0, 10);

        /// <summary>
        /// Minimum value for heartbeat timeout.
        /// <para>500 milliseconds or half a second.</para>
        /// </summary>
        public static readonly TimeSpan MinimumHeartbeatTimeout = new TimeSpan(0, 0, 0, 0, 500);


        private TimeSpan _hbTimeout = DefaultHeartbeatTimeout;

        /// <summary>
        /// Gets or sets the timeout duration of a heartbeat.
        /// </summary>
        public TimeSpan HeartbeatTimeout
        {
            get
            {
                return _hbTimeout;
            }
            set
            {
                if (value < MinimumHeartbeatTimeout)
                    throw new ArgumentOutOfRangeException("value", "Value must be larger or equal to vProto.BaseClient.MinimumHeartbeatTimeout (500 ms).");

                _hbTimeout = value;
            }
        }


        /// <summary>
        /// Gets a delayed and inaccurate approximation of the connection's one-way latency.
        /// </summary>
        public TimeSpan Ping { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a heartbeat request is pending.
        /// </summary>
        public Boolean IsAwaitingHeartbeat { get { return awaitingHeartbeat; } }


        int htbCounter = int.MinValue;

        private int __getNewHeartbeatID()
        {
            if (htbCounter == int.MaxValue)
            {
                htbCounter = int.MinValue;
                return int.MaxValue;
            }
            else
            {
                return htbCounter++;
            }
        }


        /* Heartbeat regulating timer.
         */

        Timer heartbeatTimer = null;

        void __heartbeatTimerCallback(object state)
        {
            if (IsInternallyConnected)
                SendHeartbeat();
        }


        /* These varaibles hold the state of the heartbeat mechanism.
         */

        int consecutiveFailures = 0;
        bool awaitingHeartbeat = false;
        int lastIDsent = 0;

        /* Mostly synchronizes access to awaitingHeartbeat and consecutiveFailures. */
        object heartbeat_sync = new object();
        /* Used to calculate pings. */
        DateTime heartbeatSendTime;


        /* The timer used for heartbeat timeouts.
         * This is messy.
         */

        Timer heartbeatTimeoutTimer = null;

        private void __heartbeatTimeoutCallback(object state)
        {
            if (Disposed)
                return;

            //Console.WriteLine("Heartbeat timeout callback!!");

            bool awaiting = false;

            lock (heartbeat_sync)
                awaiting = awaitingHeartbeat;

            if (awaiting)
                __scoreHeartbeatFailure();
        }


        /* The private methods that constitute the "operations" performed by heartbeats.
         */

        private void __prepareHeartbeat()
        {
            lock (heartbeat_sync)
            {
                heartbeatSendTime = DateTime.Now;
            }

            try
            {
                heartbeatTimeoutTimer.Change(_hbTimeout, BaseClient.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void __scoreHeartbeatFailure()
        {
            lock (heartbeat_sync)
            {
                awaitingHeartbeat = false;
                consecutiveFailures++;

                //Console.WriteLine("Heartbeat failure! ({0})", consecutiveFailures);

                if (consecutiveFailures >= 3)
                    _CheckIfStopped(null);

                //  If you do something to cause this, it's your fault.
            }

            try
            {
                heartbeatTimeoutTimer.Change(BaseClient.InfiniteTimeSpan, BaseClient.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void __scoreHeartbeatSuccess()
        {
            lock (heartbeat_sync)
            {
                awaitingHeartbeat = false;
                consecutiveFailures = 0;

                Ping = new TimeSpan((DateTime.Now - heartbeatSendTime).Ticks / 2);

                //Console.WriteLine("Heartbeat success! ({0})", Ping);
            }

            try
            {
                heartbeatTimeoutTimer.Change(BaseClient.InfiniteTimeSpan, BaseClient.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }


        /* The methods that handle heartbeat-specific pipe events.
         */

        /// <summary>
        /// Invoked on receipt of heartbeat request from the other side.
        /// </summary>
        /// <param name="pack">Package detailing heartbeat data.</param>
        protected virtual void OnInternalHeartbeatRequestReceived(Package pack)
        {
            try
            {
                LowSendPackage(new Packages.PackageHeader() { Type = PackageType.HeartbeatResponse, ID = pack.Header.ID }, emptyPayload);
            }
            catch (ObjectDisposedException)
            {
                //  Do nothing.
            }
            catch (Exception x)
            {
                _CheckIfStopped(x);
            }
        }

        /// <summary>
        /// Invoked on receipt of heartbeat response from the other side.
        /// </summary>
        /// <param name="pack">Package detailing heartbeat data.</param>
        protected virtual void OnInternalHeartbeatResponseReceived(Package pack)
        {
            bool awaiting = false;

            lock (heartbeat_sync)
                awaiting = awaitingHeartbeat;

            if (awaiting)
            {
                if (pack.Header.ID == lastIDsent)
                    __scoreHeartbeatSuccess();
                //else    //  Shouldn't really happen, but meh. Don't risk it.
                //    __scoreHeartbeatFailure();
                //  Well. Old timeout packages might arrive very late due to weird pings. They must simply be ignored.
            }
        }

        /// <summary>
        /// Invoked when a heartbeat-related failure occurred.
        /// </summary>
        /// <param name="pack">Package detailing heartbeat data, if available.</param>
        /// <param name="x">Exception causing the failure.</param>
        protected virtual void OnInternalHeartbeatFailure(Package pack, Exception x)
        {
            __scoreHeartbeatFailure();
        }


        /// <summary>
        /// Sends a heartbeat package to the other end.
        /// <para>Used to check connection health and calculate the ping.</para>
        /// </summary>
        /// <returns>False if a heartbeat is already awaiting or disconnected; otherwise true.</returns>
        public bool SendHeartbeat()
        {
            if (Disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (!IsInternallyConnected)
                return false;

            lock (heartbeat_sync)
                if (awaitingHeartbeat)
                    return false;
                else
                    awaitingHeartbeat = true;

            if (heartbeatTimeoutTimer == null)
                heartbeatTimeoutTimer = new Timer(new TimerCallback(__heartbeatTimeoutCallback), null, BaseClient.InfiniteTimeSpan, BaseClient.InfiniteTimeSpan);

            lastIDsent = __getNewHeartbeatID();

            __prepareHeartbeat();

            try
            {
                LowSendPackage(new PackageHeader() { Type = PackageType.HeartbeatRequest, ID = lastIDsent }, emptyPayload);
            }
            catch (ObjectDisposedException)
            {
                //  Do nothing.
            }
            catch (Exception x)
            {
                _CheckIfStopped(x);
            }

            return true;
        }
    }
}
