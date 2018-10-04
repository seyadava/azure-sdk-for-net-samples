namespace StorageAccount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;

    class Program
    {
        private const string ComponentName = "DotnetSDK";

        static void Main(string[] args)
        {
            //Set variables
            var location = "location";
            var baseUriString = "baseUriString";
            var resourceGroupName = "resourceGroupOneName";
            var servicePrincipalId = "servicePrincipalID";
            var servicePrincipalSecret = "servicePrincipalSecret";
            var azureResourceId = "resourceID";
            var tenantId = "tenantID";
            var subscriptionId = "subscriptionID";
            var storageAccountName = "storageAccountOne";
            var storageAccount2Name = "storageAccountTwo";
            
            Console.WriteLine("Get credential token");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            Console.WriteLine("Instantiate resource management client");
            var rmClient = GetResourceManagementClient(new Uri(baseUriString), credentials, subscriptionId);

            Console.WriteLine("Instantiate storage account client");
            var storageClient = GetStorageClient(new Uri(baseUriString), credentials, subscriptionId);

            // Create resource group.
            try
            {
                Console.WriteLine(String.Format("Creating a resource group with name:{0}", resourceGroupName));
                var rmCreateTask = rmClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    new Profile2018ResourceManager.Models.ResourceGroup
                    {
                        Location = location
                    });
                rmCreateTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create resource group {0}. Exception: {1}", resourceGroupName, ex.Message));
            }

            // Create storage account.
            try
            {
                Console.WriteLine(String.Format("Creating a storage account with name:{0}", storageAccountName));
                var storageProperties = new Profile2018Storage.Models.StorageAccountCreateParameters
                {
                    Location = location,
                    Kind = Profile2018Storage.Models.Kind.Storage,
                    Sku = new Profile2018Storage.Models.Sku(Profile2018Storage.Models.SkuName.StandardLRS)
                };

                var storageTask = storageClient.StorageAccounts.CreateWithHttpMessagesAsync(resourceGroupName, storageAccountName, storageProperties);
                storageTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create storage account {0}. Exception: {1}", storageAccountName, ex.Message));
            }

            // Get | regenerate storage account access keys.
            try
            {
                Console.WriteLine("Getting storage account access keys");
                var storageAccountKeysTask = storageClient.StorageAccounts.ListKeysWithHttpMessagesAsync(resourceGroupName, storageAccountName);
                storageAccountKeysTask.Wait();
                var storageAccountKeysResults = storageAccountKeysTask.Result?.Body?.Keys;

                foreach (var key in storageAccountKeysResults)
                {
                    Console.WriteLine(String.Format("Storage account key name: {0}, key value: {1}", key.KeyName, key.Value));
                }

                Console.WriteLine("Regenerating first storage account access key");
                var storageAccountRegenerateTask = storageClient.StorageAccounts.RegenerateKeyWithHttpMessagesAsync(
                    resourceGroupName, 
                    storageAccountName, 
                    new Profile2018Storage.Models.StorageAccountRegenerateKeyParameters{
                        KeyName = storageAccountKeysResults[0].KeyName
                    });
                storageAccountRegenerateTask.Wait();
                var storageAccountRegenerateResults = storageAccountRegenerateTask.Result?.Body?.Keys;
                foreach (var key in storageAccountRegenerateResults)
                {
                    Console.WriteLine(String.Format("Storage account key name: {0}, key value: {1}", key.KeyName, key.Value));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create storage account {0}. Exception: {1}", storageAccountName, ex.Message));
            }

            // Create another storage account.
            try
            {
                Console.WriteLine(String.Format("Creating a storage account with name: {0}", storageAccount2Name));
                var storageProperties = new Profile2018Storage.Models.StorageAccountCreateParameters
                {
                    Location = location,
                    Kind = Profile2018Storage.Models.Kind.Storage,
                    Sku = new Profile2018Storage.Models.Sku(Profile2018Storage.Models.SkuName.StandardLRS)
                };

                var storageTask = storageClient.StorageAccounts.CreateWithHttpMessagesAsync(resourceGroupName, storageAccount2Name, storageProperties);
                storageTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create storage account {0}. Exception: {1}", storageAccount2Name, ex.Message));
            }

            // Update storage account by enabling encryption.
            try
            {
                Console.WriteLine(String.Format("Enabling blob encryption for the storage account: {0}", storageAccount2Name));
                var storageAccountUpdateTask = storageClient.StorageAccounts.UpdateWithHttpMessagesAsync(resourceGroupName, storageAccount2Name, new Profile2018Storage.Models.StorageAccountUpdateParameters
                {
                    Encryption = new Profile2018Storage.Models.Encryption(new Profile2018Storage.Models.EncryptionServices
                    {
                        Blob = new Profile2018Storage.Models.EncryptionService()
                    })
                });

                storageAccountUpdateTask.Wait();
                var status = storageAccountUpdateTask.Result?.Body?.Encryption?.Services?.Blob?.Enabled.Value ;
                if (status.HasValue && status.Value)
                {
                    Console.WriteLine(String.Format("Encryption status of the service  {0} is enabled", storageAccount2Name));
                }
                else
                {
                    Console.WriteLine(String.Format("Encryption status of the service  {0} is not enabled", storageAccount2Name));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not enable blob encryption for storage account {0}. Exception: {1}", storageAccount2Name, ex.Message));
            }

            // List storage accounts.
            var storageAccountResults = new List<Profile2018Storage.Models.StorageAccount>();
            try
            {
                Console.WriteLine("Listing storage accounts");
                var storageAccountListTask = storageClient.StorageAccounts.ListByResourceGroupWithHttpMessagesAsync(resourceGroupName);
                storageAccountListTask.Wait();
                storageAccountResults = storageAccountListTask.Result?.Body.ToList();

                foreach (var storageAccount in storageAccountResults)
                {
                    Console.WriteLine(String.Format("Storage account name: {0}, created at: {1}", storageAccount.Name, storageAccount.CreationTime.ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not list storage accounts. Exception: {0}", ex.Message));
            }

            // Delete storage accounts.
            try
            {
                foreach(var storageAccount in storageAccountResults)
                {
                    Console.WriteLine(String.Format("Deleting a storage account with name: {0}", storageAccount.Name));

                    var storageDeleteTask = storageClient.StorageAccounts.DeleteWithHttpMessagesAsync(resourceGroupName, storageAccount.Name);
                    storageDeleteTask.Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not delete storage accounts. Exception: {0}", ex.Message));
            }
        }

        private static Profile2018Storage.StorageManagementClient GetStorageClient(Uri baseUri, CustomLoginCredentials customCredential, string subscriptionId)
        {
            var client = new Profile2018Storage.StorageManagementClient(baseUri: baseUri, credentials: customCredential)
            {
                SubscriptionId = subscriptionId
            };
            client.SetUserAgent(ComponentName);

            return client;
        }

        private static Profile2018ResourceManager.ResourceManagementClient GetResourceManagementClient(Uri baseUri, CustomLoginCredentials customCredential, string subscriptionId)
        {
            var client = new Profile2018ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: customCredential)
            {
                SubscriptionId = subscriptionId
            };
            client.SetUserAgent(ComponentName);

            return client;
        }
    }
}
