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
        private readonly Uri baseUri;
        private static Profile2018Network.NetworkManagementClient client;
        private static IAzure azure;

        public NetworkController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subsId)
        {
            
                this.baseUri = baseUri;
                this.customCredential = credentials;

                GetNetworkClient(subsId);
        }

        private void GetNetworkClient(string subscriptionId)
        {
            if (client != null)
            {
                return;
            }
            
            client = new Profile2018Network.NetworkManagementClient(baseUri: baseUri, credentials: customCredential)
            {
                SubscriptionId = subscriptionId
            };
            
            client.SetUserAgent(ComponentName);
        }

        private async Task<AzureOperationResponse<string>> CreatePublicIpAddressAzureStack(
            string publicIpName,
            string resourceGroupName,
            string location,
            string allocationMethod = "Dynamic",
            string publicIpAddress = null,
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

            var publicIp = new Profile2018Network.Models.PublicIPAddress
            {
                Location = location,
            };

            if (String.Equals("dynamic", allocationMethod, StringComparison.OrdinalIgnoreCase))
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

            foreach (var tag in tags ?? new List<Tuple<string, string>>())
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
                var ip = azure.PublicIPAddresses.
                    Define(publicIpName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName);

                if (String.Equals("dynamic", allocationMethod, StringComparison.OrdinalIgnoreCase))
                {
                    ip = ip.WithDynamicIP(); 
                }
                else
                {
                    ip = ip.WithStaticIP();
                }

                foreach (var tag in tags ?? new List<Tuple<string, string>>())
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
            IList<Tuple<string, string>> tags = null)
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

        public async Task<AzureOperationResponse<Profile2018Network.Models.Subnet>> AddSubnet(
            string subnetName,
            string virtualNetworkName,
            string subnetAddressSpace,
            string resourceGroupName)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Network.Models.Subnet>
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
                var vnet = await GetVirtualNetwork(virtualNetworkName, resourceGroupName);
                if (!vnet.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<Profile2018Network.Models.Subnet>
                    {
                        Response = vnet.Response
                    };
                }
                var subnetParams = new Profile2018Network.Models.Subnet
                {
                    AddressPrefix = subnetAddressSpace
                };
                var subnetTask = await client.Subnets.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, virtualNetworkName, subnetName, subnetParams);
                return subnetTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Network.Models.Subnet>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
            
        }

        public async Task<AzureOperationResponse<Profile2018Network.Models.Subnet>> GetSubnet(
            string subnetName,
            string virtualNetworkName,
            string resourceGroupName)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Network.Models.Subnet>
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
                var subnetTask = await client.Subnets.GetWithHttpMessagesAsync(resourceGroupName, virtualNetworkName, subnetName);
                return subnetTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Network.Models.Subnet>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }

        }

        public async Task<AzureOperationResponse<Profile2018Network.Models.VirtualNetwork>> GetVirtualNetwork(
            string virtualNetworkName, 
            string resourceGroupName)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Network.Models.VirtualNetwork>
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
                var vnet = await client.VirtualNetworks.GetWithHttpMessagesAsync(resourceGroupName, virtualNetworkName);
                return vnet;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Network.Models.VirtualNetwork>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<AzureOperationResponse<Profile2018Network.Models.PublicIPAddress>> GetPublicIpAddress(
            string publicIpName, 
            string resourceGroupName)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Network.Models.PublicIPAddress>
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
                var publicIpTask = await client.PublicIPAddresses.GetWithHttpMessagesAsync(resourceGroupName, publicIpName);
                return publicIpTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Network.Models.PublicIPAddress>
                {
                    Response = new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ReasonPhrase = ex.Message
                    }
                };
            }
        }

        public async Task<AzureOperationResponse<Profile2018Network.Models.NetworkInterface>> CreateNetworkInterface(
            string nicName,
            string resourceGroupName,
            string virtualNetworkName,
            string subnetName,
            string ipName,
            string location,
            string allocationMethod = "Dynamic",
            IList<Tuple<string, string>> tags = null)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Network.Models.NetworkInterface>
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
                var subnetTask = await GetSubnet(subnetName, virtualNetworkName, resourceGroupName);
                if (!subnetTask.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<Profile2018Network.Models.NetworkInterface>
                    {
                        Response = subnetTask.Response
                    };
                }

                var ipTask = await GetPublicIpAddress(ipName, resourceGroupName);
                if (!ipTask.Response.IsSuccessStatusCode)
                {
                    return new AzureOperationResponse<Profile2018Network.Models.NetworkInterface>
                    {
                        Response = ipTask.Response
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
                            PublicIPAddress = ipTask.Body,
                            Subnet = subnetTask.Body
                        }
                    }
                };

                foreach (var tag in tags ?? new List<Tuple<string, string>>())
                {
                    var t = tag.Item1;

                    nic.Tags.Add(tag.Item1, tag.Item2);
                }

                var nicTask = await client.NetworkInterfaces.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, nicName, nic);
                return nicTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Network.Models.NetworkInterface>
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
