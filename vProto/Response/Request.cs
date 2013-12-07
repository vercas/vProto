using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*  A few notes:
 *  1.  try-finally doesn't catch exceptions!!
 */

namespace vProto
{
    partial class Response
    {
        System.IO.MemoryStream reqstr = null;
        byte[] reqarr = null;



        /// <summary>
        /// Gets a memory stream over the request's payload.
        /// </summary>
        public System.IO.MemoryStream RequestPayloadStream { get { return reqstr; } }

        /// <summary>
        /// Gets an array of bites containing the payload of the request.
        /// </summary>
        public Byte[] RequestPayload { get { return reqarr; } }
    }
}
