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
        private readonly string subscriotionId;
        private readonly CustomLoginCredentials customCredential;
        private readonly string environment;
        private readonly Uri baseUri;
        private static Profile2018ResourceManager.ResourceManagementClient client;
        private static IAzure azure;

        public ResourcesController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier,
            string environment = "azurestack")
        {
            this.baseUri = baseUri;
            this.customCredential = credentials;
            this.subscriotionId = subscriptionIdentifier;
            this.environment = environment;
            GetResourceGroupClient();
        }

        public ResourcesController(
            Uri baseUri,
            AzureCredentials credentials,
            string environment = "azurestack")
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                this.baseUri = baseUri;
                this.azureCredential = credentials;
                GetResourceGroupClient();
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

        private void GetResourceGroupClient()
        {
            if (client != null)
            {
                return;
            }
            if (customCredential != null)
            {
                client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: customCredential)
                {
                    SubscriptionId = this.subscriotionId
                };
            }
            else
            {
                client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: azureCredential)
                {
                    SubscriptionId = this.azureCredential.DefaultSubscriptionId
                };
            }

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
    }
}