using KeyVault.CertificateRotation;
using KeyVault.CertificateRotation.Internal;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.FrontDoor;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;

[assembly: FunctionsStartup(typeof(Startup))]

namespace KeyVault.CertificateRotation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(provider =>
                new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)));

            builder.Services.AddSingleton(provider =>
            {
                return new CdnManagementClient(new TokenCredentials(new AppAuthenticationTokenProvider()))
                {
                    SubscriptionId = "6f43ad45-521d-4bd7-9d01-f95a290041fe"
                };
            });

            builder.Services.AddSingleton(provider =>
            {
                return new FrontDoorManagementClient(new TokenCredentials(new AppAuthenticationTokenProvider()))
                {
                    SubscriptionId = "6f43ad45-521d-4bd7-9d01-f95a290041fe"
                };
            });
        }
    }
}
