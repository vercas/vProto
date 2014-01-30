using System;

namespace vProto.Internals
{
    internal static class BitConversion
    {
        [ThreadStatic]
        static byte[] buff;

        public static int CombineShorts(short a, short b)
        {
            if (buff == null)
                buff = new byte[16];

            BitConverter.GetBytes(a).CopyTo(buff, 0);
            BitConverter.GetBytes(b).CopyTo(buff, 2);

            return BitConverter.ToInt32(buff, 0);
        }
    }
}
