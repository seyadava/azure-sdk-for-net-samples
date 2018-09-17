namespace ComputeSample
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Compute = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Compute;
    using Profile2018Network = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Network;
    using Profile2018ResourceManager = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.ResourceManager;

    using Microsoft.Rest.Azure;

    class Program
    {
        private const string ComponentName = "DotnetSDK";
        private const string vhdURItemplate = "https://{0}.blob.{1}/vhds/{2}.vhd";
        private readonly CustomLoginCredentials customCredential;
        private readonly string subscriotionId;
        private readonly Uri baseUri;

        static void Main(string[] args)
        {
            //Set variables
            var location = "location";
            var baseUriString = "baseUriString";
            var resourceGroupName = "resourceGroupName";
            var servicePrincipalId = "servicePrincipalID";
            var servicePrincipalSecret = "servicePrincipalSecret";
            var azureResourceId = "resourceID";
            var tenantId = "tenantID";
            var subscriptionId = "subscriptionID";
            var vmName = "virtualMachineName";
            var vnetName = "virtualNetworkName";
            var subnetName = "subnetName";
            var subnetAddress = "10.0.0.0/24";
            var vnetAddresses = "10.0.0.0/16";
            var ipName = "ipName";
            var nicName = "networkInterfaceName";
            var diskName = "diskName";

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

            try
            {
                Console.WriteLine("Instantiate compute client");
                var computeClient = GetComputeClient(new Uri(baseUriString), credentials, subscriptionId);
                var diskProperties = new Profile2018Compute.Models.Disk
                {
                    CreationData = new Profile2018Compute.Models.CreationData
                    {
                        CreateOption = Profile2018Compute.Models.DiskCreateOption.Empty,
                    },
                    Location = location,
                    Sku = new Profile2018Compute.Models.DiskSku
                    {
                        Name = Profile2018Compute.Models.StorageAccountTypes.StandardLRS
                    },
                    DiskSizeGB = 1,
                };
                var diskTask = computeClient.Disks.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName,
                    diskName,
                    diskProperties);
                diskTask.Wait();

                Console.WriteLine("Create virtual machine");
                var vmProperties = new Profile2018Compute.Models.VirtualMachine
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
                        DataDisks = new List<Profile2018Compute.Models.DataDisk>
                        {
                            new Profile2018Compute.Models.DataDisk
                            {
                                CreateOption = Profile2018Compute.Models.DiskCreateOptionTypes.Attach,
                                ManagedDisk = new Profile2018Compute.Models.ManagedDiskParameters
                                {
                                    StorageAccountType = Profile2018Compute.Models.StorageAccountTypes.StandardLRS,
                                    Id = diskTask.Result.Body.Id
                                }
                            }
                        },
                        OsDisk = new Profile2018Compute.Models.OSDisk
                        {
                            Name = "osDisk",
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
                    vmProperties);
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
    }
}
