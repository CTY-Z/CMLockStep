using LSServer.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace LSServer
{
    public struct ProcessData
    {
        public IPEndPoint endPoint;
        public byte[] dataByte;

        public ProcessData(IPEndPoint endPoint, byte[] data)
        {
            this.endPoint = endPoint;
            this.dataByte = data;
        }
    }

    public static class ProtoHandler
    {
        private static Dictionary<int, BaseProcessor> dic_ID_process = new();

        public static void Create()
        {
            dic_ID_process.Add(1, new LoginProcessor());
        }

        public static void OnSendMsg<T>(string key, IPEndPoint endPoint, T data) where T : global::ProtoBuf.IExtensible
        {
            var value = Protocols.GetCMD(key);

            if (value.cmd == 0)
            {
                Debug.LogError("协议号不存在");
                return;
            }

            byte[] result = ProtobufHelper.Encode(value.cmd, value.param, data);
            ProcessData p = new ProcessData(endPoint, result);
            EventPool.Fire(EventDefine.SendMsg, p);
        }

        public static void OnRecvMsg(IPEndPoint endPoint, byte[] data)
        {
            var tuple = ProtobufHelper.DecodeHeader(data);
            ProcessData recvData = new ProcessData(endPoint, data);
            if(dic_ID_process.TryGetValue(tuple.cmd, out BaseProcessor processor))
            {
                Action<ProcessData> handler = processor.GetHandler(tuple.param);
                handler(recvData);
            }
            else
                Debug.LogError($"{tuple.cmd} - {tuple.param} - ProtoHandler找不到对应事件");
        }
    }
}

