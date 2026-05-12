using FrameSync;
using Login;
using LSServer.Model;
using LSServer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace LSServer
{
    public class FrameSyncProcessor : BaseProcessor
    {
        public FrameSyncProcessor() : base()
        {
            Add(1, C_S_FrameData);
        }

        //2-1 client -> server
        public static void C_S_FrameData(ProcessData recvData)
        {
            if (!ModelManager.Instance.game.dic_client_info.TryGetValue(recvData.endPoint, out var clientData)) return;

            var result = ProtobufHelper.DecodeData<FrameSync.PlayerInput>(recvData.dataByte);
            result.PlayerId = clientData.ClientID;
            Debug.Log($"收到玩家{result.PlayerId}的输入");
            EventPool.Fire(EventDefine.C_S_FrameData, result);

            NetManager.Instance.UDPServer.RegisterInput(result.PlayerId, result);
        }

        //2-2 server -> client
        public static void S_C_FrameData(IPEndPoint endPoint, FrameSync.FrameInput data)
        {
            ProtoHandler.OnSendMsg(EventDefine.S_C_FrameData, endPoint, data);
        }
    }
}
