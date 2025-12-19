using Login;
using System.Xml.Linq;

namespace LS
{
    public class LoginProcessor : BaseProcessor
    {
        public LoginProcessor() : base()
        {
            Add(2, S_C_ConnectResponse);
        }

        //1-1
        public static void C_S_ConnectRequest()
        {
            var package = new ConnectRequest
            {
                PlayerName = "client_1",
                IsConnect = true,
            };

            ProtoHandler.OnSendMsg("C_S_ConnectRequest", package);
        }
        //1-2
        public static void S_C_ConnectResponse()
        {
            var package = new ConnectResponse
            {
                Success = true,
                ClientId = 0,
                Message = ""
            };

            GameEntry.Instance.eventPool.Fire(EventDefine.S_C_ConnectResponse, package);
        }

        //1-3
        public static void C_S_HeartBeat()
        {
            var package = new Heartbeat
            {
                Str = "ping",
            };

            ProtoHandler.OnSendMsg("C_S_HeartBeat", package);
        }

        //1-4
        public static void S_C_HeartBeat()
        {
            var package = new Heartbeat
            {
                Str = "ping",
            };

            GameEntry.Instance.eventPool.Fire(EventDefine.S_C_HeartBeat, package);
        }
    }
}
