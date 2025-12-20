using Login;
using LSServer.Server;
using LSServer.Utils;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LSServer
{
    public class LoginProcessor : BaseProcessor
    {
        public LoginProcessor() : base()
        {
            Add(1, C_S_ConnectRequest);
        }

        //1-1
        public static void C_S_ConnectRequest(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<ConnectRequest>(recvData.dataByte);
            NetManager.Instance.UDPServer.RegisterClient(recvData.endPoint, result);

            EventPool.Fire(ProtoStrDefine.C_S_ConnectRequest, result);
        }
        //1-2
        public static void S_C_ConnectResponse(IPEndPoint endPoint, ConnectResponse data)
        {
            ProtoHandler.OnSendMsg(ProtoStrDefine.S_C_ConnectResponse, endPoint, data);
        }

        //1-3
        public static void C_S_HeartBeat(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<Heartbeat>(recvData.dataByte);
            EventPool.Fire(ProtoStrDefine.S_C_HeartBeat, result);


        }

        //1-4
        public static void S_C_Heartbeat(IPEndPoint endPoint, Heartbeat data)
        {
            ProtoHandler.OnSendMsg(ProtoStrDefine.C_S_HeartBeat, endPoint, data);
        }
    }
}
