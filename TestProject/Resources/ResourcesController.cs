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
        private readonly AzureCredentials azureCredential;
        private readonly CustomLoginCredentials credential;
        private readonly string subscriotionId;
        private readonly string environment; 
        private readonly Uri baseUri;
        private static Profile2018ResourceManager.ResourceManagementClient client;
        private static IAzure azure;

        public ResourcesController(
            Uri baseUri,
            AzureCredentials credentials,
            string environment = "azurestack")
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                this.baseUri = baseUri;
                this.azureCredential = credentials;
                GetResourceGroupClient(this.azureCredential.DefaultSubscriptionId);
            }
            else
            {
                azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();
            }
            this.environment = environment;
        }

        public ResourcesController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionId)
        {
            this.baseUri = baseUri;
            this.credential = credentials;

            GetResourceGroupClient(subscriotionId);
            this.environment = "azurestack";
        }

        private void GetResourceGroupClient(string subscriptionId)
        {
            if (client != null)
            {
                return;
            }
            client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: azureCredential);
            client.SubscriptionId = subscriotionId;
            
            client.SetUserAgent(ComponentName);
        }

        private async Task<AzureOperationResponse<string>> CreateResourceGroupAzureStack(
            string resourceGroupName,
            string location)
        {
            if (client == null)
            {
                return new AzureOperationResponse<string>
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
                return new AzureOperationResponse<string>
                {
                    Body = resourceGroupTask.Body.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        private AzureOperationResponse<string> CreateResourceGroupAzure(
            string resourceGroupName,
            string location)
        {
            if (azure == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Azure is not instantiated"
                    }
                };
            }
            try
            {
                var resourceGroup = azure.ResourceGroups
                    .Define(resourceGroupName)
                    .WithRegion(location)
                    .Create();
                return new AzureOperationResponse<string>
                {
                    Body = resourceGroup.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<AzureOperationResponse<string>> CreateResourceGroup(
            string resourceGroupName, 
            string location)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreateResourceGroupAzureStack(resourceGroupName, location);
            }
            else
            {
                return CreateResourceGroupAzure(resourceGroupName, location);
            }
        }

        private async Task<HttpResponseMessage> DeleteResourceGroupAzureStack(string resourceGroupName)
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

        private async Task<HttpResponseMessage> DeleteResourceGroupAzure(string resourceGroupName)
        {
            if (azure == null)
            {
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                    ReasonPhrase = "Azure is not instantiated"

                };
            }
            try
            {
                await azure.ResourceGroups.DeleteByNameAsync(resourceGroupName);

                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK
                };
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

        public async Task<HttpResponseMessage> DeleteResourceGroup(string resourceGroupName)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await DeleteResourceGroupAzureStack(resourceGroupName);
            }
            else
            {
                return await DeleteResourceGroupAzure(resourceGroupName);
            }
            
        }

        private async Task<AzureOperationResponse<bool>> CheckResourceGroupExistanceAzureStack(string resourceGroupName)
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

        private AzureOperationResponse<bool> CheckResourceGroupExistanceAzure(string resourceGroupName)
        {
            if (azure == null)
            {
                return new AzureOperationResponse<bool>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Azure is not instantiated"
                    }
                };
            }
            try
            {
                var resourceGroup = azure.ResourceGroups.CheckExistence(resourceGroupName);
                    
                return new AzureOperationResponse<bool>
                {
                    Body = resourceGroup
                };
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

        public async Task<AzureOperationResponse<bool>> CheckResourceGroupExistance(string resourceGroupName)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CheckResourceGroupExistanceAzureStack(resourceGroupName);
            }
            else
            {
                return CheckResourceGroupExistanceAzure(resourceGroupName);
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
                    if (String.Equals(provider.Body.RegistrationState, "registered", StringComparison.InvariantCultureIgnoreCase))
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
