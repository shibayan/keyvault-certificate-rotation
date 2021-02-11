using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using KeyVault.CertificateRotation.Internal;

using Microsoft.Azure.Management.FrontDoor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KeyVault.CertificateRotation
{
    public class FrontDoorRotationFunction
    {
        public FrontDoorRotationFunction(CertificateClientFactory certificateClientFactory, FrontDoorManagementClient frontDoorManagementClient)
        {
            _certificateClientFactory = certificateClientFactory;
            _frontDoorManagementClient = frontDoorManagementClient;
        }

        private readonly CertificateClientFactory _certificateClientFactory;
        private readonly FrontDoorManagementClient _frontDoorManagementClient;

        [FunctionName(nameof(FrontDoorRotation))]
        public async Task FrontDoorRotation([TimerTrigger("0 0 0 * * 1")] TimerInfo timer, ILogger log)
        {
            var tasks = new List<Task>();

            var frontDoors = await _frontDoorManagementClient.FrontDoors.SafeListAllAsync();

            foreach (var frontDoor in frontDoors)
            {
                log.LogInformation($"Front Door: {frontDoor.Name}");

                var resourceGroupName = frontDoor.ResourceGroupName();

                var frontendEndpoints = await _frontDoorManagementClient.FrontendEndpoints.ListAllByFrontDoorAsync(resourceGroupName, frontDoor.Name);

                foreach (var frontendEndpoint in frontendEndpoints)
                {
                    log.LogInformation($"Frontend Endpoint: {frontendEndpoint.Name}");

                    if (frontendEndpoint.CustomHttpsConfiguration?.CertificateSource != "AzureKeyVault")
                    {
                        continue;
                    }

                    var vaultName = ExtractVaultName(frontendEndpoint.CustomHttpsConfiguration.Vault.Id);

                    log.LogInformation($"Vault Name: {vaultName}");
                    log.LogInformation($"Secret Name: {frontendEndpoint.CustomHttpsConfiguration.SecretName}");
                    log.LogInformation($"Secret Version: {frontendEndpoint.CustomHttpsConfiguration.SecretVersion}");

                    var certificateClient = _certificateClientFactory.CreateClient(vaultName);

                    var latestCertificate = await certificateClient.GetCertificateAsync(frontendEndpoint.CustomHttpsConfiguration.SecretName);

                    if (latestCertificate.Value.Properties.Version == frontendEndpoint.CustomHttpsConfiguration.SecretVersion)
                    {
                        continue;
                    }

                    log.LogInformation($"Target Secret Version: {latestCertificate.Value.Properties.Version}");

                    frontendEndpoint.CustomHttpsConfiguration.SecretVersion = latestCertificate.Value.Properties.Version;

                    tasks.Add(_frontDoorManagementClient.FrontendEndpoints.EnableHttpsAsync(resourceGroupName, frontDoor.Name, frontendEndpoint.Name, frontendEndpoint.CustomHttpsConfiguration));
                }
            }

            await Task.WhenAll(tasks);
        }

        private static string ExtractVaultName(string resourceId)
        {
            var values = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return values[7];
        }
    }
}
