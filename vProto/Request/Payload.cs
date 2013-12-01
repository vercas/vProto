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
    partial class Request
    {
        System.IO.MemoryStream str = null;


        /// <summary>
        /// Sets the payload of the request.
        /// </summary>
        /// <param name="bt">Array of bytes constituting the payload.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(byte[] bt)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (bt == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            str = new System.IO.MemoryStream(bt);
            //str.Write(bt, 0, bt.Length);

            return this;
        }

        /// <summary>
        /// Extracts the contents of the given stream according to the parameters and assigns them as the request payload.
        /// </summary>
        /// <param name="stream">The stream from which the data is extracted.</param>
        /// <param name="offset">The offset at which to begin extraction relative to the seek origin.</param>
        /// <param name="length">The number of bytes to copy. Usage of a negative number means copying everything to the end of the stream.</param>
        /// <param name="origin">The point from which seeking in the stream should take place.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(System.IO.Stream stream, int length = -1, int offset = 0, System.IO.SeekOrigin origin = System.IO.SeekOrigin.Begin)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (stream == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            stream.Seek(offset, origin);

            if (length < 0)
                length = (int)(stream.Length - stream.Position);

            var ba = new byte[length];

            int read = 0;

            while (read < length)
            {
                read += stream.Read(ba, read, length - read);
            }

            str = new System.IO.MemoryStream(ba);

            return this;
        }

        /// <summary>
        /// Extracts the contents of the given stream according to the parameters and assigns them as the request payload.
        /// </summary>
        /// <param name="stream">The stream from which the data is extracted.</param>
        /// <param name="length">The number of bytes to copy. Usage of a negative number means copying everything to the end of the stream.</param>
        /// <returns>The request object.</returns>
        public Request SetPayload(System.IO.Stream stream, int length = -1)
        {
            if (Disposed)
                throw new ObjectDisposedException("Cannot change a disposed request!", (Exception)null);

            if (stream == null)
                throw new ArgumentNullException("bt", "Payload cannot be null!");

            if (str != null)
                try
                {
                    str.Dispose();
                }
                catch { }

            if (length < 0)
                length = (int)(stream.Length - stream.Position);

            var ba = new byte[length];

            int read = 0;

            while (read < length)
            {
                read += stream.Read(ba, read, length - read);
            }

            str = new System.IO.MemoryStream(ba);

            return this;
        }
    }
}
