using System;
using System.Threading.Tasks;
using Authorization;
using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;

namespace Resource
{
    public class Resources
    {

        private static Profile2018ResourceManager.ResourceManagementClient client;

        public static Profile2018ResourceManager.ResourceManagementClient GetClient
            (Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            if (client == null)
            {
                client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: credentials);
                client.SubscriptionId = subscriotionId;
            }
            return client;
        }

        public static async Task<Profile2018ResourceManager.Models.ResourceGroup> CreateResourceGroup(string resourceGroupName, string location)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
            }
            var resourceGroup = await client.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                resourceGroupName: resourceGroupName,
                parameters: new Profile2018ResourceManager.Models.ResourceGroup
                {
                    Location = location
                });

            return resourceGroup.Body;
        }
    }
}
