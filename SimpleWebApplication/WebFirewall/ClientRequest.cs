namespace SimpleWebApplication.WebFirewall
{
    public class ClientRequest
    {
        public int RequestCount { get; set; }
        public DateTime LastRequestTime { get; set; }
        public int BlockDuration { get; set; }

        public ClientRequest(int requestCount, DateTime lastRequestTime, int blockDuration)
        {
            RequestCount = requestCount;
            LastRequestTime = lastRequestTime;
            BlockDuration = blockDuration;
        }
    }
}
