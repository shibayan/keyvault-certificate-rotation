using System;
using System.Threading.Tasks;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.Cdn.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KeyVault.CertificateRotation
{
    public class CdnUpdaterFunction
    {
        public CdnUpdaterFunction(KeyVaultClient keyVaultClient, CdnManagementClient cdnManagementClient)
        {
            _keyVaultClient = keyVaultClient;
            _cdnManagementClient = cdnManagementClient;
        }

        private readonly KeyVaultClient _keyVaultClient;
        private readonly CdnManagementClient _cdnManagementClient;

        [FunctionName(nameof(CdnUpdater))]
        public async Task CdnUpdater([TimerTrigger("0 0 * * * *")] TimerInfo timer, ILogger log)
        {
            var cdnProfiles = await _cdnManagementClient.Profiles.ListAsync();

            foreach (var cdnProfile in cdnProfiles)
            {
                var resourceGroupName = ExtractResourceGroup(cdnProfile.Id);

                var cdnEndpoints = await _cdnManagementClient.Endpoints.ListByProfileAsync(resourceGroupName, cdnProfile.Name);

                foreach (var cdnEndpoint in cdnEndpoints)
                {
                    var cdnCustomDomains = await _cdnManagementClient.CustomDomains.ListByEndpointAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name);

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

                        await _cdnManagementClient.CustomDomains.EnableCustomHttpsAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name, cdnCustomDomain.Name, httpsParameters);
                    }
                }
            }
        }

        private static string ExtractResourceGroup(string resourceId)
        {
            var values = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return values[3];
        }
    }
}
