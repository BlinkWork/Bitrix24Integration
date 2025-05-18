namespace Bitrix24API.Models
{
    public class CallApiRequest
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string method { get; set; }
        public Dictionary<string, object> payload { get; set; }

    }
}
