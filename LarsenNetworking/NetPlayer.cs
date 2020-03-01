namespace LarsenNetworking
{
    public class NetPlayer
    {
        public int Ping { get; set; }
        enum State
        {
            Connected,
            Disconnected,
            Retrying,
        }
    }
}