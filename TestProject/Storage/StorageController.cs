namespace Storage
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;
    using Microsoft.Rest.Azure;

    public class StorageController
    {
        private const string ComponentName = "DotnetSDK_StorageController";
        private readonly CustomLoginCredentials credential;
        private readonly string subscriotionId;
        private Uri baseUri;
        private static Profile2018Storage.StorageManagementClient client;

        public StorageController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier
            )
        {
            this.baseUri = baseUri;
            this.credential = credentials;
            this.subscriotionId = subscriptionIdentifier;

            GetStorageAccountClient();
        }

        private void GetStorageAccountClient()
        {
            if (client != null)
            {
                return;
            }
            client = new Profile2018Storage.StorageManagementClient(baseUri: this.baseUri, credentials: this.credential);
            client.SubscriptionId = this.subscriotionId;
            client.SetUserAgent(ComponentName);
        }

        public async Task<AzureOperationResponse<Profile2018Storage.Models.StorageAccount>> CreateStorageAccount(
            string storageAccountName,
            string resourceGroupName,
            string location,
            Profile2018Storage.Models.SkuName sku)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Storage.Models.StorageAccount>
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
                var storageAccountProperties = new Profile2018Storage.Models.StorageAccountCreateParameters
                {
                    Location = location,
                    Kind = Profile2018Storage.Models.Kind.Storage,
                    Sku = new Profile2018Storage.Models.Sku(sku)
                };
                var storageAccountTask = await client.StorageAccounts.CreateWithHttpMessagesAsync(
                    resourceGroupName,
                    storageAccountName,
                    storageAccountProperties);
                return storageAccountTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Storage.Models.StorageAccount>
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
