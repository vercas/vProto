using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto
{
    using Packages;

    partial class BaseClient
    {
        /// <summary>
        /// Gets the number of bytes sent in the last second.
        /// </summary>
        public Int32 OutgoingSpeed { get; private set; }

        /// <summary>
        /// Gets the number of bytes received in the last second.
        /// </summary>
        public Int32 IncommingSpeed { get; private set; }


        /// <summary>
        /// Gets the maximum number of bytes sent in a second.
        /// </summary>
        public Int32 OutgoingSpeedPeak { get; private set; }

        /// <summary>
        /// Gets the maximum number of bytes received in a second.
        /// </summary>
        public Int32 IncommingSpeedPeak { get; private set; }



        int bytesSent = 0, bytesReceived = 0;

        object speed_lock = new object();



        private void __addSent(int amnt)
        {
            lock (speed_lock)
                bytesSent += amnt;
        }

        private void __addReceived(int amnt)
        {
            lock (speed_lock)
                bytesReceived += amnt;
        }



        protected System.Threading.Timer speedCountingTimer = null;



        protected void __speedCountingTimerCallback(object state)
        {
            lock (speed_lock)
            {
                OutgoingSpeed = bytesSent;
                IncommingSpeed = bytesReceived;

                if (bytesSent > OutgoingSpeedPeak)
                    OutgoingSpeedPeak = bytesSent;
                if (bytesReceived > IncommingSpeedPeak)
                    IncommingSpeedPeak = bytesSent;

                bytesSent = 0;
                bytesReceived = 0;
            }
        }
    }
}
