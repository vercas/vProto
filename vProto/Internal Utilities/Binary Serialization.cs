using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace vProto.Internal_Utilities
{
    internal static class BinarySerialization
    {
        [ThreadStatic]
        private static BinaryFormatter formatter;

        [ThreadStatic]
        private static MemoryStream buffer;

        public static byte[] Serialize(object o)
        {
            if (formatter == null)
            {
                formatter = new BinaryFormatter();
                buffer = new MemoryStream(4096);
            }

            buffer.Seek(0, SeekOrigin.Begin);
            buffer.SetLength(0L);

            formatter.Serialize(buffer, o);

            return buffer.ToArray();
        }
    }
}
