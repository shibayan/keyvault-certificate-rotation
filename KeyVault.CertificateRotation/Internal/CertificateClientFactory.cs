using System;

using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace KeyVault.CertificateRotation.Internal
{
    public interface ICertificateClientFactory
    {
        CertificateClient CreateClient(string vaultName);
    }

    public class CertificateClientFactory : ICertificateClientFactory
    {
        public CertificateClient CreateClient(string vaultName)
        {
            return new CertificateClient(new Uri($"https://{vaultName}.vault.azure.net"), new DefaultAzureCredential());
        }
    }
}
