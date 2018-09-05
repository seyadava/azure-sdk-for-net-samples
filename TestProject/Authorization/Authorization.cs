namespace Authorization
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Rest;

    public class CustomLoginCredentials : ServiceClientCredentials
    {
        private string clientId;
        private string clientSecret;
        private string resourceId;
        private string tenantId;

        private const string authenticationBase = "https://login.windows.net/{0}";

        public CustomLoginCredentials(string servicePrincipalId, string servicePrincipalSecret, string azureEnvironmentResourceId, string azureEnvironmentTenandId)
        {
            clientId = servicePrincipalId;
            clientSecret = servicePrincipalSecret;
            resourceId = azureEnvironmentResourceId;
            tenantId = azureEnvironmentTenandId;
        }

        private string AuthenticationToken { get; set; }
        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            var authenticationContext =
                new AuthenticationContext(String.Format(authenticationBase, tenantId));
            var credential = new ClientCredential(clientId, clientSecret);
            var result = authenticationContext.AcquireTokenAsync(resource: resourceId,
                clientCredential: credential).Result;
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            AuthenticationToken = result.AccessToken;
        }
    }
}
