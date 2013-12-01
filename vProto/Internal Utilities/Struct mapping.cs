using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.InteropServices;

namespace vProto.Internal_Utilities
{
    internal static class Struct_mapping
    {
        public static byte[] StructureToByteArray<T>(T obj)
            where T : struct
        {
            int len = Marshal.SizeOf(obj);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static int ByteArrayToStructure<T>(byte[] bytearray, ref T obj, int start = 0)
            where T : struct
        {
            int len = Marshal.SizeOf(typeof(T));

            IntPtr i = Marshal.AllocHGlobal(len);

            Marshal.Copy(bytearray, start, i, len);

            obj = (T)Marshal.PtrToStructure(i, typeof(T));

            Marshal.FreeHGlobal(i);

            return start + len;
        }
    }
}
