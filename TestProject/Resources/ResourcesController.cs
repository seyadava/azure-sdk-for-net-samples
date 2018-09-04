namespace Resource
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Rest.Azure;
    
    public class ResourcesController
    {
        private const string ComponentName = "DotnetSDK_ResourceController";
        private readonly CustomLoginCredentials credential;
        private readonly Uri baseUri;
        private static Profile2018ResourceManager.ResourceManagementClient client;
        private static IAzure azure;

        public ResourcesController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subsId)
        {
            this.baseUri = baseUri;
            this.credential = credentials;
            GetResourceGroupClient(subsId);
        }

        private void GetResourceGroupClient(string subscriptionId)
        {
            if (client != null)
            {
                return;
            }
            client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: this.credential)
            {
                SubscriptionId = subscriptionId
            };

            client.SetUserAgent(ComponentName);
        }

       public async Task<AzureOperationResponse<Profile2018ResourceManager.Models.ResourceGroup>> CreateResourceGroup(
            string resourceGroupName, 
            string location)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018ResourceManager.Models.ResourceGroup>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }
            try
            {
                var resourceGroupTask = await client.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName: resourceGroupName,
                    parameters: new Profile2018ResourceManager.Models.ResourceGroup
                    {
                        Location = location
                    });
                return resourceGroupTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018ResourceManager.Models.ResourceGroup>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<HttpResponseMessage> DeleteResourceGroup(string resourceGroupName)
        {
            if (client == null)
            {
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                    ReasonPhrase = "Client is not instantiated"
                };
            }
            try
            {
                var resourceGroupTask = await client.ResourceGroups.DeleteWithHttpMessagesAsync(resourceGroupName);

                return resourceGroupTask.Response;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ReasonPhrase = ex.Message
                };
            }
        }

        public async Task<AzureOperationResponse<bool>> CheckResourceGroupExistance(string resourceGroupName)
        {
            if (client == null)
            {
                return new AzureOperationResponse<bool>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }
            try
            {
                var resourceGroupTask = await client.ResourceGroups.CheckExistenceWithHttpMessagesAsync(resourceGroupName);
                return resourceGroupTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<bool>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<AzureOperationResponse<Profile2018ResourceManager.Models.Provider>> RegisterResourceProvider(
            string resourceProvider,
            int maxRetryDuration = 120,
            int sleepDuration = 10)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018ResourceManager.Models.Provider>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }
            var sleepSeconds = 0;
            AzureOperationResponse<Profile2018ResourceManager.Models.Provider> resourceGroupTask;
            try
            {
                while (true)
                {
                    if (sleepSeconds > maxRetryDuration)
                    {
                        return null;
                    }
                    resourceGroupTask = await client.Providers.RegisterWithHttpMessagesAsync(resourceProvider);

                    if (!resourceGroupTask.Response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var provider = resourceGroupTask;
                    if (String.Equals(provider.Body.RegistrationState, "registered", StringComparison.OrdinalIgnoreCase))
                    {
                        return provider;
                    }
                    sleepSeconds += sleepDuration;
                    System.Threading.Thread.Sleep(sleepSeconds * 1000);
                }
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018ResourceManager.Models.Provider>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }
    }
}
