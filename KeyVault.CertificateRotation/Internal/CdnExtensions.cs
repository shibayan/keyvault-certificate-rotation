using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Management.Cdn;
using Microsoft.Azure.Management.Cdn.Models;

namespace KeyVault.CertificateRotation.Internal
{
    internal static class CdnExtensions
    {
        public static async Task<IList<Profile>> SafeListAllAsync(this IProfilesOperations operations)
        {
            try
            {
                return await operations.ListAllAsync();
            }
            catch
            {
                return Array.Empty<Profile>();
            }
        }

        public static async Task<IList<Profile>> ListAllAsync(this IProfilesOperations operations)
        {
            var profiles = new List<Profile>();

            var list = await operations.ListAsync();

            profiles.AddRange(list);

            while (list.NextPageLink != null)
            {
                list = await operations.ListNextAsync(list.NextPageLink);

                profiles.AddRange(list);
            }

            return profiles;
        }

        public static async Task<IList<Endpoint>> ListAllByProfileAsync(this IEndpointsOperations operations, string resourceGroupName, string profileName)
        {
            var endpoints = new List<Endpoint>();

            var list = await operations.ListByProfileAsync(resourceGroupName, profileName);

            endpoints.AddRange(list);

            while (list.NextPageLink != null)
            {
                list = await operations.ListByProfileNextAsync(list.NextPageLink);

                endpoints.AddRange(list);
            }

            return endpoints;
        }

        public static async Task<IList<CustomDomain>> ListAllByEndpointAsync(this ICustomDomainsOperations operations, string resourceGroupName, string profileName, string endpointName)
        {
            var endpoints = new List<CustomDomain>();

            var list = await operations.ListByEndpointAsync(resourceGroupName, profileName, endpointName);

            endpoints.AddRange(list);

            while (list.NextPageLink != null)
            {
                list = await operations.ListByEndpointNextAsync(list.NextPageLink);

                endpoints.AddRange(list);
            }

            return endpoints;
        }
    }
}
