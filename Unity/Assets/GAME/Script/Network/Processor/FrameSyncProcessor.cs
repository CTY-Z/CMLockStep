using LS;
using UnityEngine;

public class FrameSyncProcessor : BaseProcessor
{
    public FrameSyncProcessor() : base()
    {
        Add(2, S_C_FrameData);
    }

    // 2-1 client -> server
    public static void C_S_FrameData(int targetFrame, float inputX, float inputY, bool jump)
    {
        int clientId = GameEntry.Instance.model.login.clientID;
        if (clientId < 0)
        {
            Debug.Log($"[FrameSync] Client is not connected, skip input for frame {targetFrame}");
            return;
        }

        var data = new FrameSync.PlayerInput
        {
            PlayerId = clientId,
            TargetFrame = targetFrame,
            InputX = inputX,
            InputY = inputY,
            Jump = jump,
        };

        ProtoHandler.OnSendMsg(EventDefine.C_S_FrameData, data);
    }

    // 2-2 server -> client
    public static void S_C_FrameData(byte[] dataByte)
    {
        var result = ProtobufHelper.DecodeData<FrameSync.FrameInput>(dataByte);
        GameEntry.Instance.eventPool.Fire(EventDefine.S_C_FrameData, result);
    }
}
