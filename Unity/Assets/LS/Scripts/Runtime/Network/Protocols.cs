using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public static class Protocols
    {
        public readonly static Dictionary<string, (UInt16 cmd, UInt16 param)> dic_protoName_cmd = new()
        {
            ["C_S_ConnectRequest"] = (1, 1),
            ["S_C_ConnectResponse"] = (1, 2),
        };

        public static (UInt16 cmd, UInt16 param) GetCMD(string key)
        {
            if(dic_protoName_cmd.TryGetValue(key, out var value))
                return (value.cmd, value.param);

            return (0, 0);
        }

    }
}
