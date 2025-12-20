namespace LSServer.Client
{
    internal abstract class ClientBase
    {
        protected int m_clientID;

        public abstract void SendMsg(byte[] data);
        public abstract void ProcessMsg(string msg);

        protected abstract void Disconnect();
    }
}
