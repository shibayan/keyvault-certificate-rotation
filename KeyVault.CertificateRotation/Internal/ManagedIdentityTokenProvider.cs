using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core;
using Azure.Identity;

using Microsoft.Rest;

namespace KeyVault.CertificateRotation.Internal
{
    internal class ManagedIdentityTokenProvider : ITokenProvider
    {
        public ManagedIdentityTokenProvider()
        {
            _tokenCredential = new DefaultAzureCredential();
        }

        private readonly TokenCredential _tokenCredential;

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var context = new TokenRequestContext(new[] { "https://management.azure.com" });

            var accessToken = await _tokenCredential.GetTokenAsync(context, cancellationToken);

            return new AuthenticationHeaderValue("Bearer", accessToken.Token);
        }
    }
}
