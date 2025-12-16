using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    [Header("连接设置")]
    public string serverIP = "127.0.0.1";
    public int serverPort = 8888;

    [Header("调试信息")]
    public bool showLog = true;
    public string status = "未连接";//本地化
    public int clientID = -1;
    public int currentFrame = 0;

    private TcpClient m_tcpClient;
    private NetworkStream m_stream;
    private Thread m_receiveThread;
    private bool m_isConnected = false;
    private Queue<string> que_receivedMsg = new();
    private object m_queueLock = new();

    private string m_currentInput = "";

    private void Start()
    {
        ConnentToServer();
    }

    private void Update()
    {
        
    }

    private void ConnentToServer()
    {
        try
        {
            status = "连接中...";
            m_tcpClient = new();
            m_tcpClient.Connect(serverIP, serverPort);
            m_stream = m_tcpClient.GetStream();
            m_isConnected = true;

            m_receiveThread = new(ReceivedMsgs);
            m_receiveThread.IsBackground = true;
            m_receiveThread.Start();

            status = "已连接";
            Debug.Log("成功连接到服务器");
        }
        catch(Exception ex)
        {
            status = "连接失败";
            Debug.LogError($"连接服务器失败：{ex.Message}");
        }
    }

    private void ReceivedMsgs()
    {
        byte[] buffer = new byte[4096];

        while (m_isConnected && m_stream != null)
        {
            try
            {
                int bytesRead = m_stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                    break;

                string bufferStr = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                string[] arr_msg = bufferStr.Split('\n');
                foreach (string msg in arr_msg)
                {
                    lock (m_queueLock)
                    {
                        que_receivedMsg.Enqueue(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"接收错误: {ex.Message}");
                break;
            }

            Disconnect();
        }

        void Disconnect()
        {
            m_isConnected = false;
            status = "已断开";

            if (m_stream != null)
            {
                m_stream.Close();
                m_stream = null;
            }

            if (m_tcpClient != null)
            {
                m_tcpClient.Close();
                m_tcpClient = null;
            }

            Debug.Log("与服务器断开连接");
        }
    }
}
