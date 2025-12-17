using LSServer.Client;
using LSServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LSServer.Server
{
    internal class TCPServer
    {
        static List<TCPClient> list_client = new();
        static Dictionary<int, string> dic_client_input = new();
        static int m_frameCount = 0;

        public void Start()
        {
            Debug.Log("启动服务端");

            int port = 8888;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Debug.Log($"服务器监听端口{port}");

            Thread frameThread = new Thread(FrameSyncLoop);
            frameThread.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Debug.Log($"新客户端连接: {client.Client.RemoteEndPoint}");

                TCPClient handler = new TCPClient(client, list_client.Count + 1);
                list_client.Add(handler);
                Thread clientThread = new Thread(handler.HandleClient);
                clientThread.Start();
            }
        }

        static void FrameSyncLoop()
        {
            while (true)
            {
                Thread.Sleep(33);
                m_frameCount++;

                lock (dic_client_input)
                {
                    string frameData = $"frame|{m_frameCount}|";

                    foreach (var inputKV in dic_client_input)
                        frameData += $"P{inputKV.Key} : {inputKV.Value}";

                    BroadcastToAll(frameData);
                    dic_client_input.Clear();
                }

                if (m_frameCount % 30 == 0)
                    Debug.Log($"已经处理{m_frameCount}帧");
            }
        }

        static void BroadcastToAll(string message)
        {
            foreach (var client in list_client)
            {
                if (client.IsConnented)
                    client.SendMsg(message);
            }
        }

        public static void RecordInput(int clientID, string input)
        {
            lock (dic_client_input)
            {
                dic_client_input[clientID] = input;
            }
        }
    }
}
