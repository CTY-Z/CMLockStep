using LSServer.Server;

namespace LS.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //NetManager.Instance.TCPStart();
            NetManager.Instance.UDPStart();
        }
    }
}



