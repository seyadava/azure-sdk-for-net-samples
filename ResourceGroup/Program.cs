namespace ResourceGroup
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;

    class Program
    {
        private const string ComponentName = "DotnetSDK";

        static void Main(string[] args)
        {
            //Set variables
            var location = "location";
            var baseUriString = "baseUriString";
            var resourceGroup1Name = "resourceGroupOneName";
            var resourceGroup2Name = "resourceGroupTwoName";
            var servicePrincipalId = "servicePrincipalID";
            var servicePrincipalSecret = "servicePrincipalSecret";
            var azureResourceId = "resourceID";
            var tenantId = "tenantID";
            var subscriptionId = "subscriptionID";
            
            Console.WriteLine("Get credential token");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            Console.WriteLine("Instantiate resource management client");
            var rmClient = GetResourceManagementClient(new Uri(baseUriString), credentials, subscriptionId);

            // Create resource group.
            try
            {
                Console.WriteLine(String.Format("Creating a resource group with name:{0}", resourceGroup1Name));
                var rmCreateTask = rmClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroup1Name,
                    new Profile2018ResourceManager.Models.ResourceGroup
                    {
                        Location = location
                    });
                rmCreateTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create resource group {0}. Exception: {1}", resourceGroup1Name, ex.Message));
            }

            // Update the resource group.
            try
            {
                Console.WriteLine(String.Format("Updating the resource group with name:{0}", resourceGroup1Name));
                var rmTagTask = rmClient.ResourceGroups.PatchWithHttpMessagesAsync(resourceGroup1Name, new Profile2018ResourceManager.Models.ResourceGroup
                {
                    Tags = new Dictionary<string, string> { { "DotNetTag", "DotNetValue" } }
                });

                rmTagTask.Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("Could not tag resource grooup {0}. Exception: {1}", resourceGroup1Name, ex.Message));
            }

            // Create another resource group.
            try
            {
                Console.WriteLine(String.Format("Creating a resource group with name:{0}", resourceGroup2Name));
                var rmCreateTask = rmClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroup2Name,
                    new Profile2018ResourceManager.Models.ResourceGroup
                    {
                        Location = location
                    });
                rmCreateTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create resource group {0}. Exception: {1}", resourceGroup2Name, ex.Message));
            }

            // List resource groups.
            try
            {
                Console.WriteLine("Listing all resource groups.");
                var rmListTask = rmClient.ResourceGroups.ListWithHttpMessagesAsync();
                rmListTask.Wait();

                var resourceGroupResults = rmListTask.Result.Body;
                foreach(var result in resourceGroupResults)
                {
                    Console.WriteLine(String.Format("Resource group name:{0}", result.Name));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not list resource groups.");
            }

            // Delete a resource group.
            try
            {
                Console.WriteLine(String.Format("Deleting resource group with name:{0}", resourceGroup2Name));
                var rmDeleteTask = rmClient.ResourceGroups.DeleteWithHttpMessagesAsync(resourceGroup2Name);
                rmDeleteTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not delete resource group {0}. Exception: {1}", resourceGroup2Name, ex.Message));
            }
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
