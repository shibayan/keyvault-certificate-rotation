using System.Collections.Generic;
using System.Threading.Tasks;

using KeyVault.CertificateRotation.Internal;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.Cdn.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KeyVault.CertificateRotation
{
    public class CdnRotationFunction
    {
        public CdnRotationFunction(KeyVaultClient keyVaultClient, CdnManagementClient cdnManagementClient)
        {
            _keyVaultClient = keyVaultClient;
            _cdnManagementClient = cdnManagementClient;
        }

        private readonly KeyVaultClient _keyVaultClient;
        private readonly CdnManagementClient _cdnManagementClient;

        [FunctionName(nameof(CdnRotation))]
        public async Task CdnRotation([TimerTrigger("0 0 * * * *")] TimerInfo timer, ILogger log)
        {
            var tasks = new List<Task>();

            var cdnProfiles = await _cdnManagementClient.Profiles.SafeListAllAsync();

            foreach (var cdnProfile in cdnProfiles)
            {
                var resourceGroupName = cdnProfile.ResourceGroupName();

                var cdnEndpoints = await _cdnManagementClient.Endpoints.ListAllByProfileAsync(resourceGroupName, cdnProfile.Name);

                foreach (var cdnEndpoint in cdnEndpoints)
                {
                    var cdnCustomDomains = await _cdnManagementClient.CustomDomains.ListAllByEndpointAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name);

                    foreach (var cdnCustomDomain in cdnCustomDomains)
                    {
                        var httpsParameters = cdnCustomDomain.CustomHttpsParameters as UserManagedHttpsParameters;

                        if (httpsParameters == null)
                        {
                            continue;
                        }

                        var latestCertificate = await _keyVaultClient.GetCertificateAsync(
                            $"https://{httpsParameters.CertificateSourceParameters.VaultName}.vault.azure.net/",
                            httpsParameters.CertificateSourceParameters.SecretName);

                        if (latestCertificate.CertificateIdentifier.Version == httpsParameters.CertificateSourceParameters.SecretVersion)
                        {
                            continue;
                        }

                        httpsParameters.CertificateSourceParameters.SecretVersion = latestCertificate.CertificateIdentifier.Version;

                        tasks.Add(_cdnManagementClient.CustomDomains.EnableCustomHttpsAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name, cdnCustomDomain.Name, httpsParameters));
                    }
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
