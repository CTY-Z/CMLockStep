using System;
using System.Collections;
using System.Collections.Generic;

namespace LSServer
{
    public static class Protocols
    {
        public readonly static Dictionary<string, (UInt16 cmd, UInt16 param)> dic_protoName_cmd = new()
        {
            [ProtoStrDefine.C_S_ConnectRequest] = (1, 1),
            [ProtoStrDefine.S_C_ConnectResponse] = (1, 2),
        };

        public readonly static Dictionary<int, string> dic_ID_eventKey = new()
        {
            [1 * 256 + 1] = ProtoStrDefine.C_S_ConnectRequest,
            [1 * 256 + 2] = ProtoStrDefine.S_C_ConnectResponse,
        };

        public static (UInt16 cmd, UInt16 param) GetCMD(string key)
        {
            if (dic_protoName_cmd.TryGetValue(key, out var value))
                return (value.cmd, value.param);

            return (0, 0);
        }

        public static string GetProtoStr(UInt16 cmd, UInt16 param)
        {
            int ID = cmd * 256 + param;

            if (dic_ID_eventKey.TryGetValue(ID, out string protoStr))
                return protoStr;

            return "";
        }
    }
}
