namespace Compute
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Authorization;
    using Profile2018Compute = Microsoft.Azure.Management.Profiles.hybrid_2018_03_01.Compute;
    using Microsoft.Azure.Management.Compute.Fluent.Models;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Rest.Azure;

    public class ComputeController
    {
        private const string ComponentName = "DotnetSDK_ComputeController";
        private const string vhdURItemplate = "https://{0}.blob.{1}/vhds/{2}.vhd";
        private readonly CustomLoginCredentials customCredential;
        private readonly AzureCredentials azureCredential;
        private readonly string subscriotionId;
        private readonly string environment;
        private readonly Uri baseUri;
        private static Profile2018Compute.ComputeManagementClient client;
        private static IAzure azure;

        private readonly Profile2018Compute.Models.ImageReference linuxImageReference = new Profile2018Compute.Models.ImageReference
        {
            Publisher = "Canonical",
            Offer = "UbuntuServer",
            Sku = "16.04-LTS",
            Version = "latest"
        };

        public ComputeController(
                Uri baseUri,
                CustomLoginCredentials credentials,
                string subscriptionIdentifier,
                string environment = "azurestack")
        {
            this.baseUri = baseUri;
            this.customCredential = credentials;
            this.subscriotionId = subscriptionIdentifier;
            this.environment = environment;
            GetComputeClient();
        }

        public ComputeController(
            Uri baseUri,
            AzureCredentials credentials,
            string environment = "azurestack")
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                this.baseUri = baseUri;
                this.azureCredential = credentials;
                GetComputeClient();
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

        private async Task<AzureOperationResponse<string>> CreateVirtialMachineWithManagedDiskStack(
            string resourceGroupName,
            string virtualMachineName,
            string nicId,
            string diskId,
            string location,
            Profile2018Compute.Models.ImageReference imageReference = null)
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
                        DataDisks = new List<Profile2018Compute.Models.DataDisk>
                        {
                            new Profile2018Compute.Models.DataDisk
                            {
                                CreateOption = Profile2018Compute.Models.DiskCreateOptionTypes.Attach,
                                ManagedDisk = new Profile2018Compute.Models.ManagedDiskParameters
                                {
                                    StorageAccountType = Profile2018Compute.Models.StorageAccountTypes.StandardLRS,
                                    Id = diskId
                                }
                            }
                        },
                        OsDisk = new Profile2018Compute.Models.OSDisk
                        {
                            Name = "osDisk",
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

                if (imageReference == null)
                {
                    vmParameters.StorageProfile.ImageReference = linuxImageReference;
                }
                else
                {
                    vmParameters.StorageProfile.ImageReference = imageReference;
                }

                var virtualMachineTask = await client.VirtualMachines.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, virtualMachineName, vmParameters);
                return new AzureOperationResponse<string>
                {
                    Body = virtualMachineTask.Body.Id
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

        private async Task<AzureOperationResponse<string>> CreateVirtialMachineWithManagedDiskAzure(
            string resourceGroupName,
            string virtualMachineName,
            string nicId,
            string diskId,
            string location)
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
                var nic = await azure.NetworkInterfaces.GetByIdAsync(nicId);

                var disk = await azure.Disks.GetByIdAsync(diskId);

                var vm = await azure.VirtualMachines.Define(virtualMachineName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithExistingPrimaryNetworkInterface(nic)
                    .WithPopularLinuxImage(Microsoft.Azure.Management.Compute.Fluent.KnownLinuxVirtualMachineImage.UbuntuServer16_04_Lts)
                    .WithRootUsername("useradmin")
                    .WithRootPassword("userpassword1!")
                    .WithExistingDataDisk(disk)
                    .WithSize(VirtualMachineSizeTypes.StandardA1)
                    .CreateAsync();

                return new AzureOperationResponse<string>
                {
                    Body = vm.Id
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

        public async Task<AzureOperationResponse<string>> CreateVirtialMachineWithManagedDisk(
            string resourceGroupName,
            string virtualMachineName,
            string nicId,
            string diskId,
            string location,
            Profile2018Compute.Models.ImageReference imageReference = null)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreateVirtialMachineWithManagedDiskStack
                    (resourceGroupName, virtualMachineName, nicId, diskId, location, imageReference);
            }
            else
            {
                return await CreateVirtialMachineWithManagedDiskAzure
                    (resourceGroupName, virtualMachineName, nicId, diskId, location);
            }
        }

        private async Task<AzureOperationResponse<string>> CreateDiskStack(
            string resourceGroupName,
            string diskName,
            int sizeGb,
            string location,
            Profile2018Compute.Models.StorageAccountTypes? diskSku = null)
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
                var diskParams = new Profile2018Compute.Models.Disk
                {
                    CreationData = new Profile2018Compute.Models.CreationData
                    {
                        CreateOption = Profile2018Compute.Models.DiskCreateOption.Empty,
                    },
                    Location = location,
                    Sku = new Profile2018Compute.Models.DiskSku
                    {
                        Name = diskSku ?? Profile2018Compute.Models.StorageAccountTypes.StandardLRS
                    },
                    DiskSizeGB = sizeGb,
                };
                var diskTask = await client.Disks.CreateOrUpdateWithHttpMessagesAsync(resourceGroupName, diskName, diskParams);
                return new AzureOperationResponse<string>
                {
                    Body = diskTask.Body.Id
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

        private async Task<AzureOperationResponse<string>> CreateDiskAzure(
            string resourceGroupName,
            string diskName,
            int sizeGb,
            string location)
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
                var dataDisk = await azure.Disks.Define(diskName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithData()
                    .WithSizeInGB(sizeGb)
                    .CreateAsync();

                return new AzureOperationResponse<string>
                {
                    Body = dataDisk.Id
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

        public async Task<AzureOperationResponse<string>> CreateDisk(
            string resourceGroupName,
            string diskName,
            int sizeGb,
            string location,
            Profile2018Compute.Models.StorageAccountTypes? diskSku = null)
        {
            if (String.Equals(environment, "azurestack", StringComparison.CurrentCultureIgnoreCase))
            {
                return await CreateDiskStack
                    (resourceGroupName, diskName, sizeGb, location, diskSku);
            }
            else
            {
                return await CreateDiskAzure
                    (resourceGroupName, diskName, sizeGb, location);
            }
        }
    }
}
