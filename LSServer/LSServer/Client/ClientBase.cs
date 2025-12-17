namespace LSServer.Client
{
    internal abstract class ClientBase
    {
        protected int m_clientID;

        public abstract void SendMsg(string msg);
        public abstract void ProcessMsg(string msg);

        protected abstract void Disconnect();
    }
}
