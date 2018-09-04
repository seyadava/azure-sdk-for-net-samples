namespace TestProject
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Authorization;
    using Compute;
    using Network;
    using Resource;
    using Storage;
    using Xunit;
    using Microsoft.Rest.Azure;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;
    using Microsoft.Azure.Management.ResourceManager.Fluent;

    public class TestSamples
    {
        [Fact]
        public async Task CreateResourceGroupTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // VALIDATION
            Assert.NotNull(resourceGroup.Body);
            Assert.NotEmpty(resourceGroup.Body.Id);
        }

        [Fact]
        public async Task DeleteResourceGroupTest()
        {
            // SET PARAMETERS
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);

            // DELETE RESOURCE GROUP
            var resourceGroup = await resourceController.DeleteResourceGroup(resourceGroupName);

            // VALIDATION
            Assert.True(resourceGroup.IsSuccessStatusCode);
        }
        
        [Fact]
        public async Task RegisterResourceProviderTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var resourceProvidersName = Environment.GetEnvironmentVariable("AZURE_RESOURCEPROVIDERS");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);

            // REGISTER RESOURCE PROVIDER
            var providers = resourceProvidersName.Split(';');

            foreach(var resourceProviderName in providers)
            {
                var resourceProvider = await resourceController.RegisterResourceProvider(resourceProviderName);
                
                // VALIDATION
                Assert.NotNull(resourceProvider.Body);
                Assert.Equal("registered", resourceProvider.Body.RegistrationState, ignoreCase: true);
            }
        }

        [Fact]
        public async Task ResourceGroupExistanceCheckTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CHECK RESOURCE GROUP
            var resourceGroupExistance = await resourceController.CheckResourceGroupExistance(resourceGroupName);

            // CHECK RESOURCE GROUP VALIDATION
            Assert.True(resourceGroupExistance.Body);
        }

        [Fact]
        public async Task CreateDynamicPubliIPAddressTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);

            // VALIDATION
            Assert.NotNull(ip.Body);
            Assert.NotEmpty(ip.Body);
        }

        [Fact]
        public async Task CreateStaticPubliIPAddressTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);
            
            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location, "Static");

            // VALIDATION
            Assert.NotNull(ip.Body);            
            Assert.NotEmpty(ip.Body);
        }

        [Fact]
        public async Task CreateVnetWithSubnetTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var vnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var subnetNames = Environment.GetEnvironmentVariable("AZURE_SUBNET_NAMES");
            var subnetAddresses = Environment.GetEnvironmentVariable("AZURE_SUBNET_ADDRESSES");
            var vnetAddresses = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE VNET
            var vnetAddressSpaces = vnetAddresses.Split(';');
            var subNames = subnetNames.Split(';');
            var suAddresses = subnetAddresses.Split(';');
            var subnets = new Dictionary<string, string>();
            for (int i = 0; i < subNames.Length; i++)
            {
                subnets.Add(subNames[i], suAddresses[i]);
            }
            
            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, resourceGroupName, location, subnets);

            // VALIDATION
            Assert.NotNull(vnet.Body);
            Assert.NotEmpty(vnet.Body);
        }

        [Fact]
        public async Task AddSubnetToExistingVnetTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var vnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var vnetAddressSpace = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            var newSubnetName = "test-dotnet-newsubnet";
            var newSubnetAddress = "10.0.16.0/24";

            // SET CONTROLLERS
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // RESOURCE GROUP CREATION VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE VNET
            var vnetAddressSpaces = vnetAddressSpace.Split(';');
            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, resourceGroupName, location);

            // VNET CREATION VALIDATION
            Assert.NotNull(vnet.Body);

            // ADD NEW SUBNET
            var subnet = await networkController.AddSubnet(newSubnetName, vnetName, newSubnetAddress, resourceGroupName);

            // SUBNET CREATION VALIDATION
            Assert.NotNull(subnet.Body);
            Assert.Equal("Succeeded", subnet.Body.ProvisioningState, ignoreCase: true);
            Assert.Equal(newSubnetName, subnet.Body.Name);
            Assert.NotEmpty(subnet.Body.Id);
        }

        [Fact]
        public async Task GetPubliIPAddressTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLERS
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // RESOURCE GROUP CREATION VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location, "Static");

            // CREATE IP VALIDATION
            Assert.NotNull(ip.Body);

            // GET IP
            var ipTask = await networkController.GetPublicIpAddress(ipName, resourceGroupName);

            // VALIDATION
            Assert.NotNull(ipTask.Body);
            Assert.Equal(ipName, ipTask.Body.Name);
            Assert.NotEmpty(ipTask.Body.Id);
        }

        [Fact]
        public async Task CreateNetworkInterfaceTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var vnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var subnetNames = Environment.GetEnvironmentVariable("AZURE_SUBNET_NAMES");
            var subnetAddresses = Environment.GetEnvironmentVariable("AZURE_SUBNET_ADDRESSES");
            var vnetAddresses = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var nicName = Environment.GetEnvironmentVariable("AZURE_NIC_NAME");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE VNET
            var vnetAddressSpaces = vnetAddresses.Split(';');
            var subNames = subnetNames.Split(';');
            var suAddresses = subnetAddresses.Split(';');
            var subnets = new Dictionary<string, string>();
            for (int i = 0; i < subNames.Length; i++)
            {
                subnets.Add(subNames[i], suAddresses[i]);
            }

            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, resourceGroupName, location, subnets);

            // CREATE VNET VALIDATION
            Assert.NotNull(vnet.Body);

            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);

            // CREATE IP VALIDATION
            Assert.NotNull(ip.Body);

            // CREATE NIC
            var nic = await networkController.CreateNetworkInterface(nicName, resourceGroupName, vnetName, subNames[0], ipName, location);

            // VALIDATION
            Assert.NotNull(nic.Body);
            Assert.Equal("Succeeded", nic.Body.ProvisioningState, ignoreCase: true);
        }

        [Fact]
        public async Task CreateStorageAccountTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var storageNamePrefix = Environment.GetEnvironmentVariable("AZURE_STORAGENAME_PREFIX");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            var storageAccountName = string.Format("{0}{1}", storageNamePrefix, new Random().Next(0, 99));
            var storageAccountSku = Profile2018Storage.Models.SkuName.StandardLRS;

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var storageController = new StorageController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);
            
            // CREATE STORAGE ACCOUNT
            var storageAccount = await storageController.CreateStorageAccount(storageAccountName, resourceGroupName, location, storageAccountSku);

            // VALIDATION
            Assert.NotNull(storageAccount.Body);
            Assert.Equal("Succeeded", storageAccount.Body.ProvisioningState.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task CreateLinuxVirtualMachineTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var vmName = Environment.GetEnvironmentVariable("AZURE_VM_NAME");
            var vnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var subnetNames = Environment.GetEnvironmentVariable("AZURE_SUBNET_NAMES");
            var subnetAddresses = Environment.GetEnvironmentVariable("AZURE_SUBNET_ADDRESSES");
            var vnetAddresses = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var nicName = Environment.GetEnvironmentVariable("AZURE_NIC_NAME");
            var storageNamePrefix = Environment.GetEnvironmentVariable("AZURE_STORAGENAME_PREFIX");
            var storageEndpoint = Environment.GetEnvironmentVariable("AZURE_STORAGE_ENDPOINT");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            var storageAccountName = string.Format("{0}{1}", storageNamePrefix, new Random().Next(0, 99));
            var storageAccountSku = Profile2018Storage.Models.SkuName.StandardLRS;

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentialsFromFile);
            var storageController = new StorageController(new Uri(baseUriString), credentialsFromFile);
            var computerController = new ComputeController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE VNET
            var vnetAddressSpaces = vnetAddresses.Split(';');
            var subNames = subnetNames.Split(';');
            var suAddresses = subnetAddresses.Split(';');
            var subnets = new Dictionary<string, string>();
            for (int i = 0; i < subNames.Length; i++)
            {
                subnets.Add(subNames[i], suAddresses[i]);
            }

            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, resourceGroupName, location, subnets);

            // CREATE VNET VALIDATION
            Assert.NotNull(vnet.Body);

            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);

            // CREATE IP VALIDATION
            Assert.NotNull(ip.Body);

            // CREATE NIC
            var nic = await networkController.CreateNetworkInterface(nicName, resourceGroupName, vnetName, subNames[0], ipName, location);

            // CREATE NIC VALIDATION
            Assert.NotNull(nic.Body);
            Assert.Equal("Succeeded", nic.Body.ProvisioningState, ignoreCase: true);

            // CREATE STORAGE ACCOUNT
            var storageAccount = await storageController.CreateStorageAccount(storageAccountName, resourceGroupName, location, storageAccountSku);

            // STORAGE ACCOUNT VALIDATION
            Assert.NotNull(storageAccount.Body);
            Assert.Equal("Succeeded", storageAccount.Body.ProvisioningState.ToString(), ignoreCase: true);

            // CREATE VM
            var vm = await computerController.CreateVirtialMachine(resourceGroupName, vmName, storageAccountName, storageEndpoint, nic.Body.Id, location);

            // VALIDATION
            Assert.NotNull(vm.Body);
            Assert.Equal("Succeeded", vm.Body.ProvisioningState.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task CreateDataDiskTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            var diskName = Environment.GetEnvironmentVariable("AZURE_DISK_NAME");
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var computerController = new ComputeController(new Uri(baseUriString), credentialsFromFile);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE DISK
            var disk = await computerController.CreateDisk(resourceGroupName, diskName, 1, location);

            // CREATE DISK VALIDATION
            Assert.NotNull(disk.Body);
            Assert.Equal("Succeeded", disk.Body.ProvisioningState.ToString(), ignoreCase: true);
        }

        [Fact]
        public async Task CreateLinuxVirtualMachineWithManagedDiskTest()
        {
            // SET PARAMETERS
            var location = Environment.GetEnvironmentVariable("AZURE_LOCATION");
            var baseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCEGROUP");
            var servicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var servicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var vmName = Environment.GetEnvironmentVariable("AZURE_VM_NAME");
            var vnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var subnetNames = Environment.GetEnvironmentVariable("AZURE_SUBNET_NAMES");
            var subnetAddresses = Environment.GetEnvironmentVariable("AZURE_SUBNET_ADDRESSES");
            var vnetAddresses = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var ipName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var nicName = Environment.GetEnvironmentVariable("AZURE_NIC_NAME");
            var diskName = Environment.GetEnvironmentVariable("AZURE_DISK_NAME");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            // SET CONTROLLER
            var resourceController = new ResourcesController(new Uri(baseUriString), credentials, subscriptionId);
            var networkController = new NetworkController(new Uri(baseUriString), credentials, subscriptionId);
            var computerController = new ComputeController(new Uri(baseUriString), credentials, subscriptionId);

            // CREATE RESOURCE GROUP
            var resourceGroup = await resourceController.CreateResourceGroup(resourceGroupName, location);

            // CREATE RESOURCE GROUP VALIDATION
            Assert.NotNull(resourceGroup.Body);

            // CREATE VNET
            var vnetAddressSpaces = vnetAddresses.Split(';');
            var subNames = subnetNames.Split(';');
            var suAddresses = subnetAddresses.Split(';');
            var subnets = new Dictionary<string, string>();
            for (int i = 0; i < subNames.Length; i++)
            {
                subnets.Add(subNames[i], suAddresses[i]);
            }

            var vnet = await networkController.CreateVirtualNetwork(vnetName, vnetAddressSpaces, resourceGroupName, location, subnets);

            // CREATE VNET VALIDATION
            Assert.NotNull(vnet.Body);

            // CREATE IP
            var ip = await networkController.CreatePublicIpAddress(ipName, resourceGroupName, location);

            // CREATE IP VALIDATION
            Assert.NotNull(ip.Body);

            // CREATE NIC
            var nic = await networkController.CreateNetworkInterface(nicName, resourceGroupName, vnetName, subNames[0], ipName, location);

            // CREATE NIC VALIDATION
            Assert.NotNull(nic.Body);
            Assert.Equal("Succeeded", nic.Body.ProvisioningState, ignoreCase: true);

            // CREATE DISK
            var disk = await computerController.CreateDisk(resourceGroupName, diskName, 1, location);

            // CREATE DISK VALIDATION
            Assert.NotNull(disk.Body);
            Assert.Equal("Succeeded", disk.Body.ProvisioningState.ToString(), ignoreCase: true);

            // CREATE VM
            var vm = await computerController.CreateVirtialMachineWithManagedDisk(resourceGroupName, vmName, nic.Body.Id, disk.Body.Id, location);

            // VALIDATION
            Assert.NotNull(vm.Body);
            Assert.Equal("Succeeded", vm.Body.ProvisioningState.ToString(), ignoreCase: true);
        }
    }
}
