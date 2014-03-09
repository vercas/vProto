using System;

namespace vProto
{
    using Events;
    using Packages;

    partial class BaseClient
    {
        /// <summary>
        /// Invoked when receiving any package from the other side.
        /// </summary>
        /// <param name="pack">Package received.</param>
        protected virtual void OnInternalPackageReceived(Package pack)
        {
            switch (pack.Header.Type)
            {
                case PackageType.Request:
                case PackageType.InternalRequest:
                    OnInternalRequestReceived(pack);
                    break;

                case PackageType.Response:
                case PackageType.InternalResponse:
                    OnInternalResponseReceived(pack);
                    break;

                case PackageType.Data:
                    OnDataReceived(new DataReceivedEventArgs(pack.Payload, pack.Header.ID));
                    break;

                case PackageType.HeartbeatRequest:
                    OnInternalHeartbeatRequestReceived(pack);
                    break;

                case PackageType.HeartbeatResponse:
                    OnInternalHeartbeatResponseReceived(pack);
                    break;

                case PackageType.PeerConnected:
                    OnInternalPeerConnectedReceived(pack);
                    break;
                case PackageType.PeerDisconnected:
                    OnInternalPeerDisconnectedReceived(pack);
                    break;
            }
        }

        /// <summary>
        /// Invoked when successfully sending any package to the other side.
        /// </summary>
        /// <param name="pack">Package sent.</param>
        protected virtual void OnInternalPackageSent(Package pack)
        {
            switch (pack.Header.Type)
            {
                case PackageType.Request:
                case PackageType.InternalRequest:
                    OnInternalRequestSent(pack);
                    break;

                case PackageType.Response:
                case PackageType.InternalResponse:
                    OnInternalResponseSent(pack);
                    break;
            }
        }


        private void _OnPipeFailure(Exception x, bool outg, Package pack)
        {
            try
            {//*/
                OnPipeFailure(new PipeFailureEventArgs(x, outg));
            }
            finally
            {
                if (outg)
                {
                    switch (pack.Header.Type)
                    {
                        case PackageType.Request:
                        case PackageType.InternalRequest:
                            OnInternalRequestSendFailed(pack, x);
                            break;

                        case PackageType.Response:
                        case PackageType.InternalResponse:
                            OnInternalResponseSendFailed(pack, x);
                            break;

                        case PackageType.HeartbeatRequest:
                        //case PackageType.HeartbeatResponse:
                            OnInternalHeartbeatFailure(pack, x);
                            break;
                    }
                }
                else if (pack != null)
                {
                    switch (pack.Header.Type)
                    {
                        case PackageType.Response:
                            OnInternalResponseReceiveFailed(pack, x);
                            break;
                    }
                }
            }//*/

            _CheckIfStopped(x);
        }
    }
}
