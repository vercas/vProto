using System;
using System.Runtime.InteropServices;

using TypeOfSize = System.UInt32;
using TypeOfType = vProto.Packages.PackageType;
using TypeOfID = System.Int32;
using HalfTypeOfID = System.Int16;
using TypeOfTimeout = System.UInt16;

namespace vProto.Packages
{
    /// <summary>
    /// The header structure of all the packages of the protocol.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = PackageHeader.StructSize, CharSet = CharSet.Ansi)]
    public unsafe struct PackageHeader
    {
        /// <summary>
        /// The exact size in bytes of the vProto.Packages.PackageHeader structure.
        /// </summary>
        public const int StructSize = sizeof(TypeOfSize) + sizeof(TypeOfType) + sizeof(TypeOfID) + sizeof(TypeOfTimeout);

        #region Buffer
        [FieldOffset(0)]
        internal fixed byte BUFFER[StructSize];

        internal byte[] Buffer
        {
            set
            {
                fixed (byte* b = BUFFER)
                {
                    byte* bptr = b;

                    for (int i = 0; i < StructSize; i++)
                        *bptr++ = (byte)value[i];
                }
            }
            get
            {
                byte[] buffer = new byte[StructSize];

                fixed (byte* b = BUFFER)
                {
                    byte* bptr = b;

                    for (int i = 0; i < StructSize; i++)
                        buffer[i] = (byte)*bptr++;
                }

                return buffer;
            }
        }
        #endregion

        /// <summary>
        /// Size of the package's payload.
        /// <para>Header not included.</para>
        /// </summary>
        [FieldOffset(0)]
        public TypeOfSize Size;

        /// <summary>
        /// Type of package.
        /// </summary>
        [FieldOffset(sizeof(TypeOfSize))]
        public TypeOfType Type;

        /// <summary>
        /// ID of the package.
        /// <para>Used as a whole for data types.</para>
        /// </summary>
        [FieldOffset(sizeof(TypeOfSize) + sizeof(TypeOfType))]
        public TypeOfID ID;

        /// <summary>
        /// Top half of the package ID.
        /// <para>Used for request and response matching.</para>
        /// </summary>
        [FieldOffset(sizeof(TypeOfSize) + sizeof(TypeOfType))]
        public HalfTypeOfID IDTop;
        /// <summary>
        /// Bottom half of the package ID.
        /// <para>Used for requst and response types.</para>
        /// </summary>
        [FieldOffset(sizeof(TypeOfSize) + sizeof(TypeOfType) + sizeof(HalfTypeOfID))]
        public HalfTypeOfID IDBottom;

        /// <summary>
        /// Request timeout duration.
        /// <para>Expressed in centiseconds.</para>
        /// </summary>
        [FieldOffset(sizeof(TypeOfSize) + sizeof(TypeOfType) + sizeof(TypeOfID))]
        public TypeOfTimeout RequestTimeout;
    }
}
