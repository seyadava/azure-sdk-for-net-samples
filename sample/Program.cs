using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure;
using stackauth = Microsoft.Azure.Management.Profiles.profile_2017_03_09.Authorization;
using stackcompute = Microsoft.Azure.Management.Profiles.profile_2017_03_09.Compute;
using stackresource = Microsoft.Azure.Management.Profiles.profile_2017_03_09.ResourceManager;
using stacknetwork = Microsoft.Azure.Management.Profiles.profile_2017_03_09.Network;
using stackstorage = Microsoft.Azure.Management.Profiles.profile_2017_03_09.Storage;
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
            const string subscriptionId = "23be27d7-0237-47fd-a012-9d691c6a3d83";
            const string location = "redmond";
            var baseUri = new Uri("https://management.redmond.ext-n22r1002.masd.stbtest.microsoft.com/");
            var credentials = new CustomLoginCredentials();

            Console.WriteLine("New Azure VM!");

            //var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            //var azure = Azure.Configure().Authenticate(credentials).WithDefaultSubscription();
            //var vm = azure.VirtualMachines.
            //    Define("newDotnetVm").
            //    WithRegion("eastus").
            //    WithNewResourceGroup("newtestdotnet").
            //    WithNewPrimaryNetwork("10.0.0.0/20").
            //    WithPrimaryPrivateIPAddressDynamic().
            //    WithoutPrimaryPublicIPAddress().
            //    WithPopularLinuxImage(KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts).
            //    WithRootUsername("testuser").
            //    WithRootPassword("!!123abc").
            //    WithSize(VirtualMachineSizeTypes.BasicA0).
            //    Create();

            var networkclient = GetNetworkClient(baseUri, credentials, subscriptionId);
            var resourceGroupClient = GetResourceGroupClient(baseUri, credentials, subscriptionId);
            var computeClient = GetComputeClient(baseUri, credentials, subscriptionId);
            var storageClient = GetStorageClient(baseUri, credentials, subscriptionId);

            var rgname = "test-dotnet-rg2";
            var rg = resourceGroupClient.ResourceGroups.CreateOrUpdateWithHttpMessagesAsync(rgname, parameters: new stackresource.Models.ResourceGroup {
                Location = location
            }).Result;

            if (!rg.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create resource group");
            }

            var publicIpName = "test-dotnet-publicIp";
            var publicIp = new stacknetwork.Models.PublicIPAddress
            {
                Location = location,
                Tags = new Dictionary<string, string>
                {
                    {"key", "value" }
                },
                PublicIPAllocationMethod = stacknetwork.Models.IPAllocationMethod.Dynamic
            };

            var pip = networkclient.PublicIPAddresses.CreateOrUpdateWithHttpMessagesAsync(rgname, publicIpName, publicIp).Result;
            if (!pip.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create public IP");
            }

            var vnetName = "test-dotnet-vnet";
            var subnetName = "subnet1";
            var vnet = new stacknetwork.Models.VirtualNetwork
            {
                Location = location,
                AddressSpace = new stacknetwork.Models.AddressSpace
                {
                    AddressPrefixes = new List<string> { "10.0.0.0/16" }
                },
                Subnets = new List<stacknetwork.Models.Subnet>
                {
                    new stacknetwork.Models.Subnet
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
            var nic = new stacknetwork.Models.NetworkInterface
            {
                Location = location,
                Tags = new Dictionary<string, string> { { "key", "value" } },
                IpConfigurations = new List<stacknetwork.Models.NetworkInterfaceIPConfiguration>
                {
                    new stacknetwork.Models.NetworkInterfaceIPConfiguration
                    {
                        Name = ipconfigName,
                        PrivateIPAllocationMethod = stacknetwork.Models.IPAllocationMethod.Dynamic,
                        PublicIPAddress = new stacknetwork.Models.PublicIPAddress
                        {
                            Id = getPublicIpAddressResponse.Id
                        },
                        Subnet = new stacknetwork.Models.Subnet
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

            var storageAccountName = "test-dotnet-sa";
            var storageAccountParameters = new stackstorage.Models.StorageAccountCreateParameters
            {
                Location = location,
                Kind = stackstorage.Models.Kind.Storage,
                Sku = new stackstorage.Models.Sku(stackstorage.Models.SkuName.StandardLRS)
            };

            var storageAccountResponse = storageClient.StorageAccounts.CreateWithHttpMessagesAsync(rgname, storageAccountName, storageAccountParameters).Result;
            if (!storageAccountResponse.Response.IsSuccessStatusCode)
            {
                Console.WriteLine("Could not create storage account");
            }
            var vmName = "test-dotnet-vm";
            var vhdURItemplate = "https://" + storageAccountName + ".blob.redmond.ext-n22r1002.masd.stbtest.microsoft.com/vhds/" + vmName + ".vhd";
            var vm = new stackcompute.Models.VirtualMachine
            {
                Location = location,
                NetworkProfile = new stackcompute.Models.NetworkProfile
                {
                    NetworkInterfaces = new List<stackcompute.Models.NetworkInterfaceReference>
                    {
                        new stackcompute.Models.NetworkInterfaceReference
                        {
                            Id = nicResponse.Body.Id,
                            Primary = true
                        }
                    }
                },
                StorageProfile = new stackcompute.Models.StorageProfile
                {
                    ImageReference = new stackcompute.Models.ImageReference
                    {
                        Publisher = "Canonical",
                        Offer = "UbuntuServer",
                        Sku = "16.04-LTS",
                        Version = "latest"
                    },
                    OsDisk = new stackcompute.Models.OSDisk
                    {
                        Name = "osDisk",
                        Vhd = new stackcompute.Models.VirtualHardDisk
                        {
                            Uri = vhdURItemplate
                        },
                        CreateOption = stackcompute.Models.DiskCreateOptionTypes.FromImage
                    }
                },
                OsProfile = new stackcompute.Models.OSProfile
                {
                    ComputerName = vmName,
                    AdminUsername = "useradmin",
                    AdminPassword = "!!123abc"
                },
                HardwareProfile = new stackcompute.Models.HardwareProfile
                {
                    VmSize = "Standard_A1"
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

        public static stacknetwork.NetworkManagementClient GetNetworkClient(Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new stacknetwork.NetworkManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static stackresource.ResourceManagementClient GetResourceGroupClient(Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        { 
            var client = new stackresource.ResourceManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

        public static stackcompute.ComputeManagementClient GetComputeClient(Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new stackcompute.ComputeManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }
        
        public static stackstorage.StorageManagementClient GetStorageClient(Uri baseUri, CustomLoginCredentials credentials, string subscriotionId)
        {
            var client = new stackstorage.StorageManagementClient(baseUri: baseUri, credentials: credentials);
            client.SubscriptionId = subscriotionId;
            return client;
        }

    }


    public class CustomLoginCredentials : ServiceClientCredentials
    {
        private string AuthenticationToken { get; set; }
        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            var authenticationContext =
                new AuthenticationContext("https://login.windows.net/73103a66-894e-4622-8ca7-da73c5c00c0b");
            var credential = new ClientCredential(clientId: "106a0cf8-a34c-41af-bfef-fe8a1c8652fd", clientSecret: "l8plqAxDsEKmr5M1PuW361NYy4MnGNHqdFbygddkhtQ=");

            var result = authenticationContext.AcquireToken(resource: "https://management.azurestackci03.onmicrosoft.com/1b6c5003-5826-4804-87c1-1a04f2d65076",
                clientCredential: credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            AuthenticationToken = result.AccessToken;
        }
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (AuthenticationToken == null)
            {
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            }



            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //request.Version = new Version(apiVersion);
            await base.ProcessHttpRequestAsync(request, cancellationToken);

        }
    }
}
