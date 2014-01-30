using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace vProto.Internals
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
            else
            {
                buffer.Seek(0, SeekOrigin.Begin);
                buffer.SetLength(0L);
            }

            formatter.Serialize(buffer, o);

            return buffer.ToArray();
        }



        public static object Deserialize(byte[] data)
        {
            if (formatter == null)
            {
                formatter = new BinaryFormatter();
                buffer = new MemoryStream(4096);
            }
            else
            {
                buffer.Seek(0, SeekOrigin.Begin);
                buffer.SetLength(0L);
            }

            buffer.Write(data, 0, data.Length);

            buffer.Seek(0, SeekOrigin.Begin);

            return formatter.Deserialize(buffer);
        }



        public static TResult Deserialize<TResult>(byte[] data)
        {
            return (TResult)Deserialize(data);  //  BASICALLY, sugar syntax for myself. :(
        }
    }
}
