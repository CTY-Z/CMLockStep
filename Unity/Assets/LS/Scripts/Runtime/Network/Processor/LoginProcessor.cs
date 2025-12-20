using Login;
using System.Xml.Linq;
using UnityEngine;

namespace LS
{
    public class LoginProcessor : BaseProcessor
    {
        public LoginProcessor() : base()
        {
            Add(2, S_C_ConnectResponse);
        }

        //1-1
        public static void C_S_ConnectRequest()
        {
            var package = new ConnectRequest
            {
                PlayerName = "client_1",
                IsConnect = true,
            };

            ProtoHandler.OnSendMsg(ProtoStrDefine.C_S_ConnectRequest, package);
        }
        //1-2
        public static void S_C_ConnectResponse(byte[] data)
        {
            var result = ProtobufHelper.DecodeData<ConnectResponse>(data);
            Debug.Log($"服务器连接成功 - 服务器ID : {result.ClientId}");
            GameEntry.Instance.eventPool.Fire(ProtoStrDefine.S_C_ConnectResponse, result);
        }

        //1-3
        public static void C_S_HeartBeat()
        {
            var package = new Heartbeat
            {
                Str = "ping",
            };

            ProtoHandler.OnSendMsg(ProtoStrDefine.C_S_HeartBeat, package);
        }

        //1-4
        public static void S_C_Heartbeat(byte[] data)
        {
            var result = ProtobufHelper.DecodeData<Heartbeat>(data);
            GameEntry.Instance.eventPool.Fire(ProtoStrDefine.S_C_HeartBeat, result);
        }
    }
}
