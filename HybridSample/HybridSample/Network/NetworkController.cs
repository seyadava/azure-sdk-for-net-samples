namespace Network
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Network = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Network;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.Network.Fluent;

    using Microsoft.Rest.Azure;


    public class NetworkController
    {
        private const string ComponentName = "DotnetSDK_NetworkController";
        private readonly CustomLoginCredentials customCredential;
        private readonly AzureCredentials azureCredential;
        private readonly string subscriotionId;
        private readonly string environment;
        private readonly Uri baseUri;
        private static Profile2018Network.NetworkManagementClient client;
        private static IAzure azure;

        public NetworkController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier,
            string environment = "azurestack")
        {
            this.baseUri = baseUri;
            this.customCredential = credentials;
            this.subscriotionId = subscriptionIdentifier;
            this.environment = environment;
            GetNetworkClient();
        }

        public NetworkController(
            Uri baseUri,
            AzureCredentials credentials,
            string environment = "azurestack")
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                this.baseUri = baseUri;
                this.azureCredential = credentials;

                GetNetworkClient();
            }
            else
            {
                azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();
            }
            this.environment = environment;

        }

        private void GetNetworkClient()
        {
            if (client != null)
            {
                return;
            }
            if (customCredential != null)
            {
                client = new Profile2018Network.NetworkManagementClient(baseUri: baseUri, credentials: customCredential)
                {
                    SubscriptionId = this.subscriotionId
                };
            }
            else
            {
                client = new Profile2018Network.NetworkManagementClient(baseUri: baseUri, credentials: azureCredential)
                {
                    SubscriptionId = this.azureCredential.DefaultSubscriptionId
                };
            }
            client.SetUserAgent(ComponentName);
        }

        private async Task<AzureOperationResponse<string>> CreatePublicIpAddressAzureStack(
            string publicIpName,
            string resourceGroupName,
            string location,
            string allocationMethod = "Dynamic",
            string publicIpAddress = null,
            IList<(string, string)> tags = null)
        {
            if (client == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }

            var publicIp = new Profile2018Network.Models.PublicIPAddress
            {
                Location = location,
            };

            if (String.Equals("dynamic", allocationMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                allocationMethod = Profile2018Network.Models.IPAllocationMethod.Dynamic;
            }
            else
            {
                allocationMethod = Profile2018Network.Models.IPAllocationMethod.Static;
                if (!String.IsNullOrEmpty(publicIpAddress))
                {
                    publicIp.IpAddress = publicIpAddress;
                }
            }
            publicIp.PublicIPAllocationMethod = allocationMethod;

            foreach (var tag in tags ?? new List<(string, string)>())
            {
                publicIp.Tags.Add(tag.Item1, tag.Item2);
            }

            try
            {
                var publicIpTask = await client.PublicIPAddresses.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName: resourceGroupName,
                    publicIpAddressName: publicIpName,
                    parameters: publicIp);
                return new AzureOperationResponse<string>
                {
                    Body = publicIpTask.Body.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        private async Task<AzureOperationResponse<string>> CreatePublicIpAddressAzure(
            string publicIpName,
            string resourceGroupName,
            string location,
            string allocationMethod = "Dynamic",
            string publicIpAddress = null,
            IList<(string, string)> tags = null)
        {
            if (azure == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Azure is not instantiated"
                    }
                };
            }
            try
            {
                var ip = azure.PublicIPAddresses.
                    Define(publicIpName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName);

                if (String.Equals("dynamic", allocationMethod, StringComparison.InvariantCultureIgnoreCase))
                {
                    ip = ip.WithDynamicIP();
                }
                else
                {
                    ip = ip.WithStaticIP();
                }

                foreach (var tag in tags ?? new List<(string, string)>())
                {
                    ip = ip.WithTag(tag.Item1, tag.Item2);
                }

                var ipTask = await ip.CreateAsync();
                return new AzureOperationResponse<string>
                {
                    Body = ipTask.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }


        public async Task<AzureOperationResponse<string>> CreatePublicIpAddress(
            string publicIpName,
            string resourceGroupName,
            string location,
            string allocationMethod = "Dynamic",
            string publicIpAddress = null,
            IList<(string, string)> tags = null)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreatePublicIpAddressAzureStack(publicIpName, resourceGroupName, location, allocationMethod, publicIpAddress, tags);
            }
            else
            {
                return await CreatePublicIpAddressAzure(publicIpName, resourceGroupName, location, allocationMethod, publicIpAddress, tags);
            }
        }

        public async Task<AzureOperationResponse<string>> CreateVirtualNetwork(
            string virtualNetworkName,
            IList<string> vnetAddressSpaces,
            string resourceGroupName,
            string location,
            Dictionary<string, string> subnets = null)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreateVirtualNetworkAzureStack(virtualNetworkName, vnetAddressSpaces, resourceGroupName, location, subnets);
            }
            else
            {
                return await CreateVirtualNetworkAzure(virtualNetworkName, vnetAddressSpaces, resourceGroupName, location, subnets);
            }
        }

        public async Task<AzureOperationResponse<string>> CreateVirtualNetworkAzure(
            string virtualNetworkName,
            IList<string> vnetAddressSpaces,
            string resourceGroupName,
            string location,
            Dictionary<string, string> subnets = null)
        {
            if (azure == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Azure is not instantiated"
                    }
                };
            }
            try
            {

                var vnet = azure.Networks.
                    Define(virtualNetworkName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithAddressSpace(vnetAddressSpaces[0]);

                foreach (var subnet in subnets ?? new Dictionary<string, string>())
                {
                    if (string.IsNullOrEmpty(subnet.Value))
                    {
                        return new AzureOperationResponse<string>
                        {
                            Response = new HttpResponseMessage
                            {
                                StatusCode = System.Net.HttpStatusCode.BadRequest,
                                ReasonPhrase = string.Format("Subnet address space is not valid. Subnet: {0}", subnet.Key)
                            }
                        };
                    }

                    vnet = vnet.WithSubnet(subnet.Key, subnet.Value);
                }

                var vnetTask = await vnet.CreateAsync();
                return new AzureOperationResponse<string>
                {
                    Body = vnetTask.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        private async Task<AzureOperationResponse<string>> CreateVirtualNetworkAzureStack(
            string virtualNetworkName,
            IList<string> vnetAddressSpaces,
            string resourceGroupName,
            string location,
            Dictionary<string, string> subnets = null)
        {
            if (client == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }

            var subnetsParameters = new List<Profile2018Network.Models.Subnet>();
            foreach (var subnet in subnets ?? new Dictionary<string, string>())
            {
                if (string.IsNullOrEmpty(subnet.Value))
                {
                    return new AzureOperationResponse<string>
                    {
                        Response = new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            ReasonPhrase = string.Format("Subnet address space is not valid. Subnet: {0}", subnet.Key)
                        }
                    };
                }
                subnetsParameters.Add(new Profile2018Network.Models.Subnet
                {
                    Name = subnet.Key,
                    AddressPrefix = subnet.Value
                });
            }

            var vnet = new Profile2018Network.Models.VirtualNetwork
            {
                Location = location,
                AddressSpace = new Profile2018Network.Models.AddressSpace
                {
                    AddressPrefixes = vnetAddressSpaces
                },
                Subnets = subnetsParameters
            };

            try
            {
                var vnetTask = await client.VirtualNetworks.CreateOrUpdateWithHttpMessagesAsync(
                    resourceGroupName: resourceGroupName,
                    virtualNetworkName: virtualNetworkName,
                    parameters: vnet);
                return new AzureOperationResponse<string>
                {
                    Body = vnetTask.Body.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<AzureOperationResponse<string>> CreateNetworkInterface(
            string nicName,
            string resourceGroupName,
            string virtualNetworkName,
            string vnetAddressSpace,
            string subnetName,
            string subnetAddressSpace,
            string ipName,
            string location,
            string allocationMethod = "Dynamic",
            IList<Tuple<string, string>> tags = null)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreateNetworkInterfaceAzureStack
                    (nicName, resourceGroupName, virtualNetworkName, vnetAddressSpace, subnetName, subnetAddressSpace, ipName, location, allocationMethod, tags);
            }
            else
            {
                return await CreateNetworkInterfaceAzure
                    (nicName, resourceGroupName, virtualNetworkName, vnetAddressSpace, subnetName, subnetAddressSpace, ipName, location, allocationMethod, tags);
            }
        }

        private async Task<AzureOperationResponse<string>> CreateNetworkInterfaceAzureStack(
            string nicName,
            string resourceGroupName,
            string virtualNetworkName,
            string vnetAddressSpace,
            string subnetName,
            string subnetAddressSpace,
            string ipName,
            string location,
            string allocationMethod = "Dynamic",
            IList<Tuple<string, string>> tags = null)
        {
            if (client == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Client is not instantiated"
                    }
                };
            }

            try
            {
                var vnetTask = await this.CreateVirtualNetworkAzureStack
                    (virtualNetworkName, new List<string> { vnetAddressSpace }, resourceGroupName, location, new Dictionary<string, string> { {subnetName, subnetAddressSpace}});
                if (!vnetTask.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<string>
                    {
                        Response = vnetTask.Response
                    };
                }

                var subnet = await client.Subnets.GetWithHttpMessagesAsync(resourceGroupName, virtualNetworkName, subnetName);
                if (!subnet.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<string>
                    {
                        Response = subnet.Response
                    };
                }

                var ipTask = await this.CreatePublicIpAddressAzureStack(ipName, resourceGroupName, location);
                if (!ipTask.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<string>
                    {
                        Response = ipTask.Response
                    };
                }

                var ip = await client.PublicIPAddresses.GetWithHttpMessagesAsync(resourceGroupName, ipName);
                if (!ip.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<string>
                    {
                        Response = ip.Response
                    };
                }
                
                var nic = new Profile2018Network.Models.NetworkInterface
                {
                    Location = location,
                    IpConfigurations = new List<Profile2018Network.Models.NetworkInterfaceIPConfiguration>
                    {
                        new Profile2018Network.Models.NetworkInterfaceIPConfiguration
                        {
                            Name = string.Format("{0}-ipconfig", nicName),
                            PrivateIPAllocationMethod = allocationMethod,
                            PublicIPAddress = ip.Body,
                            Subnet = subnet.Body
                        }
                    }
                };

                foreach (var tag in tags ?? new List<Tuple<string, string>>())
                {
                    var t = tag.Item1;

                    nic.Tags.Add(tag.Item1, tag.Item2);
                }

                var nicTask = await client.NetworkInterfaces.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, nicName, nic);
                return new AzureOperationResponse<string>
                {
                    Body = nicTask.Body.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        private async Task<AzureOperationResponse<string>> CreateNetworkInterfaceAzure(
            string nicName,
            string resourceGroupName,
            string virtualNetworkName,
            string vnetAddressSpace,
            string subnetName,
            string subnetAddressSpace,
            string ipName,
            string location,
            string allocationMethod = "Dynamic",
            IList<Tuple<string, string>> tags = null)
        {
            if (azure == null)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.ExpectationFailed,
                        ReasonPhrase = "Azure is not instantiated"
                    }
                };
            }

            try
            {
                var network = azure.Networks
                    .Define(virtualNetworkName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithAddressSpace(vnetAddressSpace)
                    .DefineSubnet(subnetName)
                        .WithAddressPrefix(subnetAddressSpace)
                        .Attach()
                    .Create();

                var nic = await azure.NetworkInterfaces.Define(nicName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithExistingPrimaryNetwork(network)
                    .WithSubnet(subnetName)
                    .WithPrimaryPrivateIPAddressDynamic()
                    .CreateAsync();
                return new AzureOperationResponse<string>
                {
                    Body = nic.Id
                };
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<string>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }
    }

}
