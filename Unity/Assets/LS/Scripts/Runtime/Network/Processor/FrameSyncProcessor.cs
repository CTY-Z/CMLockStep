using FrameSync;
using Login;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public class FrameSyncProcessor : BaseProcessor
    {
        public FrameSyncProcessor() : base()
        {
            Add(2, S_C_FrameData);
        }

        //2-1
        public static void C_S_FrameData(int frameCount, float inputX, float inputY)
        {
            var data = new FrameSync.PlayerInput
            {
                PlayerId = GameEntry.Instance.model.login.clientID,
                FrameIdx = frameCount,
                InputX = inputX, InputY = inputY
            };
            ProtoHandler.OnSendMsg(EventDefine.C_S_FrameData, data);
        }

        //2-2
        public static void S_C_FrameData(byte[] dataByte)
        {
            var result = ProtobufHelper.DecodeData<FrameSync.PlayerInput>(dataByte);
            GameEntry.Instance.eventPool.Fire(EventDefine.S_C_FrameData, result);
        }

    }
}
