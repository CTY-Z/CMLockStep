using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSServer.Server
{
    internal class NetManager
    {
        private TCPServer m_TCPServer;
        public TCPServer TCPServer { get { return m_TCPServer; } }
        private UDPServer m_UDPServer;
        public UDPServer UDPServer { get { return m_UDPServer; } }

        private NetManager() { }
        private static NetManager instance;
        public static NetManager Instance 
        {
            get
            {
                if (instance == null)
                    instance = new NetManager();
                return instance;
            }
        }

        public void TCPStart()
        {
            if(m_TCPServer == null) m_TCPServer = new TCPServer();
            m_TCPServer.Start();
        }

        public void UDPStart()
        {
            if (m_UDPServer == null) m_UDPServer = new UDPServer();
            m_UDPServer.Start();
        }
    }
}
