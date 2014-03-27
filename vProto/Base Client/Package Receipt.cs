using System;
using System.IO;

#if NETFX_CORE
using Windows.Foundation;
using Windows.System.Threading;
#endif

namespace vProto
{
    using Internals;
    using Packages;

    partial class BaseClient
    {
        const int packageHeaderSize = PackageHeader.StructSize;



#if RECEIVER_THREAD
        protected bool LowStartGettingPackages()
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
            byte[] packageHeaderBuff = new byte[packageHeaderSize], packagePayloadBuff = null;
            int packageBytesRead = 0, expectedSize = -1, amnt = 0;
            PackageHeader lastHeader = new PackageHeader();

            while (!Disposed)
            {
                packageBytesRead = 0;
                expectedSize = -1;
                amnt = 0;
                packagePayloadBuff = null;

                while (true)
                {
                    if (packagePayloadBuff == null)
                        //  No payload buffer means we're reading a header.
                        try
                        {
                            //Console.WriteLine("HEADER Receipt start.");

                            //stream.BeginRead(packageHeaderBuff, 0, packageHeaderSize, LowStartGettingPackages_Callback, null);
                            //amnt = stream.Read(packageHeaderBuff, 0, packageHeaderSize);
                            amnt = streamOut.Read(packageHeaderBuff, packageBytesRead, packageHeaderSize - packageBytesRead);
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

                            //stream.BeginRead(packagePayloadBuff, packageBytesRead - packageHeaderSize, expectedSize - packageBytesRead, LowStartGettingPackages_Callback, ar.AsyncState);
                            amnt = streamOut.Read(packagePayloadBuff, packageBytesRead - packageHeaderSize, expectedSize - packageBytesRead);
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

                    packageBytesRead += amnt;

                    __addReceived(amnt);

                    if (packageBytesRead == packageHeaderSize)
                    {
                        Struct_mapping.ByteArrayToStructure(packageHeaderBuff, ref lastHeader);
                        uint len = lastHeader.Size;

                        expectedSize = (int)len + packageHeaderSize;

                        packagePayloadBuff = new byte[len];
                    }

                    if (packageBytesRead == expectedSize)
                    {
                        try
                        {
                            OnInternalPackageReceived(new Package(lastHeader, packagePayloadBuff));
                        }
                        finally
                        {
                            packagePayloadBuff = null;

                            //  Getting ready to receive the next package header here.
                        }

                        break;
                    }
                }
            }
        }//*/

#else

        object rec_sync = new object();
        bool receiving = false;

        byte[] packageHeaderBuff = new byte[packageHeaderSize], packagePayloadBuff = null;
        int packageBytesRead = 0, expectedSize = -1;
        PackageHeader lastHeader = new PackageHeader();

        DateTime receiptStartTime; //  Not the most precise, but surely the fastest way of accomplishing this.

        /// <summary>
        /// Initiates receiving packages.
        /// </summary>
        /// <returns>False if already receiving; otherwise true.</returns>
        protected bool LowStartGettingPackages()
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

            packageBytesRead = 0;
            expectedSize = -1;

            try
            {
#if NETFX_CORE
                streamReceiver.ReadAsync(packageHeaderBuff, 0, packageHeaderSize).AsAsyncOperation().Completed = LowStartGettingPackages_Callback;
#else
                streamReceiver.BeginRead(packageHeaderBuff, 0, packageHeaderSize, LowStartGettingPackages_Callback, null);
#endif

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

#if NETFX_CORE
        void LowStartGettingPackages_Callback(IAsyncOperation<int> asyncInfo, AsyncStatus asyncStatus)
#else
        void LowStartGettingPackages_Callback(IAsyncResult ar)
#endif
        {
            int amnt = 0;

            //Console.WriteLine("Receipt callback.");

#if NETFX_CORE
            if (asyncInfo.Status == AsyncStatus.Error)
            {
                if (asyncInfo.ErrorCode is ObjectDisposedException || asyncInfo.ErrorCode is IOException)
                {
                    _CheckIfStopped(asyncInfo.ErrorCode);

                    return;
                }

                _OnPipeFailure(asyncInfo.ErrorCode, false, null);

                return;
            }
            else if (asyncInfo.Status == AsyncStatus.Completed)
                amnt = asyncInfo.GetResults();
#else
            try
            {
                amnt = streamReceiver.EndRead(ar);
            }
            catch (ObjectDisposedException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (IOException x)
            {
                _CheckIfStopped(x);

                return;
            }
            catch (Exception x)
            {
                _OnPipeFailure(x, false, null);

                return;
            }
#endif

            if (amnt == 0)
            {
                _CheckIfStopped(null);

                return;
            }

            packageBytesRead += amnt;

            __addReceived(amnt);

            if (packageBytesRead == packageHeaderSize)
            {
                lastHeader.Buffer = packageHeaderBuff;

                expectedSize = (int)lastHeader.Size + packageHeaderSize;

                if (packagePayloadBuff == null)
                    packagePayloadBuff = new byte[lastHeader.Size];
                else
                    System.Diagnostics.Debug.Assert(false, "This really should not happen.");

                receiptStartTime = DateTime.Now;

                //  Length 0 will be handled in the next conditional.
            }

            if (packageBytesRead == expectedSize)
            {
                try
                {
                    OnInternalPackageReceived(new Package(lastHeader, packagePayloadBuff) { time = DateTime.Now - receiptStartTime });
                }
                finally
                {
                    packagePayloadBuff = null;
                    //  Luckily this is just a reference!

                    lock (rec_sync)
                        receiving = false;

                    //  Should someone call the function between these two instructions, it's fine. :3

                    LowStartGettingPackages();
                }
            }
            else if (packagePayloadBuff == null)
            {
                try
                {
#if NETFX_CORE
                    streamReceiver.ReadAsync(packageHeaderBuff, packageBytesRead, packageHeaderSize - packageBytesRead).AsAsyncOperation().Completed = LowStartGettingPackages_Callback;
#else
                    streamReceiver.BeginRead(packageHeaderBuff, packageBytesRead, packageHeaderSize - packageBytesRead, LowStartGettingPackages_Callback, ar.AsyncState);
#endif
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
#if NETFX_CORE
                    streamReceiver.ReadAsync(packagePayloadBuff, packageBytesRead - packageHeaderSize, expectedSize - packageBytesRead).AsAsyncOperation().Completed = LowStartGettingPackages_Callback;
#else
                    streamReceiver.BeginRead(packagePayloadBuff, packageBytesRead - packageHeaderSize, expectedSize - packageBytesRead, LowStartGettingPackages_Callback, ar.AsyncState);
#endif
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
