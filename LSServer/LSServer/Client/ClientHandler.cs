using LS.Server;
using LSServer.Utils;
using System.Net.Sockets;
using System.Text;

namespace LSServer.Client
{
    internal class ClientHandler
    {
        private TcpClient m_client;
        private NetworkStream m_stream;
        private int m_clientID;
        private bool m_isConnented = true;

        public int ClientID => m_clientID;
        public bool IsConnented => m_isConnented;

        public ClientHandler(TcpClient client, int clientID)
        {
            m_client = client;
            m_stream = client.GetStream();
            m_clientID = clientID;
        }

        public void HandleClient()
        {
            try
            {
                SendMessage($"welcome! {m_clientID}");
                Debug.Log($"分配ID给客户端:{m_clientID}");

                byte[] buffer = new byte[1024];

                while (m_isConnented)
                {
                    int bytesRead = m_stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) // 客户端已经断开连接了
                        break;

                    string msg = Encoding.UTF8.GetString(buffer);
                    Debug.Log($"收到客户端{m_clientID}: {msg}");

                    ProcessMessage(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"客户端 {m_clientID} 错误: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        void ProcessMessage(string msg)
        {
            if (msg.StartsWith("Input|"))
            {
                string[] arr_part = msg.Split('|');
                if (arr_part.Length >= 3)
                {
                    string inputData = arr_part[2];
                    Program.RecordInput(m_clientID, inputData);
                }
            }
            else if(msg == "Ping")
                SendMessage("Pong");
        }

        public void SendMessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                m_stream.Write(data, 0, data.Length);
            }
            catch
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            m_isConnented = false;
            m_stream?.Close();
            m_client?.Close();
            Debug.Log($"客户端-{m_clientID}-断开连接");
        }
    }
}
