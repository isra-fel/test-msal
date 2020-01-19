using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.Common.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Linq;
using System.Dynamic;

namespace TestMsal
{
    public class MockPs
    {
        private static readonly string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        //private static readonly string Tenant = "54826b22-38d6-4fb2-bad9-b7b93a3e9c5a";
        private static readonly string CacheFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ".IdentityService",
            "msal.cache"
        );


        public static IPublicClientApplication PublicClientApp;

        public void CreatePublicClient(string authority = null)
        {
            PublicClientApp = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(authority ?? "https://login.microsoftonline.com/organizations")
                .WithRedirectUri("http://localhost:8400")
                .WithLogging((level, message, pii) =>
                    {
                        Console.WriteLine(string.Format("[MSAL] {0}: {1}", level, message));
                    })
                .Build();

            RegisterCache(PublicClientApp);
        }

        private void RegisterCache(IPublicClientApplication client)
        {
            var cacheHelper = GetCacheHelper();
            cacheHelper.RegisterCache(client.UserTokenCache);
        }

        private MsalCacheHelper GetCacheHelper()
        {
            Console.WriteLine($"CacheFilePath: {CacheFilePath}");
            var builder = new StorageCreationPropertiesBuilder(Path.GetFileName(CacheFilePath), Path.GetDirectoryName(CacheFilePath), ClientId);
            builder = builder.WithMacKeyChain(serviceName: "Microsoft.Developer.IdentityService", accountName: "MSALCache");
            builder = builder.WithLinuxKeyring(
                schemaName: "msal.cache",
                collection: "default",
                secretLabel: "MSALCache",
                attribute1: new KeyValuePair<string, string>("MsalClientID", "Microsoft.Developer.IdentityService"),
                attribute2: new KeyValuePair<string, string>("MsalClientVersion", "1.0.0.0"));
            var storageCreationProperties = builder.Build();
            return MsalCacheHelper.CreateAsync(storageCreationProperties).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public AuthenticationResult AcquireTokenInteractive(string scope = null)
        {
            return PublicClientApp.AcquireTokenInteractive(new string[] { scope ?? "https://management.core.windows.net//user_impersonation" })
                        .WithCustomWebUi(new CustomWebUi())
                                      .ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        }

        public static void Main()
        {
            var mock = new MockPs();

            // common token (interactive)
            mock.CreatePublicClient("https://login.microsoftonline.de/organizations");
            var result = mock.AcquireTokenInteractive("https://management.core.cloudapi.de//user_impersonation");
            var accounts = PublicClientApp.GetAccountsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (accounts.Count() == 0)
            {
                throw new Exception("Cannot acquire user account");
            }
            else
            {
                Console.WriteLine($"{accounts.Count()} accounts acquired");
            }

            // tenant token (silent)
            mock.CreatePublicClient("https://login.microsoftonline.com/5bc0604d-40a5-4aa7-894a-a538fb85dcda/");
            var accessToken = PublicClientApp.AcquireTokenSilent(new string[] { "https://management.core.windows.net//user_impersonation" }, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine($"Access token: {accessToken.AccessToken}");


            // common token again
            mock.CreatePublicClient("https://login.microsoftonline.com/common");
            accessToken = PublicClientApp.AcquireTokenSilent(new string[] { "https://management.core.windows.net//user_impersonation" }, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
