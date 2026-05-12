using Login;
using LSServer.Client;
using System;
using System.Net;
using System.Security.Cryptography;

namespace LSServer.Model
{
    internal class GameModel
    {
        private const int MinClientId = 1000;
        private const int MaxClientId = 9999;

        private object m_clientLock = new object();

        public Dictionary<IPEndPoint, UDPClient> dic_client_info = new();
        private HashSet<int> usedClientIds = new HashSet<int>();

        public void RegisterClient(IPEndPoint endPoint, ConnectRequest data)
        {
            lock (m_clientLock)
            {
                if (dic_client_info.TryGetValue(endPoint, out UDPClient? oldClient) && oldClient != null)
                {
                    usedClientIds.Remove(oldClient.ClientID);
                }

                int clientId = AllocateClientId();
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

        private int AllocateClientId()
        {
            int capacity = MaxClientId - MinClientId + 1;
            if (usedClientIds.Count >= capacity)
                throw new InvalidOperationException("No available client IDs.");

            for (int i = 0; i < capacity; i++)
            {
                int clientId = RandomNumberGenerator.GetInt32(MinClientId, MaxClientId + 1);
                if (usedClientIds.Add(clientId))
                    return clientId;
            }

            for (int clientId = MinClientId; clientId <= MaxClientId; clientId++)
            {
                if (usedClientIds.Add(clientId))
                    return clientId;
            }

            throw new InvalidOperationException("No available client IDs.");
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
                    usedClientIds.Remove(clientId);
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
