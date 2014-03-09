using System;

namespace vProto
{
    using Internals;
    using Packages;

    partial class BaseClient
    {
        const int packetHeaderSize = PackageHeader.StructSize;



#if RECEIVER_THREAD
        protected bool LowStartGettingPackets()
        {
            if (Disposed)
                throw new ObjectDisposedException("vProto.BaseClient", "This client base is disposed of!");

            if (receiver.IsAlive)
            {
                //  Eh?

                return false;
            }

            receiver.Start();

            return true;
        }

        protected void ReceiverLoop()
        {
            byte[] packetHeaderBuff = new byte[packetHeaderSize], packetPayloadBuff = null;
            int packetBytesRead = 0, expectedSize = -1, amnt = 0;
            PackageHeader lastHeader = new PackageHeader();

            while (!Disposed)
            {
                packetBytesRead = 0;
                expectedSize = -1;
                amnt = 0;
                packetPayloadBuff = null;

                while (true)
                {
                    if (packetPayloadBuff == null)
                        //  No payload buffer means we're reading a header.
                        try
                        {
                            //Console.WriteLine("HEADER Receipt start.");

                            //stream.BeginRead(packetHeaderBuff, 0, packetHeaderSize, LowStartGettingPackets_Callback, null);
                            //amnt = stream.Read(packetHeaderBuff, 0, packetHeaderSize);
                            amnt = streamOut.Read(packetHeaderBuff, packetBytesRead, packetHeaderSize - packetBytesRead);
                        }
                        catch (ObjectDisposedException x)
                        {
                            _CheckIfStopped(x);

                            return;
                        }
                        catch (Exception x)
                        {
                            _OnPipeFailure(x, false, null);

                            return;
                        }
                    else
                        try
                        {
                            //Console.WriteLine("PAYLOAD Receipt start.");

                            //stream.BeginRead(packetPayloadBuff, packetBytesRead - packetHeaderSize, expectedSize - packetBytesRead, LowStartGettingPackets_Callback, ar.AsyncState);
                            amnt = streamOut.Read(packetPayloadBuff, packetBytesRead - packetHeaderSize, expectedSize - packetBytesRead);
                        }
                        catch (ObjectDisposedException x)
                        {
                            _CheckIfStopped(x);

                            return;
                        }
                        catch (Exception x)
                        {
                            _OnPipeFailure(x, false, null);

                            return;
                        }

                    if (amnt == 0)
                    {
                        _CheckIfStopped(null);

                        return;
                    }

                    packetBytesRead += amnt;

                    __addReceived(amnt);

                    if (packetBytesRead == packetHeaderSize)
                    {
                        Struct_mapping.ByteArrayToStructure(packetHeaderBuff, ref lastHeader);
                        uint len = lastHeader.Size;

                        expectedSize = (int)len + packetHeaderSize;

                        packetPayloadBuff = new byte[len];
                    }

                    if (packetBytesRead == expectedSize)
                    {
                        try
                        {
                            OnInternalPacketReceived(new Package(lastHeader, packetPayloadBuff));
                        }
                        finally
                        {
                            packetPayloadBuff = null;

                            //  Getting ready to receive the next packet header here.
                        }

                        break;
                    }
                }
            }
        }//*/

#else

        object rec_sync = new object();
        bool receiving = false;

        byte[] packetHeaderBuff = new byte[packetHeaderSize], packetPayloadBuff = null;
        int packetBytesRead = 0, expectedSize = -1;
        PackageHeader lastHeader = new PackageHeader();

        DateTime receiptStartTime; //  Not the most precise, but surely the fastest way of accomplishing this.

        /// <summary>
        /// Initiates receiving packages.
        /// </summary>
        /// <returns>False if already receiving; otherwise true.</returns>
        protected bool LowStartGettingPackets()
        {
            lock (rec_sync)
            {
                if (receiving)
                {
                    //Console.WriteLine("Already receiving!");
                    return false;
                }

                receiving = true;
            }

            packetBytesRead = 0;
            expectedSize = -1;

            try
            {
                streamReceiver.BeginRead(packetHeaderBuff, 0, packetHeaderSize, LowStartGettingPackets_Callback, null);

                //Console.WriteLine("Receipt start.");

                return true;
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, false, null);
            }

            return false;
        }

        void LowStartGettingPackets_Callback(IAsyncResult ar)
        {
            int amnt;

            //Console.WriteLine("Receipt callback.");

            try
            {
                amnt = streamReceiver.EndRead(ar);
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (System.IO.IOException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, false, null);

                return;
            }

            if (amnt == 0)
            {
                _CheckIfStopped(null);

                return;
            }

            packetBytesRead += amnt;

            __addReceived(amnt);

            if (packetBytesRead == packetHeaderSize)
            {
                Struct_mapping.ByteArrayToStructure(packetHeaderBuff, ref lastHeader);

                expectedSize = (int)lastHeader.Size + packetHeaderSize;

                if (packetPayloadBuff == null)
                    packetPayloadBuff = new byte[lastHeader.Size];
                else
                    System.Diagnostics.Debug.Assert(false, "This really should not happen.");

                receiptStartTime = DateTime.Now;

                //  Length 0 will be handled in the next conditional.
            }

            if (packetBytesRead == expectedSize)
            {
                try
                {
                    OnInternalPackageReceived(new Package(lastHeader, packetPayloadBuff) { time = DateTime.Now - receiptStartTime });
                }
                finally
                {
                    packetPayloadBuff = null;
                    //  Luckily this is just a reference!

                    lock (rec_sync)
                        receiving = false;

                    //  Should someone call the function between these two instructions, it's fine. :3

                    LowStartGettingPackets();
                }
            }
            else if (packetPayloadBuff == null)
            {
                try
                {
                    streamReceiver.BeginRead(packetHeaderBuff, packetBytesRead, packetHeaderSize - packetBytesRead, LowStartGettingPackets_Callback, ar.AsyncState);
                }
                catch (ObjectDisposedException x)
                {
                    _CheckIfStopped(x);

                    return;
                }
                catch (Exception x)
                {
                    _OnPipeFailure(x, false, null);

                    return;
                }
            }
            else
            {
                try
                {
                    streamReceiver.BeginRead(packetPayloadBuff, packetBytesRead - packetHeaderSize, expectedSize - packetBytesRead, LowStartGettingPackets_Callback, ar.AsyncState);
                }
                catch (ObjectDisposedException x)
                {
                    _CheckIfStopped(x);

                    return;
                }
                catch (Exception x)
                {
                    _OnPipeFailure(x, false, null);

                    return;
                }
            }
        }//*/
#endif
    }
}
