using System;
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

        private static readonly byte[] emptyPayload = new byte[] { };


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

        protected Timer heartbeatTimer = null;

        protected void __heartbeatTimerCallback(object state)
        {
            if (IsConnected)
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
                heartbeatTimeoutTimer.Change(_hbTimeout, Timeout.InfiniteTimeSpan);
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

                Console.WriteLine("Heartbeat failure! ({0})", consecutiveFailures);
            }

            try
            {
                heartbeatTimeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
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
                heartbeatTimeoutTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }


        /* The methods that handle heartbeat-specific pipe events.
         */

        protected virtual void OnInternalHeartbeatRequestReceived(Package pack)
        {
            //Console.WriteLine("Bouncing heartbeat.");

            for (int i = 0; i < 3; i++)
                if (LowSendPacket(new Packages.PackageHeader() { Type = PackageType.HeartbeatResponse, ID = pack.Header.ID }, emptyPayload))
                    break;
        }

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

            if (!IsConnected)
                return false;

            lock (heartbeat_sync)
                if (awaitingHeartbeat)
                    return false;
                else
                    awaitingHeartbeat = true;

            if (heartbeatTimeoutTimer == null)
                heartbeatTimeoutTimer = new Timer(new TimerCallback(__heartbeatTimeoutCallback));

            lastIDsent = __getNewHeartbeatID();

            __prepareHeartbeat();

            bool ok = LowSendPacket(new PackageHeader() { Type = PackageType.HeartbeatRequest, ID = lastIDsent }, emptyPayload);

            if (!ok)
                __scoreHeartbeatFailure();

            return true;
        }
    }
}
