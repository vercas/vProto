using System;

namespace vProto.Internal_Utilities
{
    internal static class Bit_Conversions
    {
        static object buffLock = new object();
        static byte[] buff = new byte[16];

        public static int CombineShorts(short a, short b)
        {
            lock (buffLock) //  BLOODY MULTITHREADING M8
            {
                BitConverter.GetBytes(a).CopyTo(buff, 0);
                BitConverter.GetBytes(b).CopyTo(buff, 2);

                return BitConverter.ToInt32(buff, 0);
            }
        }
    }
}
