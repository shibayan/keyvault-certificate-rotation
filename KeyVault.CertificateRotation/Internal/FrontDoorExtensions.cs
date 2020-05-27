using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Management.FrontDoor;
using Microsoft.Azure.Management.FrontDoor.Models;

namespace KeyVault.CertificateRotation.Internal
{
    internal static class FrontDoorExtensions
    {
        public static async Task<IList<FrontDoorModel>> SafeListAllAsync(this IFrontDoorsOperations operations)
        {
            try
            {
                return await operations.ListAllAsync();
            }
            catch
            {
                return Array.Empty<FrontDoorModel>();
            }
        }

        public static async Task<IList<FrontDoorModel>> ListAllAsync(this IFrontDoorsOperations operations)
        {
            var frontDoors = new List<FrontDoorModel>();

            var list = await operations.ListAsync();

            frontDoors.AddRange(list);

            while (list.NextPageLink != null)
            {
                list = await operations.ListNextAsync(list.NextPageLink);

                frontDoors.AddRange(list);
            }

            return frontDoors;
        }

        public static async Task<IList<FrontendEndpoint>> ListAllByFrontDoorAsync(this IFrontendEndpointsOperations operations, string resourceGroupName, string frontDoorName)
        {
            var frontendEndpoints = new List<FrontendEndpoint>();

            var list = await operations.ListByFrontDoorAsync(resourceGroupName, frontDoorName);

            frontendEndpoints.AddRange(list);

            while (list.NextPageLink != null)
            {
                list = await operations.ListByFrontDoorNextAsync(list.NextPageLink);

                frontendEndpoints.AddRange(list);
            }

            return frontendEndpoints;
        }
    }
}
