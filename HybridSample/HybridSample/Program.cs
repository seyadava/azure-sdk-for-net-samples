namespace HybridSample
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Authorization;
    using Compute;
    using Network;
    using Resource;
    using Storage;
    using Microsoft.Rest.Azure;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    class Program
    {
        static void Main(string[] args)
        {
            //Set Azure environment variables
            var azureLocation = "westus";
            var azureBaseUrl = "https://management.azure.com/";
            var azureResourceGroupName = "test-hybrid-dotnet-azure-resourcegroup";
            var azureCredentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            var azureVnetName = Environment.GetEnvironmentVariable("AZURE_VNET_NAME");
            var azureSubnetName = Environment.GetEnvironmentVariable("AZURE_SUBNET_NAMES");
            var azureSubnetAddressSpace = Environment.GetEnvironmentVariable("AZURE_SUBNET_ADDRESSES");
            var azureVnetAddress = Environment.GetEnvironmentVariable("AZURE_VNET_ADDRESSES");
            var azureIpName = Environment.GetEnvironmentVariable("AZURE_IP_NAME");
            var azureNicName = Environment.GetEnvironmentVariable("AZURE_NIC_NAME");
            var azureVmName = Environment.GetEnvironmentVariable("AZURE_VM_NAME");
            

            //Set Azure stack environment variables
            var stackLocation = "redmond";
            var stackResourceGroupName = "test-hybrid-dotnet-stack-resourcegroup";
            var stackBaseUriString = Environment.GetEnvironmentVariable("AZURE_BASE_URL");
            var stackServicePrincipalId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var stackServicePrincipalSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var stackResourceId = Environment.GetEnvironmentVariable("AZURE_RESOURCE_ID");
            var stackTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var stackSubscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
            var stackCredentials = new CustomLoginCredentials(
                stackServicePrincipalId, stackServicePrincipalSecret, stackResourceId, stackTenantId);
            var stackDiskName = Environment.GetEnvironmentVariable("AZURE_DISK_NAME");

            //Set controllers
            var stackResourceController = new ResourcesController(new Uri(stackBaseUriString), stackCredentials, stackSubscriptionId);
            var azureResourceController = new ResourcesController(new Uri(azureBaseUrl), azureCredentials, "azure");
            var azureNetworkController = new NetworkController(new Uri(azureBaseUrl), azureCredentials, "azure");
            var stackComputeController = new ComputeController(new Uri(stackBaseUriString), stackCredentials, stackSubscriptionId);
            var azureComputeController = new ComputeController(new Uri(azureBaseUrl), azureCredentials, "azure");

            //Create resource group in Azure
            var azureRgTask = azureResourceController.CreateResourceGroup(azureResourceGroupName, azureLocation);
            azureRgTask.Wait();

            //Create resource group in Azure Stack
            var stackRgTask = stackResourceController.CreateResourceGroup(stackResourceGroupName, stackLocation);
            stackRgTask.Wait();

            //Create NIC on Azure
            var nicTask = azureNetworkController.CreateNetworkInterface(
                azureNicName, 
                azureResourceGroupName, 
                azureVnetName, 
                azureVnetAddress, 
                azureSubnetName, 
                azureSubnetAddressSpace, 
                azureIpName, 
                azureLocation);
            nicTask.Wait();

            //Create disk on Azure Stack
            var diskTask = stackComputeController.CreateDisk(
                stackResourceGroupName,
                stackDiskName,
                1,
                stackLocation);
            diskTask.Wait();

            //Create a VM on Azure with its data on Azure Stack
            var vmTask = azureComputeController.CreateVirtialMachineWithManagedDisk(
                azureResourceGroupName,
                azureVmName,
                nicTask.Result.Body,
                diskTask.Result.Body,
                azureLocation);
            vmTask.Wait();
        }
    }
}
