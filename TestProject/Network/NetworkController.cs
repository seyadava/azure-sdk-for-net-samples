namespace Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    using System.Net.Http;
    using Authorization;
    using Profile2018Network = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Network;
    using Resource;

    public class NetworkController
    {
        private const string ComponentName = "DotnetSDK_NetworkController";
        private readonly CustomLoginCredentials credential;
        private readonly string subscriotionId;
        private Uri baseUri;
        private static Profile2018Network.NetworkManagementClient client;

        public NetworkController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier
            )
        {
            this.baseUri = baseUri;
            this.credential = credentials;
            this.subscriotionId = subscriptionIdentifier;

            GetNetworkClient();
        }

        private void GetNetworkClient()
        {
            if (client != null)
            {
                return;
            }
            client = new Profile2018Network.NetworkManagementClient(baseUri: this.baseUri, credentials: this.credential);
            client.SubscriptionId = this.subscriotionId;
            client.SetUserAgent(ComponentName);
        }

        public async Task<Profile2018Network.Models.PublicIPAddress> CreatePublicIpAddress(
            string publicIpName,
            string resourceGroupName,
            string location,
            string allocationMethod = "Dynamic",
            string publicIpAddress = null,
            IList<(string, string)> tags = null)
        {
            if (client == null)
            {
                throw new Exception("Client is not instantiated");
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
                if (String.IsNullOrEmpty(publicIpAddress))
                {
                    throw new Exception("Public IP Address cannot be empty for static allocation method.");
                }
                publicIp.IpAddress = publicIpAddress;
            }
            publicIp.PublicIPAllocationMethod = allocationMethod;

            foreach(var tag in tags ?? new List<(string, string)>())
            {
                var t = tag.Item1;
                
                publicIp.Tags.Add(tag.Item1, tag.Item2);
            }
            
            var publicIpTask = await client.PublicIPAddresses.CreateOrUpdateWithHttpMessagesAsync(
                resourceGroupName: resourceGroupName,
                publicIpAddressName: publicIpName,
                parameters: publicIp
                );

            if (!publicIpTask.Response.IsSuccessStatusCode)
            {
                return null;
            }
            return publicIpTask.Body;
        }
    }
}
