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
            Add(4, S_C_Heartbeat);
        }

        //1-1
        public static void C_S_ConnectRequest(ConnectRequest data)
        {
            ProtoHandler.OnSendMsg(ProtoStrDefine.C_S_ConnectRequest, data);
        }
        //1-2
        public static void S_C_ConnectResponse(byte[] data)
        {
            var result = ProtobufHelper.DecodeData<ConnectResponse>(data);

            if (result.Success)
                Debug.Log($"服务器连接成功 - 服务器ID : {result.ClientId} - {result.Message}");
            else
                Debug.Log($"服务器断开连接 - 服务器ID : {result.ClientId} - {result.Message}");

            GameEntry.Instance.model.login.SetConnectData(result);
            GameEntry.Instance.eventPool.Fire(ProtoStrDefine.S_C_ConnectResponse, result);
        }

        //1-3
        public static void C_S_HeartBeat()
        {
            var data = new Heartbeat { Str = "ping" };
            ProtoHandler.OnSendMsg(ProtoStrDefine.C_S_HeartBeat, data);
        }

        //1-4
        public static void S_C_Heartbeat(byte[] data)
        {
            var result = ProtobufHelper.DecodeData<Heartbeat>(data);
            Debug.Log($"心跳: - {result.Str}");
            GameEntry.Instance.eventPool.Fire(ProtoStrDefine.S_C_HeartBeat, result);
        }
    }
}
