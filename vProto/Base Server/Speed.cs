using System;
using System.Threading;

namespace vProto
{
    using Packages;

    partial class BaseServer
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



        Timer speedCountingTimer = null;



        void __speedCountingTimerCallback(object state)
        {
            int bytesSent = 0, bytesReceived = 0;

            lock (_chs_sync)
                for (int i = 0; i < _chs.Length; i++)
                    if (_chs[i] != null)
                    {
                        bytesSent += _chs[i].OutgoingSpeed;
                        bytesReceived += _chs[i].IncommingSpeed;
                    }

            OutgoingSpeed = bytesSent;
            IncommingSpeed = bytesReceived;

            if (bytesSent > OutgoingSpeedPeak)
                OutgoingSpeedPeak = bytesSent;
            if (bytesReceived > IncommingSpeedPeak)
                IncommingSpeedPeak = bytesSent;
        }
    }
}
