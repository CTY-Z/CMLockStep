using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using static UnityEngine.Rendering.ReloadAttribute;

namespace LS
{
    public static class ProtoHandler
    {
        private static Dictionary<int, BaseProcessor> dic_ID_process = new();

        public static void Create()
        {
            dic_ID_process.Add(1, new LoginProcessor());
        }

        public static void OnSendMsg<T>(string key, T data) where T : global::ProtoBuf.IExtensible
        {
            var value = Protocols.GetCMD(key);

            if (value.cmd == 0)
            {
                Debug.LogError("协议号不存在");
                return;
            }

            byte[] result = ProtobufHelper.Encode(value.cmd, value.param, data);
            GameEntry.Instance.eventPool.Fire(EventDefine.SendMsg, result);
        }
    }
}

