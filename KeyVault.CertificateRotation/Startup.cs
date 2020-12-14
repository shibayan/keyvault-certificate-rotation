using System;

using KeyVault.CertificateRotation;
using KeyVault.CertificateRotation.Internal;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.FrontDoor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;

[assembly: FunctionsStartup(typeof(Startup))]

namespace KeyVault.CertificateRotation
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var subscriptionId = Environment.GetEnvironmentVariable("WEBSITE_OWNER_NAME").Split('+')[0];

            builder.Services.AddSingleton<CertificateClientFactory>();
            builder.Services.AddSingleton(new TokenCredentials(new ManagedIdentityTokenProvider()));

            builder.Services.AddSingleton(provider =>
            {
                var tokenCredentials = provider.GetRequiredService<TokenCredentials>();

                return new KeyVaultResolver(subscriptionId, tokenCredentials);
            });

            builder.Services.AddSingleton(provider =>
            {
                var tokenCredentials = provider.GetRequiredService<TokenCredentials>();

                return new CdnManagementClient(tokenCredentials) { SubscriptionId = subscriptionId };
            });

            builder.Services.AddSingleton(provider =>
            {
                var tokenCredentials = provider.GetRequiredService<TokenCredentials>();

                return new FrontDoorManagementClient(tokenCredentials) { SubscriptionId = subscriptionId };
            });
        }
    }
}
