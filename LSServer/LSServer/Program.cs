using LSServer;
using LSServer.Model;
using LSServer.Server;
using LSServer.Utils;

namespace LS.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            NetManager.Instance.Init();
            ModelManager.Instance.Init();

            //NetManager.Instance.TCPStart();
            NetManager.Instance.UDPStart();
        }
    }
}



