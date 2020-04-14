using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Identity.Web.Models
{
    public class TokenModel
    {
        public string Raw { get; }
        public IDictionary<string, object> Header { get; }
        public IDictionary<string, object> Payload { get; }

        public TokenModel(string token)
        {
            Raw = token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var segments = token.Split('.');
                var header = Base64UrlEncoder.Decode(segments[0]);
                var payload = Base64UrlEncoder.Decode(segments[1]);
                Header = JsonConvert.DeserializeObject<Dictionary<string, object>>(header);
                Payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
            }
        }
    }
}