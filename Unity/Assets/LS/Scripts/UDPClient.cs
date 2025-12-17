using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        public bool noDelay = true;
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
        private DateTime m_lastSendTime;
        private DateTime m_lastReceiveTime;

        private void Start()
        {
            InitializeSocket();
        }

        private void Update()
        {
            while (que_receive.TryDequeue(out byte[] data))
                ProcessMsg(data);

            CollectInput();

            if (Time.frameCount % 2 == 0)
            {
                SendInput();
            }
        }

        private void OnDestroy()
        {
            m_running = false;

            m_receiveThread?.Join(500);
            m_sendThread?.Join(500);

            m_socket?.Close();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"当前帧: {currentFrame}");
            GUILayout.Label($"当前输入: {m_currentInput}");

            if (GUILayout.Button("发送测试消息"))
                SendMsg("ping");

            GUILayout.EndArea();
        }




        private void InitializeSocket()
        {
            try
            {
                m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_socket.ReceiveBufferSize = receiveBufferSize;
                m_socket.SendBufferSize = sendBufferSize;

                m_socket.Blocking = false;

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
                SendMsg("connect");
                m_lastReceiveTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.LogError($"socket初始化失败：{ex.Message}");
            }
        }

        private void SendMsg(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            que_send.Enqueue(data);
        }
        private void SendThread()
        {
            receiveBuffer = new byte[1024];

            while (m_running)
            {
                try
                {
                    if (que_send.TryDequeue(out byte[] data))
                        m_socket.SendTo(data, m_serverEndPoint);
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

                        if (bytesRead > 0)
                        {
                            byte[] data = data = new byte[bytesRead];
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
            SendMsg(msg);
        }



        private void ProcessMsg(byte[] buffer)
        {
            string msg = Encoding.UTF8.GetString(buffer);
            HandleServerMsg(msg);
        }

        void HandleServerMsg(string msg)
        {
            string[] arr_part = msg.Split("|");

            if (showLog)
                Debug.Log($"收到{msg}");

            if (arr_part[0] == "welcome")
            {
                if (int.TryParse(arr_part[1], out int ID))
                    Debug.Log($"服务器分配ID:{ID}");
            }
            else if (arr_part[0] == "frame")
            {
                if (int.TryParse(arr_part[1], out int frameCount))
                {
                    currentFrame = frameCount;

                    for (int i = 2; i < arr_part.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(arr_part[i]))
                        {
                            //格式：P1:H:0.5,V:0.0,F:false;
                            string playerData = arr_part[i].TrimEnd(';');
                            UpdateGameState(playerData);
                        }
                    }
                }
            }
            else if (arr_part[0] == "pong")
            {
                Debug.Log("接收到服务器响应");
            }
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