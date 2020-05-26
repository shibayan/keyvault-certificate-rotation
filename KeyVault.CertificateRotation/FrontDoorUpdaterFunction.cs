using System;
using System.Threading.Tasks;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.FrontDoor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KeyVault.CertificateRotation
{
    public class FrontDoorUpdaterFunction
    {
        public FrontDoorUpdaterFunction(KeyVaultClient keyVaultClient, FrontDoorManagementClient frontDoorManagementClient)
        {
            _keyVaultClient = keyVaultClient;
            _frontDoorManagementClient = frontDoorManagementClient;
        }

        private readonly KeyVaultClient _keyVaultClient;
        private readonly FrontDoorManagementClient _frontDoorManagementClient;

        [FunctionName("FrontDoorUpdater")]
        public async Task FrontDoorUpdater([TimerTrigger("0 0 0 * * *")] TimerInfo timer, ILogger log)
        {
            var frontDoors = await _frontDoorManagementClient.FrontDoors.ListAsync();

            foreach (var frontDoor in frontDoors)
            {
                var resourceGroupName = ExtractResourceGroup(frontDoor.Id);

                var frontendEndpoints = await _frontDoorManagementClient.FrontendEndpoints.ListByFrontDoorAsync(resourceGroupName, frontDoor.Name);

                foreach (var frontendEndpoint in frontendEndpoints)
                {
                    if (frontendEndpoint.CustomHttpsConfiguration?.CertificateSource != "AzureKeyVault")
                    {
                        continue;
                    }

                    var vaultName = ExtractzVaultName(frontendEndpoint.CustomHttpsConfiguration.Vault.Id);

                    var latestCertificate = await _keyVaultClient.GetCertificateAsync(
                        $"https://{vaultName}.vault.azure.net/",
                        frontendEndpoint.CustomHttpsConfiguration.SecretName);

                    if (latestCertificate.CertificateIdentifier.Version == frontendEndpoint.CustomHttpsConfiguration.SecretVersion)
                    {
                        continue;
                    }

                    frontendEndpoint.CustomHttpsConfiguration.SecretVersion = latestCertificate.CertificateIdentifier.Version;

                    await _frontDoorManagementClient.FrontendEndpoints.EnableHttpsAsync(resourceGroupName, frontDoor.Name, frontendEndpoint.Name, frontendEndpoint.CustomHttpsConfiguration);
                }
            }
        }

        private static string ExtractResourceGroup(string resourceId)
        {
            var values = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return values[3];
        }

        private static string ExtractzVaultName(string resourceId)
        {
            var values = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return values[7];
        }
    }
}
