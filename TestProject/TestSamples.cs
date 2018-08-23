using System;
using System.Threading.Tasks;
using Authorization;
using Resource;
using Xunit;


namespace TestProject
{
    public class TestSamples
    {
        [Fact]
        public async Task CreateResourceGroupTest()
        {
            const string subscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";
            const string location = "eastus";
            var baseUri = new Uri("https://management.azure.com/");
            string servicePrincipalId = "5acab3e0-d042-49e0-86e1-cca5c52c165b";
            string servicePrincipalSecret = "683c1b3e-5479-451d-9186-9ee6b5f130b7";
            string azureEnvironmentResourceId = "https://management.core.windows.net/";
            string azureEnvironmentTenandId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

            //const string location = "redmond";
            //var baseUri = new Uri("https://management.redmond.ext-v.masd.stbtest.microsoft.com/");

            var credentials = new CustomLoginCredentials(
                servicePrincipalId, servicePrincipalSecret, azureEnvironmentResourceId, azureEnvironmentTenandId);

            var resourceController = new ResourcesController(baseUri, credentials, subscriptionId);
            var rgname = "test-dotnet-rg-3";
            var rg = await resourceController.CreateResourceGroup(rgname, location);

            Assert.NotNull(rg);
            Assert.True(String.Equals("Succeeded", rg.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(rgname, rg.Name);
            Assert.NotEmpty(rg.Id);
        }

        [Fact]
        public async Task DeleteResourceGroupTest()
        {
            const string subscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";
            var baseUri = new Uri("https://management.azure.com/");
            string servicePrincipalId = "5acab3e0-d042-49e0-86e1-cca5c52c165b";
            string servicePrincipalSecret = "683c1b3e-5479-451d-9186-9ee6b5f130b7";
            string azureEnvironmentResourceId = "https://management.core.windows.net/";
            string azureEnvironmentTenandId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

            //const string location = "redmond";
            //var baseUri = new Uri("https://management.redmond.ext-v.masd.stbtest.microsoft.com/");

            var credentials = new CustomLoginCredentials(
                servicePrincipalId, servicePrincipalSecret, azureEnvironmentResourceId, azureEnvironmentTenandId);

            var resourceController = new ResourcesController(baseUri, credentials, subscriptionId);
            var rgname = "test-dotnet-rg-3";
            var rg = await resourceController.DeleteResourceGroup(rgname);

            Assert.True(rg.IsSuccessStatusCode);
        }
        
        [Fact]
        public async Task RegisterResourceProviderTest()
        {
            const string subscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";
            var baseUri = new Uri("https://management.azure.com/");
            string servicePrincipalId = "5acab3e0-d042-49e0-86e1-cca5c52c165b";
            string servicePrincipalSecret = "683c1b3e-5479-451d-9186-9ee6b5f130b7";
            string azureEnvironmentResourceId = "https://management.core.windows.net/";
            string azureEnvironmentTenandId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

            //const string location = "redmond";
            //var baseUri = new Uri("https://management.redmond.ext-v.masd.stbtest.microsoft.com/");

            var credentials = new CustomLoginCredentials(
                servicePrincipalId, servicePrincipalSecret, azureEnvironmentResourceId, azureEnvironmentTenandId);

            var resourceController = new ResourcesController(baseUri, credentials, subscriptionId);
            var rpname = "Microsoft.Compute";
            var rp = await resourceController.RegisterResourceProvider(rpname);

            Assert.NotNull(rp);
            Assert.True(String.Equals("registered", rp.RegistrationState, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
