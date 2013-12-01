using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto
{
    using Internal_Utilities;
    using Events;
    using Packages;

    using StateType = Packages.Package;
    
    partial class BaseClient
    {
        protected virtual void OnInternalPacketReceived(StateType pack)
        {
            switch (pack.Header.Type)
            {
                case PackageType.Request:
                    OnInternalRequestReceived(pack);
                    break;
                case PackageType.Response:
                    OnInternalResponseReceived(pack);
                    break;

                case PackageType.Data:
                    OnDataReceived(new Events.DataReceivedEventArgs(pack.Payload, pack.Header.ID));
                    break;

                case PackageType.HeartbeatRequest:
                    OnInternalHeartbeatRequestReceived(pack);
                    break;
                case PackageType.HeartbeatResponse:
                    OnInternalHeartbeatResponseReceived(pack);
                    break;
            }
        }

        protected virtual void OnInternalPacketSent(StateType pack)
        {
            switch (pack.Header.Type)
            {
                case PackageType.Request:
                    OnInternalRequestSent(pack);
                    break;
                case PackageType.Response:
                    OnInternalResponseSent(pack);
                    break;
            }
        }


        private void _OnPipeFailure(Exception x, bool outg, StateType pack)
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
                            OnInternalRequestSendFailed(pack);
                            break;
                        case PackageType.Response:
                            OnInternalResponseSendFailed(pack);
                            break;

                        case PackageType.HeartbeatRequest:
                        case PackageType.HeartbeatResponse:
                            OnInternalHeartbeatFailure(pack);
                            break;
                    }
                }
            }//*/

            _CheckIfStopped(x);
        }
    }
}
