using Cysharp.Threading.Tasks;
using Login;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace LS
{
    public class UDPClient : MonoBehaviour
    {
        [Header("连接设置")]
        public string serverIP = "127.0.0.1";
        public int serverPort = 8888;

        [Header("消息设置")]
        public bool showLog = false;

        [Header("性能设置")]
        public int receiveBufferSize = 8192;
        public int sendBufferSize = 8192;
        public int currentFrame = 0;

        private Socket m_socket;
        private EndPoint m_serverEndPoint;
        private byte[] receiveBuffer;

        private ConcurrentQueue<byte[]> que_receive = new();
        private ConcurrentQueue<byte[]> que_send = new();

        private Thread m_receiveThread;
        private Thread m_sendThread;
        private bool m_running = false;

        private string m_currentInput;

        private AutoResetEvent sendEvent = new AutoResetEvent(false);

        private void OnEnable()
        {
            AddListener();
        }

        private void OnDisable()
        {
            RemoveListener();
        }

        private void Start()
        {
            InitializeSocket();
        }

        private void Update()
        {
            while (que_receive.TryDequeue(out byte[] data))
                ProcessMsg(data);

            //CollectInput();

            if (Time.frameCount % 2 == 0)
            {
                //SendInput();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnApplicationQuit()
        {
            AfterDisconnect();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"当前帧: {currentFrame}");
            GUILayout.Label($"当前输入: {m_currentInput}");

            if (GUILayout.Button("发送测试消息"))
                LoginProcessor.C_S_HeartBeat();

            if (GUILayout.Button("断开连接"))
                Disconnect();

            GUILayout.EndArea();
        }

        private void AddListener()
        {
            GameEntry.Instance.eventPool.Register<byte[]>(EventDefine.SendMsg, SendMsg);
            GameEntry.Instance.eventPool.Register<ConnectResponse>(EventDefine.S_C_ConnectResponse, Connect);
        }

        private void RemoveListener()
        {
            GameEntry.Instance.eventPool.Remove<byte[]>(EventDefine.SendMsg, SendMsg);
            GameEntry.Instance.eventPool.Remove<ConnectResponse>(EventDefine.S_C_ConnectResponse, Connect);
        }

        private void InitializeSocket()
        {
            try
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_socket.ReceiveBufferSize = receiveBufferSize;
                m_socket.SendBufferSize = sendBufferSize;

                m_socket.Blocking = false;

                receiveBuffer = new byte[1024];

                IPAddress ip = IPAddress.Parse(serverIP);
                m_serverEndPoint = new IPEndPoint(ip, serverPort);

                m_running = true;
                m_receiveThread = new Thread(ReceiveThread);
                m_receiveThread.IsBackground = true;
                m_receiveThread.Start();

                m_sendThread = new Thread(SendThread);
                m_sendThread.IsBackground = true;
                m_sendThread.Start();

                Debug.Log("UPD 初始化完成");

                // 发送连接请求
                var data = new ConnectRequest
                {
                    PlayerName = GameEntry.Instance.model.login.playerName,
                    IsConnect = true,
                };
                LoginProcessor.C_S_ConnectRequest(data);
                GameEntry.Instance.model.login.lastReceiveTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.LogError($"socket初始化失败：{ex.Message}");
            }
        }

        private async void Heartbeat()
        {
            await HeartbeatTick();
        }

        private async UniTask HeartbeatTick()
        {
            while (m_running && this != null)
            {
                await UniTask.Delay(2000);
                LoginProcessor.C_S_HeartBeat();
            }
        }

        //private void SendMsg(string msg)
        //{
        //    if (!m_running) return;
        //
        //    byte[] data = Encoding.UTF8.GetBytes(msg);
        //    que_send.Enqueue(data);
        //    sendEvent.Set();
        //}
        public void SendMsg(byte[] data)
        {
            if (!m_running) return;

            que_send.Enqueue(data);
            sendEvent.Set();
        }
        private void SendThread()
        {
            while (m_running)
            {
                try
                {
                    if (que_send.TryDequeue(out byte[] data))
                    {
                        GameEntry.Instance.model.login.lastReceiveTime = DateTime.Now;
                        m_socket.SendTo(data, m_serverEndPoint);
                        continue;
                    }
                    
                    sendEvent.WaitOne(50);
                }
                catch (Exception ex)
                {
                    if (m_running)
                        Debug.LogError($"发送线程错误: {ex.Message}");
                }
            }
        }

        private void Disconnect()
        {
            if (!m_running) return;

            // 发送断开连接请求
            var data = new ConnectRequest { IsConnect = false };
            LoginProcessor.C_S_ConnectRequest(data);
        }

        private void Connect(ConnectResponse data)
        {
            if (data.Success)
                Heartbeat();
            else
                AfterDisconnect();
        }

        private void AfterDisconnect()
        {
            if (!m_running) return;

            //SendMsg("disconnect");
            m_running = false;

            sendEvent.Set();
            sendEvent.Dispose();

            m_receiveThread?.Join(500);
            m_sendThread?.Join(500);

            m_socket?.Close();

            Debug.Log("与服务器断开连接");
        }

        private void ReceiveThread()
        {
            while (m_running)
            {
                try
                {
                    if (m_socket.Available > 0)
                    {
                        EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                        int bytesRead = m_socket.ReceiveFrom(receiveBuffer, ref sender);
                        GameEntry.Instance.model.login.lastReceiveTime = DateTime.Now;

                        if (bytesRead > 0)
                        {
                            byte[] data = new byte[bytesRead];
                            Buffer.BlockCopy(receiveBuffer, 0, data, 0, bytesRead);
                            que_receive.Enqueue(data);
                        }
                    }
                    else
                        Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    if (m_running)
                        Debug.LogError($"接收线程错误: {ex.Message}");
                }
            }
        }

        private void CollectInput()
        {
            // 收集键盘输入
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool fire = Input.GetKey(KeyCode.Space);

            m_currentInput = $"H:{horizontal:F2},V:{vertical:F2},F:{fire}";
        }
        void SendInput()
        {
            if (string.IsNullOrEmpty(m_currentInput))
                return;

            string msg = $"input|{m_currentInput}";
            //SendMsg(msg);
        }

        private void ProcessMsg(byte[] buffer)
        {
            ProtoHandler.OnRecvMsg(buffer);
        }

        void UpdateGameState(string playerData)
        {
            //格式：P1:H:0.5,V:0.0,F:false
            string[] colonParts = playerData.Split(':');
            if (colonParts.Length >= 2)
            {
                string playerId = colonParts[0];
                Debug.Log($"玩家 {playerId} 的数据: {playerData}");
            }
        }
    }

}