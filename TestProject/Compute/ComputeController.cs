namespace Compute
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Compute = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Compute;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Rest.Azure;

    public class ComputeController
    {
        private const string ComponentName = "DotnetSDK_ComputeController";
        private const string vhdURItemplate = "https://{0}.blob.{1}/vhds/{2}.vhd";
        private readonly CustomLoginCredentials customCredential;
        private readonly AzureCredentials azureCredential;
        private readonly string subscriotionId;
        private readonly Uri baseUri;
        private static Profile2018Compute.ComputeManagementClient client;

        private Profile2018Compute.Models.ImageReference linuxImageReference = new Profile2018Compute.Models.ImageReference
        {
            Publisher = "Canonical",
            Offer = "UbuntuServer",
            Sku = "16.04-LTS",
            Version = "latest"
        };

    public ComputeController(
            Uri baseUri,
            CustomLoginCredentials credentials,
            string subscriptionIdentifier
            )
        {
            this.baseUri = baseUri;
            this.customCredential = credentials;
            this.subscriotionId = subscriptionIdentifier;

            GetComputeClient();
        }

        public ComputeController(
            Uri baseUri,
            AzureCredentials credentials)
        {
            this.baseUri = baseUri;
            this.azureCredential = credentials;

            GetComputeClient();
        }

        private void GetComputeClient()
        {
            if (client != null)
            {
                return;
            }
            if (customCredential != null)
            {
                client = new Profile2018Compute.ComputeManagementClient(baseUri: baseUri, credentials: customCredential)
                {
                    SubscriptionId = this.subscriotionId
                };
            }
            else
            {
                client = new Profile2018Compute.ComputeManagementClient(baseUri: baseUri, credentials: azureCredential)
                {
                    SubscriptionId = this.azureCredential.DefaultSubscriptionId
                };
            }
            client.SetUserAgent(ComponentName);
        }

        public async Task<AzureOperationResponse<Profile2018Compute.Models.VirtualMachine>> CreateLinuxVirtialMachine(
            string resourceGroupName,
            string virtualMachineName,
            string storageAccountName,
            string storagePrefix,
            string nicId,
            string location)
        {
            if (client == null)
            {
                return new AzureOperationResponse<Profile2018Compute.Models.VirtualMachine>
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
                var vmParameters = new Profile2018Compute.Models.VirtualMachine
                {
                    Location = location,
                    NetworkProfile = new Profile2018Compute.Models.NetworkProfile
                    {
                        NetworkInterfaces = new List<Profile2018Compute.Models.NetworkInterfaceReference>
                        {
                            new Profile2018Compute.Models.NetworkInterfaceReference
                            {
                                Id = nicId,
                                Primary = true
                            }
                        }
                    },
                    StorageProfile = new Profile2018Compute.Models.StorageProfile
                    {
                        ImageReference = linuxImageReference,
                        OsDisk = new Profile2018Compute.Models.OSDisk
                        {
                            Name = "osDisk",
                            Vhd = new Profile2018Compute.Models.VirtualHardDisk
                            {
                                Uri = string.Format(vhdURItemplate, storageAccountName, storagePrefix, virtualMachineName)
                            },
                            CreateOption = Profile2018Compute.Models.DiskCreateOptionTypes.FromImage
                        }
                    },
                    OsProfile = new Profile2018Compute.Models.OSProfile
                    {
                        ComputerName = virtualMachineName,
                        AdminUsername = "useradmin",
                        AdminPassword = "userpassword1!"
                    },
                    HardwareProfile = new Profile2018Compute.Models.HardwareProfile
                    {
                        VmSize = "Standard_A1"
                    }
                };
                var virtualMachineTask = await client.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, virtualMachineName, vmParameters);
                return virtualMachineTask;
            }
            catch (Exception ex)
            {
                return new AzureOperationResponse<Profile2018Compute.Models.VirtualMachine>
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
