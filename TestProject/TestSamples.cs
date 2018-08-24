namespace TestProject
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Authorization;
    using Network;
    using Resource;
    using Storage;
    using Xunit;
    using Microsoft.Rest.Azure;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;

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

            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(resourceGroupName, resourceGroup.Body.Name);
            Assert.NotEmpty(resourceGroup.Body.Id);
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.DeleteResourceGroup(resourceGroupName);

            Assert.True(resourceGroup.IsSuccessStatusCode);
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
            var resourceProviderName = "Microsoft.Compute";
            var resourceProvider = await resourceController.RegisterResourceProvider(resourceProviderName);

            Assert.NotNull(resourceProvider.Body);
            Assert.True(String.Equals("registered", resourceProvider.Body.RegistrationState, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task ResourceGroupExistanceCheckTest()
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var resourceGroupExistance = await resourceController.CheckResourceGroupExistance(resourceGroupName);
            Assert.True(resourceGroupExistance.Body);
        }

        [Fact]
        public async Task CreateDynamicPubliIPAddressTest()
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var networkController = new NetworkController(baseUri, credentials, subscriptionId);
            var ipName = "test-dotnet-publicip";
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);
            Assert.NotNull(ip.Body);
            Assert.True(String.Equals("Succeeded", ip.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(ipName, ip.Body.Name);
            Assert.NotEmpty(ip.Body.Id);
            Assert.True(String.Equals("Dynamic", ip.Body.PublicIPAllocationMethod, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task CreateStaticPubliIPAddressTest()
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var networkController = new NetworkController(baseUri, credentials, subscriptionId);
            var ipName = "test-dotnet-publicip-static";
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location, "Static");
            Assert.NotNull(ip.Body);
            Assert.True(String.Equals("Succeeded", ip.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(ipName, ip.Body.Name);
            Assert.NotEmpty(ip.Body.Id);
            Assert.True(String.Equals("Static", ip.Body.PublicIPAllocationMethod, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(!String.IsNullOrEmpty(ip.Body.IpAddress));
        }

        [Fact]
        public async Task CreateVnetWithSubnetTest()
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var networkController = new NetworkController(baseUri, credentials, subscriptionId);

            var vnetName = "test-dotnet-vnet";
            var vnetAddressSpaces = new List<string> { "10.0.0.0/16" };
            var subnets = new Dictionary<string, string> { { "test-dotnet-subnet1", "10.0.0.0/24" } };
            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, subnets, resourceGroupName, location);
            Assert.NotNull(vnet.Body);
            Assert.True(String.Equals("Succeeded", vnet.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(vnetName, vnet.Body.Name);
            Assert.Equal(1, vnet.Body.Subnets.Count);
            Assert.NotEmpty(vnet.Body.Id);
        }

        [Fact]
        public async Task AddSubnetToExistingVnetTest()
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
            var resourceGroupName = "test-dotnet-rg-3";
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var networkController = new NetworkController(baseUri, credentials, subscriptionId);

            var vnetName = "test-dotnet-vnet";

            var subnetName = "test-dotnet-newsubnet";
            var subnetAddress = "10.0.1.0/24";
            var subnet = await networkController.AddSubnet(subnetName, vnetName, subnetAddress, resourceGroupName);
            Assert.NotNull(subnet.Body);
            Assert.True(String.Equals("Succeeded", subnet.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(subnetName, subnet.Body.Name);
            Assert.NotEmpty(subnet.Body.Id);
        }

        [Fact]
        public async Task GetPubliIPAddressTest()
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

            var ipName = "test-dotnet-publicip-static";
            var resourceGroupName = "test-dotnet-rg-3";

            var networkController = new NetworkController(baseUri, credentials, subscriptionId);

            var ip = await networkController.GetPublicIpAddress(ipName, resourceGroupName);
            Assert.NotNull(ip.Body);
            Assert.Equal(ipName, ip.Body.Name);
            Assert.NotEmpty(ip.Body.Id);
        }

        [Fact]
        public async Task CreateNetworkInterfaceTest()
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

            var resourceGroupName = "test-dotnet-rg-3";
            var virtualNetworkName = "test-dotnet-vnet";
            var vnetAddressSpaces = new List<string> { "10.0.0.0/16" };
            var subnetName = "test-dotnet-subnet";
            var subnets = new Dictionary<string, string> { { subnetName, "10.0.0.0/20" } };
            
            var ipName = "test-pip";
            var nicName = "test-dotnet-nic";

            var credentials = new CustomLoginCredentials(
                servicePrincipalId, servicePrincipalSecret, azureEnvironmentResourceId, azureEnvironmentTenandId);

            var resourceController = new ResourcesController(baseUri, credentials, subscriptionId);
            var networkController = new NetworkController(baseUri, credentials, subscriptionId);
            
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);
            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);
            Assert.NotNull(ip.Body);
            Assert.True(String.Equals("Succeeded", ip.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var vnet = await networkController.CreateVirtualNetwork(virtualNetworkName, vnetAddressSpaces, subnets, resourceGroupName, location);
            Assert.NotNull(vnet.Body);
            Assert.True(String.Equals("Succeeded", vnet.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var nic = await networkController.CreateNetworkInterface(nicName, resourceGroupName, virtualNetworkName, subnetName, ipName, location);
            Assert.NotNull(nic.Body);
            Assert.True(String.Equals("Succeeded", nic.Body.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task CreateStorageAccountTest()
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

            var resourceGroupName = "test-dotnet-rg-3";
            var storageAccountName = string.Format("teststorageaccount{0}", new Random().Next(0,99));
            var storageAccountSku = Profile2018Storage.Models.SkuName.StandardLRS;

            var resourceController = new ResourcesController(baseUri, credentials, subscriptionId);
            var storageController = new StorageController(baseUri, credentials, subscriptionId);

            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);
            Assert.NotNull(resourceGroup.Body);
            Assert.True(String.Equals("Succeeded", resourceGroup.Body.Properties.ProvisioningState, StringComparison.InvariantCultureIgnoreCase));

            var storageAccount = await storageController.CreateStorageAccount(storageAccountName, resourceGroupName, location, storageAccountSku);
            Assert.NotNull(storageAccount.Body);
            Assert.True(String.Equals("Succeeded", storageAccount.Body.ProvisioningState.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
