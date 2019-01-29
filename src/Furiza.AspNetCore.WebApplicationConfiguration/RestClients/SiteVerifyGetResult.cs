using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApplicationConfiguration.RestClients
{
    public class SiteVerifyGetResult
    {
        public bool? Success { get; set; }

        [JsonProperty(PropertyName = "challenge_ts")]
        public DateTime? ChallengeTS { get; set; }

        public string HostName { get; set; }

        [JsonProperty(PropertyName = "error-codes")]
        public IEnumerable<string> ErrorCodes { get; set; }
    }
}