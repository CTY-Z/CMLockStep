using System.Collections;
using System.Collections.Generic;

namespace LSServer
{
    public static class EventDefine
    {
        public static readonly string SendMsg = "SendMsg";

        public static readonly string C_S_ConnectRequest = "C_S_ConnectRequest";
        public static readonly string S_C_ConnectResponse = "S_C_ConnectResponse";

        public static readonly string S_C_Heartbeat = "S_C_Heartbeat";
        public static readonly string C_S_Heartbeat = "C_S_Heartbeat";
    }
}


