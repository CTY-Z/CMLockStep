using Login;
using LSServer.Client;
using LSServer.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LSServer.Server
{
    internal class UDPServer
    {
        private UdpClient m_server;
        private int port = 8888;
        private object m_clientLock = new object();

        private Dictionary<IPEndPoint, UDPClient> dic_client_info = new();
        private Dictionary<int, string> dic_id_input = new();

        private int m_frameCount = 0;
        private bool m_running = true;

        public void Start()
        {
            Console.WriteLine("启动UDP服务端...");
            m_server = new UdpClient(port);
            Console.WriteLine($"UDP服务器监听端口: {port}");

            Thread receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Thread frameThread = new Thread(FrameSyncLoop);
            frameThread.IsBackground = true;
            frameThread.Start();

            EventPool.Register<ProcessData>(ProtoStrDefine.SendMsg, SendToClient);

            Console.WriteLine("按任意键停止服务器...");
            Console.ReadKey();
            Stop();
        }

        void FrameSyncLoop()
        {
            while (m_running)
            {
                Thread.Sleep(33); // 30帧/秒
                m_frameCount++;

                lock (m_clientLock)
                {
                    if (dic_client_info.Count == 0)
                        continue;

                    // 构建帧数据
                    StringBuilder frameData = new StringBuilder();
                    frameData.Append($"frame|{m_frameCount}|");

                    foreach (var input in dic_id_input)
                        frameData.Append($"P{input.Key}:{input.Value};");

                    // 广播给所有客户端
                    foreach (var client in dic_client_info.Values)
                        //SendToClient(client.endPoint, frameData.ToString()); todo

                    // 清空本帧输入
                    dic_id_input.Clear();

                    if (m_frameCount % 30 == 0)
                        Console.WriteLine($"已广播 {m_frameCount} 帧，客户端数: {dic_client_info.Count}");
                }
            }
        }

        public void Stop()
        {
            EventPool.Remove<ProcessData>(ProtoStrDefine.SendMsg, SendToClient);

            m_running = false;
            m_server?.Close();
            Console.WriteLine("服务器已停止");
        }

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

        public void SendToClient(ProcessData processData)
        {
            try
            {
                m_server.Send(processData.dataByte, processData.dataByte.Length, processData.endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送到 {processData.endPoint} 失败: {ex.Message}");
                RemoveClient(processData.endPoint);
            }
        }

        public void RegisterInput(int clientID, string inputData)
        {
            dic_id_input[clientID] = inputData;
            Console.WriteLine($"记录玩家 {clientID} 输入: {inputData}");
        }



        void ReceiveLoop()
        {
            try
            {
                while (m_running)
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = m_server.Receive(ref clientEndPoint);

                    // 处理消息
                    ProcessMsg(clientEndPoint, data);
                }
            }
            catch (Exception ex)
            {
                if (m_running)
                    Console.WriteLine($"接收错误: {ex.Message}");
            }
        }

        private void ProcessMsg(IPEndPoint endPoint, byte[] data)
        {
            //Console.WriteLine($"收到来自 {endPoint}: {msg}");
            lock (m_clientLock)
            {
                //if (!dic_client_info.ContainsKey(endPoint))
                //{
                //    // 新客户端连接
                //    RegisterClient(endPoint);
                //    return;
                //}
                ProtoHandler.OnRecvMsg(endPoint, data);
            }
        }
    }
}
