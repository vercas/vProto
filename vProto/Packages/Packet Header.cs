using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public struct PackageHeader
    {
        public const int StructSize = sizeof(TypeOfSize) + sizeof(TypeOfType) + sizeof(TypeOfID) + sizeof(TypeOfTimeout);

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
