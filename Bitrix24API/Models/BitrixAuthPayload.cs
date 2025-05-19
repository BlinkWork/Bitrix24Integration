using Microsoft.AspNetCore.Mvc;

namespace Bitrix24API.Models
{
    public class BitrixAuthPayload
    {
        [FromForm(Name = "auth[access_token]")]
        public string AccessToken { get; set; }
        [FromForm(Name = "auth[expires]")]
        public long Expires { get; set; }
        [FromForm(Name = "auth[expires_in]")]
        public long ExpiresIn { get; set; }
        [FromForm(Name = "auth[refresh_token]")]
        public string RefreshToken { get; set; }
        [FromForm(Name = "auth[client_endpoint]")]
        public string ClientEndpoint { get; set; }
    }
}
