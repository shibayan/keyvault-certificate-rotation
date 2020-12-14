using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Rest;

using Newtonsoft.Json.Linq;

namespace KeyVault.CertificateRotation.Internal
{
    public class KeyVaultResolver
    {
        public KeyVaultResolver(string subscriptionId, TokenCredentials tokenCredentials)
        {
            _subscriptionId = subscriptionId;
            _resourceGraphClient = new ResourceGraphClient(tokenCredentials);
        }

        private readonly string _subscriptionId;
        private readonly ResourceGraphClient _resourceGraphClient;

        public async Task<QueryResult> ResolveAsync(string vaultName)
        {
            var queryRequest = new QueryRequest
            {
                Subscriptions = new[] { _subscriptionId },
                Query = $"where type == 'microsoft.keyvault/vaults' and name == '{vaultName}' | project id, resourceGroup, subscriptionId",
                Options = new QueryRequestOptions
                {
                    ResultFormat = ResultFormat.ObjectArray
                }
            };

            var response = await _resourceGraphClient.ResourcesAsync(queryRequest);

            return ((JToken)response.Data).ToObject<QueryResult[]>().FirstOrDefault();
        }
    }
}
