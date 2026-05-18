using FrameSync;
using Login;
using LSServer.Client;
using LSServer.Model;
using LSServer.Utils;
using System;
using System.Collections.Generic;
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

        private GameModel m_gameModel;
        private Dictionary<int, Dictionary<int, FrameSync.PlayerInput>> dic_frameCount_frameInputData = new();
        private Dictionary<int, FrameSync.PlayerInput> dic_clientID_lastInput = new();

        private int m_frameCount = 0;
        private bool m_running = true;

        public void Start()
        {
            m_gameModel = ModelManager.Instance.game;

            Console.WriteLine("启动UDP服务端...");
            m_server = new UdpClient(port);
            Console.WriteLine($"UDP服务器监听端口: {port}");

            Thread receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Thread frameThread = new Thread(FrameSyncLoop);
            frameThread.IsBackground = true;
            frameThread.Start();

            EventPool.Register<ProcessData>(EventDefine.SendMsg, SendToClient);

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
                    if (m_gameModel.GetClientCount() == 0)
                        continue;

                    var frameInput = new FrameSync.FrameInput
                    {
                        FrameNumber = m_frameCount,
                    };


                    dic_frameCount_frameInputData.TryGetValue(m_frameCount, out var dic_id_input);

                    foreach (var client in m_gameModel.dic_client_info.Values)
                    {
                        if (dic_id_input != null && dic_id_input.TryGetValue(client.ClientID, out var input))
                            frameInput.Inputs.Add(input);
                        else
                        {
                            dic_clientID_lastInput.TryGetValue(client.ClientID, out var lastInput);
                            if (lastInput == null)
                                input = new FrameSync.PlayerInput { PlayerId = client.ClientID, TargetFrame = m_frameCount, 
                                    InputX = 0, InputY = 0, Jump = false };
                            else
                                input = new FrameSync.PlayerInput { PlayerId = client.ClientID, TargetFrame = m_frameCount,
                                    InputX = lastInput.InputX, InputY = lastInput.InputY, Jump = lastInput.Jump };

                            frameInput.Inputs.Add(input);
                        }
                        dic_clientID_lastInput[client.ClientID] = input;
                    }

                    dic_frameCount_frameInputData.Remove(m_frameCount);

                    foreach (var client in m_gameModel.dic_client_info.Values)
                        FrameSyncProcessor.S_C_FrameData(client.endPoint, frameInput);

                    if (m_frameCount % 30 == 0)
                        Console.WriteLine($"已广播 {m_frameCount} 帧，客户端数: {m_gameModel.GetClientCount()}");
                }
            }
        }

        public void Stop()
        {
            m_gameModel = null;

            EventPool.Remove<ProcessData>(EventDefine.SendMsg, SendToClient);

            m_running = false;
            m_server?.Close();
            Console.WriteLine("服务器已停止");
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
                m_gameModel.RemoveClient(processData.endPoint);
            }
        }

        public void RegisterInput(int clientID, FrameSync.PlayerInput inputData)
        {
            lock (m_clientLock)
            {
                // 如果输入的帧数小于当前服务器帧数，说明是过期输入，直接丢弃
                if(inputData.TargetFrame <= m_frameCount) return; 

                if (!dic_frameCount_frameInputData.TryGetValue(inputData.TargetFrame, out var frameInputData))
                {
                    frameInputData = new();
                    dic_frameCount_frameInputData[inputData.TargetFrame] = frameInputData;
                }

                frameInputData[clientID] = inputData;
                Console.WriteLine($"记录玩家 {clientID} 输入: X={inputData.InputX}, Y={inputData.InputY}, Frame={inputData.TargetFrame}");
            }
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
