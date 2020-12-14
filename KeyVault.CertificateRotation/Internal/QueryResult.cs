using Newtonsoft.Json;

namespace KeyVault.CertificateRotation.Internal
{
    public class QueryResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resourceGroup")]
        public string ResourceGroup { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }
    }
}
