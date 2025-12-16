using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
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
    private bool m_isConnented = false;
    private Queue<string> m_que_receivedMsg = new();
    private object m_queueLock = new();

    private string m_currentInput = "";

    private void Start()
    {
        ConnentToServer();
    }

    private void ConnentToServer()
    {
        try
        {
            status = "连接中...";
            m_tcpClient = new();
            m_tcpClient.Connect(serverIP, serverPort);
            m_stream = m_tcpClient.GetStream();
            m_isConnented = true;

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

    }
}
