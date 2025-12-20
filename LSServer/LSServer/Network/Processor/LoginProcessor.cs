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
            Add(3, C_S_Heartbeat);
        }

        //1-1
        public static void C_S_ConnectRequest(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<ConnectRequest>(recvData.dataByte);
            if (result.IsConnect)
                NetManager.Instance.UDPServer.RegisterClient(recvData.endPoint, result);
            else
                NetManager.Instance.UDPServer.RemoveClient(recvData.endPoint);

            EventPool.Fire(ProtoStrDefine.C_S_ConnectRequest, result);
        }
        //1-2
        public static void S_C_ConnectResponse(IPEndPoint endPoint, ConnectResponse data)
        {
            ProtoHandler.OnSendMsg(ProtoStrDefine.S_C_ConnectResponse, endPoint, data);
        }

        //1-3
        public static void C_S_Heartbeat(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<Heartbeat>(recvData.dataByte);
            var data = new Heartbeat { Str = "pong" };
            S_C_Heartbeat(recvData.endPoint, data);
            EventPool.Fire(ProtoStrDefine.C_S_Heartbeat, result);
        }

        //1-4
        public static void S_C_Heartbeat(IPEndPoint endPoint, Heartbeat data)
        {
            ProtoHandler.OnSendMsg(ProtoStrDefine.S_C_Heartbeat, endPoint, data);
        }
    }
}
