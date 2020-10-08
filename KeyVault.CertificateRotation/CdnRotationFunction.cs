using System.Collections.Generic;
using System.Threading.Tasks;

using KeyVault.CertificateRotation.Internal;

using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.Cdn.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KeyVault.CertificateRotation
{
    public class CdnRotationFunction
    {
        public CdnRotationFunction(ICertificateClientFactory certificateClientFactory, CdnManagementClient cdnManagementClient)
        {
            _certificateClientFactory = certificateClientFactory;
            _cdnManagementClient = cdnManagementClient;
        }

        private readonly ICertificateClientFactory _certificateClientFactory;
        private readonly CdnManagementClient _cdnManagementClient;

        [FunctionName(nameof(CdnRotation))]
        public async Task CdnRotation([TimerTrigger("0 0 0 * * *")] TimerInfo timer, ILogger log)
        {
            var tasks = new List<Task>();

            var cdnProfiles = await _cdnManagementClient.Profiles.SafeListAllAsync();

            foreach (var cdnProfile in cdnProfiles)
            {
                log.LogInformation($"CDN Proflie: {cdnProfile.Name}");

                var resourceGroupName = cdnProfile.ResourceGroupName();

                var cdnEndpoints = await _cdnManagementClient.Endpoints.ListAllByProfileAsync(resourceGroupName, cdnProfile.Name);

                foreach (var cdnEndpoint in cdnEndpoints)
                {
                    log.LogInformation($"CDN Endpoint: {cdnEndpoint.Name}");

                    var cdnCustomDomains = await _cdnManagementClient.CustomDomains.ListAllByEndpointAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name);

                    foreach (var cdnCustomDomain in cdnCustomDomains)
                    {
                        log.LogInformation($"Custom Domain: {cdnCustomDomain.Name}");

                        var httpsParameters = cdnCustomDomain.CustomHttpsParameters as UserManagedHttpsParameters;

                        if (httpsParameters == null)
                        {
                            continue;
                        }

                        log.LogInformation($"Vault Name: {httpsParameters.CertificateSourceParameters.VaultName}");
                        log.LogInformation($"Secret Name: {httpsParameters.CertificateSourceParameters.SecretName}");
                        log.LogInformation($"Secret Version: {httpsParameters.CertificateSourceParameters.SecretVersion}");

                        var certificateClient = _certificateClientFactory.CreateClient(httpsParameters.CertificateSourceParameters.VaultName);

                        var latestCertificate = await certificateClient.GetCertificateAsync(httpsParameters.CertificateSourceParameters.SecretName);

                        if (latestCertificate.Value.Properties.Version == httpsParameters.CertificateSourceParameters.SecretVersion)
                        {
                            continue;
                        }

                        log.LogInformation($"Target Secret Version: {latestCertificate.Value.Properties.Version}");

                        httpsParameters.CertificateSourceParameters.SecretVersion = latestCertificate.Value.Properties.Version;

                        tasks.Add(_cdnManagementClient.CustomDomains.EnableCustomHttpsAsync(resourceGroupName, cdnProfile.Name, cdnEndpoint.Name, cdnCustomDomain.Name, httpsParameters));
                    }
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
