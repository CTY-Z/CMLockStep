using Login;
using LSServer.Client;
using System.Net;

namespace LSServer.Model
{
    internal class GameModel
    {
        private object m_clientLock = new object();

        public Dictionary<IPEndPoint, UDPClient> dic_client_info = new();

        public void RegisterClient(IPEndPoint endPoint, ConnectRequest data)
        {
            lock (m_clientLock)
            {
                int clientId = dic_client_info.Count + 1;
                dic_client_info[endPoint] = new UDPClient(clientId, endPoint, data);

                ConnectResponse temp = new ConnectResponse()
                {
                    ClientId = clientId,
                    Success = true,
                    Message = "connect",
                };
                LoginProcessor.S_C_ConnectResponse(endPoint, temp);
                Console.WriteLine($"客户端注册: {endPoint} -> ID: {clientId}");
            }
        }

        public void RemoveClient(IPEndPoint endPoint)
        {
            lock (m_clientLock)
            {
                if (dic_client_info.ContainsKey(endPoint))
                {
                    UDPClient client = dic_client_info[endPoint];

                    ConnectResponse temp = new ConnectResponse()
                    {
                        ClientId = client.ClientID,
                        Success = false,
                        Message = "disconnect",
                    };
                    LoginProcessor.S_C_ConnectResponse(endPoint, temp);

                    int clientId = client.ClientID;
                    dic_client_info.Remove(endPoint);
                    Console.WriteLine($"客户端 {clientId} ({endPoint}) 断开连接");
                }
            }
        }

        public int GetClientCount()
        {
            return dic_client_info.Count;
        }
    }
}
