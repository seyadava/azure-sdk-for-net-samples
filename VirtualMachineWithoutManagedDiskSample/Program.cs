﻿namespace VirtualMachineWithoutManagedDiskSample
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Compute = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Compute;
    using Profile2018Network = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Network;
    using Profile2018Storage = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Storage;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;

    //using Microsoft.Rest.Azure;

    class Program
    {
        private const string ComponentName = "DotnetSDK";
        private const string vhdURItemplate = "https://{0}.blob.{1}/vhds/{2}.vhd";

        static void Main(string[] args)
        {
            //Set variables
            var location = "redmond";
            var baseUriString = "https://management.redmond.ext-n22r1002.masd.stbtest.microsoft.com/";
            var resourceGroupName = "testrg";
            var servicePrincipalId = "6656e01b-b5ce-43be-99b6-2f109e255343";
            var servicePrincipalSecret = "e69Tve6T4XvtQUrvFXVR1B5X4ZYBS+QNY3zKQ5s2JHA=";
            var azureResourceId = "https://management.azurestackci07.onmicrosoft.com/29ae84a9-c761-4678-8cf6-0c28b6952a41";
            var tenantId = "2b3697e6-a7a2-4cdd-a3d4-f4ef6505cd4f";
            var subscriptionId = "7b5e2e72-d4ca-49de-b45f-b814d2b3aa07";
            var vmName = "virtualMachineName";
            var vnetName = "virtualNetworkName";
            var subnetName = "subnetName";
            var subnetAddress = "10.0.0.0/24";
            var vnetAddresses = "10.0.0.0/16";
            var ipName = "ipName";
            var nicName = "networkInterfaceName";
            var storagePrefix = "redmond.ext-n22r1002.masd.stbtest.microsoft.com";
            var storageAccountName = "storageaccountt";

            Console.WriteLine("Get credential token");
            var credentials = new CustomLoginCredentials(servicePrincipalId, servicePrincipalSecret, azureResourceId, tenantId);

            try
            {
                Console.WriteLine("Instantiate resource management client");
                var rmClient = GetResourceManagementClient(new Uri(baseUriString), credentials, subscriptionId);

                Console.WriteLine("Create resource group");
                var rmTask = rmClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    new Profile2018ResourceManager.Models.ResourceGroup
                    {
                        Location = location
                    });
                rmTask.Wait();
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("Could not create resource group. Exception: {0}", ex.Message));
            }

            Console.WriteLine("Instantiate network client");
            var networkClient = GetNetworkClient(new Uri(baseUriString), credentials, subscriptionId);
            var subnet = new Profile2018Network.Models.Subnet();
            
            try
            {
                Console.WriteLine("Create vitual network");
                var vnet = new Profile2018Network.Models.VirtualNetwork
                {
                    Location = location,
                    AddressSpace = new Profile2018Network.Models.AddressSpace
                    {
                        AddressPrefixes = new List<string> { vnetAddresses }
                    },
                    Subnets = new List<Profile2018Network.Models.Subnet>
                    {
                        new Profile2018Network.Models.Subnet
                        {
                            AddressPrefix = subnetAddress,
                            Name = subnetName
                        }
                    }
                };
                var vnetTask = networkClient.VirtualNetworks.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    vnetName,
                    vnet);
                vnetTask.Wait();
                subnet = vnetTask.Result.Body.Subnets[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create virtual network. Exception: {0}", ex.Message));
            }

            var ip = new Profile2018Network.Models.PublicIPAddress();
            try
            {
                Console.WriteLine("Create IP");
                var ipProperties = new Profile2018Network.Models.PublicIPAddress
                {
                    Location = location,
                    PublicIPAllocationMethod = Profile2018Network.Models.IPAllocationMethod.Dynamic,
                };
                var ipTask = networkClient.PublicIPAddresses.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    ipName,
                    ipProperties);
                ipTask.Wait();
                ip = ipTask.Result.Body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create IP. Exception: {0}", ex.Message));
            }

            var nic = new Profile2018Network.Models.NetworkInterface();
            try
            {
                Console.WriteLine("Create network interface");
                var nicProperties = new Profile2018Network.Models.NetworkInterface
                {
                    Location = location,
                    IpConfigurations = new List<Profile2018Network.Models.NetworkInterfaceIPConfiguration>
                    {
                        new Profile2018Network.Models.NetworkInterfaceIPConfiguration
                        {
                            Name = string.Format("{0}-ipconfig", nicName),
                            PrivateIPAllocationMethod = "Dynamic",
                            PublicIPAddress = ip,
                            Subnet = subnet
                        }
                    }

                };
                
                var nicTask = networkClient.NetworkInterfaces.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    nicName,
                    nicProperties);
                nicTask.Wait();
                nic = nicTask.Result.Body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create network interface. Exception: {0}", ex.Message));
            }

            var storage = new Profile2018Storage.Models.StorageAccount();
            var storageClient = GetStorageClient(new Uri(baseUriString), credentials, subscriptionId);
            try
            {
                Console.WriteLine("Create storage account");
                var storageProperties = new Profile2018Storage.Models.StorageAccountCreateParameters
                {
                    Location = location,
                    Kind = Profile2018Storage.Models.Kind.Storage,
                    Sku = new Profile2018Storage.Models.Sku(Profile2018Storage.Models.SkuName.StandardLRS)
                };

                var storageTask = storageClient.StorageAccounts.CreateWithHttpMessagesAsync(resourceGroupName, storageAccountName, storageProperties);
                storageTask.Wait();
                storage = storageTask.Result.Body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create network interface. Exception: {0}", ex.Message));
            }

            try
            {
                Console.WriteLine("Instantiate compute client");
                var computeClient = GetComputeClient(new Uri(baseUriString), credentials, subscriptionId);
                
                Console.WriteLine("Create virtual machine");
                var vmParameters = new Profile2018Compute.Models.VirtualMachine
                {
                    Location = location,
                    NetworkProfile = new Profile2018Compute.Models.NetworkProfile
                    {
                        NetworkInterfaces = new List<Profile2018Compute.Models.NetworkInterfaceReference>
                        {
                            new Profile2018Compute.Models.NetworkInterfaceReference
                            {
                                Id = nic.Id,
                                Primary = true
                            }
                        }
                    },
                    StorageProfile = new Profile2018Compute.Models.StorageProfile
                    {
                        OsDisk = new Profile2018Compute.Models.OSDisk
                        {
                            Name = "osDisk",
                            Vhd = new Profile2018Compute.Models.VirtualHardDisk
                            {
                                Uri = string.Format(vhdURItemplate, storageAccountName, storagePrefix, vmName)
                            },
                            CreateOption = Profile2018Compute.Models.DiskCreateOptionTypes.FromImage
                        },
                        ImageReference = new Profile2018Compute.Models.ImageReference
                        {
                            Publisher = "Canonical",
                            Offer = "UbuntuServer",
                            Sku = "16.04-LTS",
                            Version = "latest"
                        }
                    },
                    OsProfile = new Profile2018Compute.Models.OSProfile
                    {
                        ComputerName = vmName,
                        AdminUsername = "useradmin",
                        AdminPassword = "userpassword1!"
                    },
                    HardwareProfile = new Profile2018Compute.Models.HardwareProfile
                    {
                        VmSize = "Standard_A1"
                    }
                };

                var vmTask = computeClient.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    vmName,
                    vmParameters);
                vmTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not create virtual machine. Exception: {0}", ex.Message));
            }
        }

        private static Profile2018Compute.ComputeManagementClient GetComputeClient(Uri baseUri, CustomLoginCredentials customCredential, string subscriptionId)
        {
            var client = new Profile2018Compute.ComputeManagementClient(baseUri: baseUri, credentials: customCredential)
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

        private static Profile2018Network.NetworkManagementClient GetNetworkClient(Uri baseUri, CustomLoginCredentials customCredential, string subscriptionId)
        {
            var client = new Profile2018Network.NetworkManagementClient(baseUri: baseUri, credentials: customCredential)
            {
                SubscriptionId = subscriptionId
            };
            client.SetUserAgent(ComponentName);

            return client;
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
    }
}
