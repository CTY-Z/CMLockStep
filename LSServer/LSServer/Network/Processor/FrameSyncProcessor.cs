using FrameSync;
using Login;
using LSServer.Utils;
using System.Collections;
using System.Collections.Generic;

namespace LSServer
{
    public class FrameSyncProcessor : BaseProcessor
    {
        public FrameSyncProcessor() : base()
        {
            Add(1, C_S_FrameData);
        }

        //2-1
        public static void C_S_FrameData(ProcessData recvData)
        {
            var result = ProtobufHelper.DecodeData<FrameSync.PlayerInput>(recvData.dataByte);
            Debug.Log($"收到由{result.PlayerId}发出的第{result.FrameIdx}帧");
            EventPool.Fire(EventDefine.C_S_FrameData, result);
        }

        //2-2
        public static void S_C_FrameData(byte[] dataByte)
        {
            
        }
    }
}
