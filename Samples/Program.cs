using System;
using Authorization;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            const string subscriptionId = "fa9ea22d-a053-4a9e-9e76-d7f71c1359de";

            //const string location = "redmond";
            //var baseUri = new Uri("https://management.redmond.ext-v.masd.stbtest.microsoft.com/");
            const string location = "eastus";
            var baseUri = new Uri("https://management.azure.com/");

            string servicePrincipalId = "5acab3e0-d042-49e0-86e1-cca5c52c165b";
            string servicePrincipalSecret = "683c1b3e-5479-451d-9186-9ee6b5f130b7";
            string azureEnvironmentResourceId = "https://management.core.windows.net/";
            string azureEnvironmentTenandId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

            var credentials = new CustomLoginCredentials(
                servicePrincipalId, servicePrincipalSecret, azureEnvironmentResourceId, azureEnvironmentTenandId);

            var resourceClient = Resources.GetClient(baseUri, credentials, subscriptionId);
            var rgname = "test-dotnet-rg-3";

            var rgTask = Resources.CreateResourceGroup(rgname, location);
            var rg = rgTask.Result;
        }
    }
}
