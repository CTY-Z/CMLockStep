using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    [Header("连接设置")]
    public string serverIP = "127.0.0.1";
    public int serverPort = 8888;

    [Header("调试信息")]
    public bool showLog = true;
    public string status = "未连接";
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
        ConnectToServer();
    }

    private void Update()
    {
        ProcessReceivedMsg();

        CollectInput();

        //if (m_isConnected && Time.frameCount % 2 == 0)
        //{
        //    SendInput();
        //}
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"状态: {status}");
        GUILayout.Label($"客户端ID: {clientID}");
        GUILayout.Label($"当前帧: {currentFrame}");
        GUILayout.Label($"当前输入: {m_currentInput}");

        if (GUILayout.Button("发送测试消息"))
            SendMsg("ping");

        if (m_isConnected && GUILayout.Button("断开连接"))
            Disconnect();

        if (!m_isConnected && GUILayout.Button("重新连接"))
            ConnectToServer();

        GUILayout.EndArea();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void ConnectToServer()
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

    private void Disconnect()
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
        if (!m_isConnected || string.IsNullOrEmpty(m_currentInput))
            return;

        string msg = $"input|{clientID}|{m_currentInput}";
        SendMsg(msg);
    }



    void SendMsg(string msg)
    {
        try
        {
            //Debug.Log($"给服务端发送了 - {msg}");
            byte[] data = Encoding.UTF8.GetBytes(msg);
            m_stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"发送失败: {e.Message}");
            Disconnect();
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
                if (bytesRead == 0)
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

        }
        //Disconnect();
    }

    private void ProcessReceivedMsg()
    {
        lock(m_queueLock)
        {
            while (que_receivedMsg.Count > 0)
            {
                string msg = que_receivedMsg.Dequeue();
                HandleServerMsg(msg);
            }
        }
    }

    void HandleServerMsg(string msg)
    {
        string[] arr_part = msg.Split("|");

        if (showLog && arr_part[0] != "frame")
            Debug.Log($"收到{msg}");

        if (arr_part[0] == "welcome")
        {
            if (int.TryParse(arr_part[1], out int ID))
            {
                clientID = ID;
                status = $"已连接 - id : {ID}";
                Debug.Log($"服务器分配ID:{ID}");
            }
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
            else if (arr_part[0] == "pong")
            {
                Debug.Log("接收到服务器响应");
            }
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
