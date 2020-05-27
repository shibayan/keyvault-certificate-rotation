using System;

using CdnResource = Microsoft.Azure.Management.Cdn.Models.Resource;
using FrontDoorResource = Microsoft.Azure.Management.FrontDoor.Models.Resource;

namespace KeyVault.CertificateRotation.Internal
{
    internal static class ResourceExtensions
    {
        public static string ResourceGroupName(this CdnResource resource)
        {
            return ExtractResourceGroup(resource.Id);
        }

        public static string ResourceGroupName(this FrontDoorResource resource)
        {
            return ExtractResourceGroup(resource.Id);
        }

        private static string ExtractResourceGroup(string resourceId)
        {
            var values = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return values[3];
        }
    }
}
