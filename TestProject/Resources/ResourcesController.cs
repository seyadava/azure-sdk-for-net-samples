namespace Resource
{
    using System;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Authorization;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;
    
    public class ResourcesController
    {
        private const string ComponentName = "DotnetSDK_ResourceController";
        private readonly CustomLoginCredentials credential;
        private readonly string subscriotionId;
        private Uri baseUri;
        private static Profile2018ResourceManager.ResourceManagementClient client;

        public ResourcesController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier
            )
        {
            this.baseUri = baseUri;
            this.credential = credentials;
            this.subscriotionId = subscriptionIdentifier;

            GetResourceGroupClient();
        }

        private void GetResourceGroupClient()
        {
            if (client != null)
            {
                return;
            }
            client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: this.baseUri, credentials: this.credential);
            client.SubscriptionId = this.subscriotionId;
            client.SetUserAgent(ComponentName);
        }

        public async Task<Profile2018ResourceManager.Models.ResourceGroup> CreateResourceGroup(
            string resourceGroupName, 
            string location)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
            }
            var resourceGroupTask = await client.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                resourceGroupName: resourceGroupName,
                parameters: new Profile2018ResourceManager.Models.ResourceGroup
                {
                    Location = location
                });

            if (!resourceGroupTask.Response.IsSuccessStatusCode)
            {
                return null;
            }
            return resourceGroupTask.Body;
        }

        public async Task<HttpResponseMessage> DeleteResourceGroup(string resourceGroupName)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
            }
            var resourceGroupTask = await client.ResourceGroups.DeleteWithHttpMessagesAsync(resourceGroupName);

            return resourceGroupTask.Response;
        }

        public async Task<bool> CheckResourceGroupExistance(string resourceGroupName)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
            }
            var resourceGroupTask = await client.ResourceGroups.CheckExistenceWithHttpMessagesAsync(resourceGroupName);

            if (!resourceGroupTask.Response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to check resource group existance.");
            }
            return resourceGroupTask.Body;
        }

        public async Task<Profile2018ResourceManager.Models.Provider> RegisterResourceProvider(string resourceProvider)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
            }
            var sleepSeconds = 0;
            Microsoft.Rest.Azure.AzureOperationResponse<Profile2018ResourceManager.Models.Provider> resourceGroupTask;
            while (true) {
                if (sleepSeconds > 120)
                {
                    return null;
                }
                resourceGroupTask = await client.Providers.RegisterWithHttpMessagesAsync(resourceProvider);

                if (!resourceGroupTask.Response.IsSuccessStatusCode)
                {
                    return null;
                }

                var provider = resourceGroupTask.Body;
                if (String.Equals(provider.RegistrationState, "registered", StringComparison.InvariantCultureIgnoreCase))
                {
                    return provider;
                }
                sleepSeconds += 10;
                System.Threading.Thread.Sleep(sleepSeconds * 1000);
            }
            
        }
    }
}
