using cp = Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure;
using profile2017 = Microsoft.Azure.Management.Profiles.profile_2017_03_09;
using profile2018 = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01;
using stackaut = Microsoft.Azure.Management.Profiles.profile_2017_03_09.Authorization;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
namespace sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string subscriptionId = "23be27d7-0237-47fd-a012-9d691c6a3d83";
            const string subscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";
            const string location = "redmond";
            //const string location = "eastus";
            var baseUri = new Uri("https://management.redmond.ext-v.masd.stbtest.microsoft.com/");

            string servicePrincipalId = "";
            string servicePrincipalSecret = "";
            string azureEnvironmentResourceId = "";
            string azureEnvironmentTenandId = "";

            var credentials = new CustomLoginCredentials(
                servicePrincipalId: servicePrincipalId, 
                servicePrincipalSecret: servicePrincipalSecret, 
                azureEnvironmentResourceId: azureEnvironmentResourceId, 
                azureEnvironmentTenandId: azureEnvironmentTenandId);
            var credentialsFromFile = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));


            var networkclient = GetNetworkClient(baseUri, credentialsFromFile, subscriptionId);
            var resourceGroupClient = GetResourceGroupClient(baseUri, credentialsFromFile, subscriptionId);
            var computeClient = GetComputeClient(baseUri, credentialsFromFile, subscriptionId);
            var storageClient = GetStorageClient(baseUri, credentialsFromFile, subscriptionId);
            var subscriptionClient = GetSubscriptionClient(baseUri, credentials);


            var rgname = "test-dotnet-rg2";
            var rg = resourceGroupClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                resourceGroupName: rgname,
                parameters: new profile2017.ResourceManager.Models.ResourceGroup
                {
                    Location = location
                }).Result;

            if (!rg.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create resource group");
            }

            var t = subscriptionClient.SubscriptionDefinitions.CreateWithHttpMessagesAsync("subs", new profile2018.Subscription.Models.SubscriptionDefinition
            {
                OfferType = 
            })

            var publicIpName = "test-dotnet-publicIp";
            //var publicIp = new stacknetwork.Models.PublicIPAddress
            var publicIp = new profile2017.Network.Models.PublicIPAddress
            {
                Location = location,
                Tags = new Dictionary<string, string>
                {
                    {"key", "value" }
                },
                //PublicIPAllocationMethod = stacknetwork.Models.IPAllocationMethod.Dynamic
                PublicIPAllocationMethod = profile2017.Network.Models.IPAllocationMethod.Dynamic
            };

            var pip = networkclient.PublicIPAddresses.CreateOrUpdateWithHttpMessagesAsync(rgname, publicIpName, publicIp).Result;
            if (!pip.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create public IP");
            }

            var vnetName = "test-dotnet-vnet";
            var subnetName = "subnet1";
            //var vnet = new stacknetwork.Models.VirtualNetwork
            var vnet = new profile2017.Network.Models.VirtualNetwork
            {
                Location = location,
                //AddressSpace = new stacknetwork.Models.AddressSpace
                AddressSpace = new profile2017.Network.Models.AddressSpace
                {
                    AddressPrefixes = new List<string> { "10.0.0.0/16" }
                },
                //Subnets = new List<stacknetwork.Models.Subnet>
                Subnets = new List<profile2017.Network.Models.Subnet>
                {
                    //new stacknetwork.Models.Subnet
                    new profile2017.Network.Models.Subnet
                    {
                        Name = subnetName,
                        AddressPrefix = "10.0.0.0/24"
                    }
                }
            };

            var vnetResponse = networkclient.VirtualNetworks.CreateOrUpdateWithHttpMessagesAsync(rgname, vnetName, vnet).Result;
            if (!vnetResponse.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create virtual network");
            }

            var getPublicIpAddressResponse = networkclient.PublicIPAddresses.GetWithHttpMessagesAsync(rgname, publicIpName).Result.Body;
            var getSubnetResponse = networkclient.Subnets.GetWithHttpMessagesAsync(rgname, vnetName, subnetName).Result.Body;



            var nicName = "test-dotnet-nic";
            var ipconfigName = "test-dotnet-ipconfig";
            //var nic = new stacknetwork.Models.NetworkInterface
            var nic = new profile2017.Network.Models.NetworkInterface
            {
                Location = location,
                Tags = new Dictionary<string, string> { { "key", "value" } },
                //IpConfigurations = new List<stacknetwork.Models.NetworkInterfaceIPConfiguration>
                IpConfigurations = new List<profile2017.Network.Models.NetworkInterfaceIPConfiguration>
                {
                    //new stacknetwork.Models.NetworkInterfaceIPConfiguration
                    new profile2017.Network.Models.NetworkInterfaceIPConfiguration
                    {
                        Name = ipconfigName,
                        //PrivateIPAllocationMethod = stacknetwork.Models.IPAllocationMethod.Dynamic,
                        PrivateIPAllocationMethod = profile2017.Network.Models.IPAllocationMethod.Dynamic,
                        //PublicIPAddress = new stacknetwork.Models.PublicIPAddress
                        PublicIPAddress = new profile2017.Network.Models.PublicIPAddress
                        {
                            Id = getPublicIpAddressResponse.Id
                        },
                        //Subnet = new stacknetwork.Models.Subnet
                        Subnet = new profile2017.Network.Models.Subnet
                        {
                            Id = getSubnetResponse.Id
                        }
                    }
                }
            };

            var nicResponse = networkclient.NetworkInterfaces.CreateOrUpdateWithHttpMessagesAsync(rgname, nicName, nic).Result;
            if (!nicResponse.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create network interface");
            }

            var storageAccountName = "testdotnetsa";
            //var storageAccountParameters = new stackstorage.Models.StorageAccountCreateParameters
            var storageAccountParameters = new profile2017.Storage.Models.StorageAccountCreateParameters
            {
                Location = location,
                //Kind = stackstorage.Models.Kind.Storage,
                Kind = profile2017.Storage.Models.Kind.Storage,
                //Sku = new stackstorage.Models.Sku(stackstorage.Models.SkuName.StandardLRS)
                Sku = new profile2017.Storage.Models.Sku(profile2017.Storage.Models.SkuName.StandardLRS)
            };

            var storageAccountResponse = storageClient.StorageAccounts.CreateWithHttpMessagesAsync(rgname, storageAccountName, storageAccountParameters).Result;
            if (!storageAccountResponse.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create storage account");
            }
            var vmName = "test-dotnet-vm";
            //var vhdURItemplate = "https://" + storageAccountName + ".blob.redmond.ext-n22r1002.masd.stbtest.microsoft.com/vhds/" + vmName + ".vhd";
            var vhdURItemplate = "https://" + storageAccountName + ".blob.core.windows.net/vhds/" + vmName + ".vhd";

            //var vm = new stackcompute.Models.VirtualMachine
            var vm = new profile2017.Compute.Models.VirtualMachine
            {
                Location = location,
                //NetworkProfile = new stackcompute.Models.NetworkProfile
                NetworkProfile = new profile2017.Compute.Models.NetworkProfile
                {
                    //NetworkInterfaces = new List<stackcompute.Models.NetworkInterfaceReference>
                    NetworkInterfaces = new List<profile2017.Compute.Models.NetworkInterfaceReference>
                    {
                        //new stackcompute.Models.NetworkInterfaceReference
                        new profile2017.Compute.Models.NetworkInterfaceReference
                        {
                            Id = nicResponse.Body.Id,
                            Primary = true
                        }
                    }
                },
                //StorageProfile = new stackcompute.Models.StorageProfile
                StorageProfile = new profile2017.Compute.Models.StorageProfile
                {
                    //ImageReference = new stackcompute.Models.ImageReference
                    ImageReference = new profile2017.Compute.Models.ImageReference
                    {
                        Publisher = "Canonical",
                        Offer = "UbuntuServer",
                        Sku = "16.04-LTS",
                        Version = "latest"
                    },
                    //OsDisk = new stackcompute.Models.OSDisk
                    OsDisk = new profile2017.Compute.Models.OSDisk
                    {
                        Name = "osDisk",
                        //Vhd = new stackcompute.Models.VirtualHardDisk
                        Vhd = new profile2017.Compute.Models.VirtualHardDisk
                        {
                            Uri = vhdURItemplate
                        },
                        //CreateOption = stackcompute.Models.DiskCreateOptionTypes.FromImage
                        CreateOption = profile2017.Compute.Models.DiskCreateOptionTypes.FromImage
                    }
                },
                //OsProfile = new stackcompute.Models.OSProfile
                OsProfile = new profile2017.Compute.Models.OSProfile
                {
                    ComputerName = vmName,
                    AdminUsername = "useradmin",
                    AdminPassword = "!!123abc"
                },
                //HardwareProfile = new stackcompute.Models.HardwareProfile
                HardwareProfile = new profile2017.Compute.Models.HardwareProfile
                {
                    VmSize = "Standard_A1"
                }
            };

            var DataDisks = new List<profile2018.Compute.Models.DataDisk>
                    {
                        new profile2018.Compute.Models.DataDisk
                        {
                            CreateOption = profile2018.Compute.Models.DiskCreateOptionTypes.Attach,
                            Lun = 0,
                            ManagedDisk = new profile2018.Compute.Models.ManagedDiskParameters
                            {
                                Id = "tt"
                            }
                        }
                    };
            var vmResponse = computeClient.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync(rgname, vmName, vm).Result;
            if (!vmResponse.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create virtual machine");
            }
            //var stacknetworkclient = new stacknetwork.NetworkManagementClient(new CustomLoginCredentials());
            //stacknetworkclient.SubscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";

            //var azureclient = new Microsoft.Azure.Management.Compute.ComputeManagementClient(new CustomLoginCredentials());
            //azureclient.SubscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";

            //var stackclient = new stackcompute.ComputeManagementClient(new CustomLoginCredentials());
            //stackclient.SubscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";

            //Console.WriteLine("New VM on Profile");


            //var vmProperties = new stackcompute.Models.VirtualMachine();
            //vmProperties.Location = "eastus";
            //vmProperties.OsProfile = new stackcompute.Models.OSProfile
            //{
            //    AdminPassword = "!!123abc",
            //    AdminUsername = "testuser"
            //};
            //var ipconfig = new stacknetwork.Models.NetworkInterfaceIPConfiguration
            //{
            //    Name = "ipconfig1",
            //};

            //var nic = stacknetworkclient.NetworkInterfaces.CreateOrUpdateWithHttpMessagesAsync("testrg1", "nic111", parameters: new stacknetwork.Models.NetworkInterface
            //{
            //    Location = "eastus",
            //    IpConfigurations = new List<stacknetwork.Models.NetworkInterfaceIPConfiguration> { ipconfig }
            //}).Result; 

            //var a = new stackcompute.Models.NetworkInterfaceReference
            //{
            //    Id = "nic111",
            //    Primary = true
            //};
            //var b = new List<stackcompute.Models.NetworkInterfaceReference> { a };
            //vmProperties.NetworkProfile = new stackcompute.Models.NetworkProfile
            //{
            //    NetworkInterfaces = b
            //};
            ////{
            ////    NetworkInterfaces = new 
            ////}
            //var ab = stackclient.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync("testrg1", "vm1", vmProperties).Result;
            //Console.WriteLine("New VM on Profile");
        }

        //public static stacknetwork.NetworkManagementClient GetNetworkClient(Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        public static profile2017.Network.NetworkManagementClient GetNetworkClient
            (Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            //var client = new stacknetwork.NetworkManagementClient(baseUri: baseUri, credentials: credentials);
            var client = new profile2017.Network.NetworkManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.Network.NetworkManagementClient GetNetworkClient
            (Uri baseUri, AzureCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.Network.NetworkManagementClient(credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.ResourceManager.ResourceManagementClient GetResourceGroupClient
            (Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.ResourceManager.ResourceManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.ResourceManager.ResourceManagementClient GetResourceGroupClient
            (Uri baseUri, AzureCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.ResourceManager.ResourceManagementClient(credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.Compute.ComputeManagementClient GetComputeClient
            (Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.Compute.ComputeManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.Compute.ComputeManagementClient GetComputeClient
            (Uri baseUri, AzureCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.Compute.ComputeManagementClient(credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.Storage.StorageManagementClient GetStorageClient
            (Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.Storage.StorageManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2017.Storage.StorageManagementClient GetStorageClient
            (Uri baseUri, AzureCredentials credentials, string subscriotionId)
        {
            var client = new profile2017.Storage.StorageManagementClient(credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static profile2018.Subscription.SubscriptionDefinitionsClient GetSubscriptionClient
            (Uri baseUri, CustomLoginCredentials credentials)
        {
            var client = new profile2018.Subscription.SubscriptionDefinitionsClient(baseUri: baseUri, credentials: credentials);
            return client;
        }
    }
}
