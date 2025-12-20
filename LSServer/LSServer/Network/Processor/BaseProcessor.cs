using FrameSync;
using LSServer.Utils;
using System;

namespace LSServer
{
    public struct ProtocolMessage
    {
        public string Key { get; set; }
        public string Raw { get; set; }
        public Dictionary<string, string> Args { get; set; }
        public byte[] RawBytes { get; set; }
        public DateTime ReceivedAt { get; set; }
    }

    public class BaseProcessor
    {
        protected Dictionary<int, Action<ProcessData>> dic_key_handler;

        public BaseProcessor()
        {
            dic_key_handler = new();
        }

        protected void Add(int key, Action<ProcessData> handler)
        {
            if (dic_key_handler.ContainsKey(key))
                Debug.LogError("协议注册重复");

            dic_key_handler[key] = handler;
        }

        public Action<ProcessData> GetHandler(int key)
        {
            if (dic_key_handler.ContainsKey(key))
                return dic_key_handler[key];

            return null;
        }
    }
}

