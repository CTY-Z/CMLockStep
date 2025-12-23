using Login;
using LSServer.Model;
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
                ModelManager.Instance.game.RegisterClient(recvData.endPoint, result);
            else
                ModelManager.Instance.game.RemoveClient(recvData.endPoint);

            EventPool.Fire(EventDefine.C_S_ConnectRequest, result);
        }
        //1-2
        public static void S_C_ConnectResponse(IPEndPoint endPoint, ConnectResponse data)
        {
            ProtoHandler.OnSendMsg(EventDefine.S_C_ConnectResponse, endPoint, data);
        }

        //1-3
        public static void C_S_Heartbeat(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<Heartbeat>(recvData.dataByte);
            //Debug.Log($"ÐÄÌø: - {result.Str}");
            var data = new Heartbeat { Str = "pong" };
            S_C_Heartbeat(recvData.endPoint, data);
            EventPool.Fire(EventDefine.C_S_Heartbeat, result);
        }

        //1-4
        public static void S_C_Heartbeat(IPEndPoint endPoint, Heartbeat data)
        {
            ProtoHandler.OnSendMsg(EventDefine.S_C_Heartbeat, endPoint, data);
        }
    }
}
