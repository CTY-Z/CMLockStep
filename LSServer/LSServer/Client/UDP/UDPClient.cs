using LSServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LSServer.Client
{
    internal class UDPClient : ClientBase
    {
        public int ClientID => m_clientID;
        public IPEndPoint endPoint { get; set; }
        public DateTime lastReceiveTime { get; set; }

        public UDPClient(int clientId, IPEndPoint endPoint)
        {
            m_clientID = clientId;
            this.endPoint = endPoint;
            lastReceiveTime = DateTime.Now;
        }

        public override void SendMsg(string msg)
        {

        }

        protected override void Disconnect()
        {

        }

        public override void ProcessMsg(string msg)
        {
            lastReceiveTime = DateTime.Now;
            string[] arr_part = msg.Split('|');
            switch (arr_part[0])
            {
                case "input":
                    if (arr_part.Length >= 2)
                    {
                        string inputData = arr_part[1];
                        NetManager.Instance.UDPServer.RegisterInput(m_clientID, inputData);
                    }
                    break;

                case "ping":
                    NetManager.Instance.UDPServer.SendToClient(endPoint, "pong");
                    break;

                case "disconnect":
                    NetManager.Instance.UDPServer.RemoveClient(endPoint);
                    break;
            }
        }
    }
}
