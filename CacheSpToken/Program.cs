using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

internal class Program
{
    private static readonly string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
    private static readonly string CacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ".IdentityService",
        "msal.cache.plaintext"
    );
    private static async Task Main(string[] args)
    {
        var pca = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithRedirectUri("http://localhost:8400")
            .WithLogging((level, message, pii) =>
                {
                    Console.WriteLine(string.Format("[MSAL] {0}: {1}", level, message));
                })
            .Build();
        await RegisterCacheAsync(pca);
        var result = await pca.AcquireTokenInteractive(new string[] { "https://management.core.windows.net//.default" }).ExecuteAsync().ConfigureAwait(false);


        var cca = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithRedirectUri("http://localhost:8400")
            .WithLogging((level, message, pii) =>
            {
                Console.WriteLine(string.Format("[MSAL] {0}: {1}", level, message));
            })
            .WithClientId("{your client id}")
            .WithClientSecret("{your client secret}")
            .Build();
        await RegisterCacheAsync(cca);
        result = await cca.AcquireTokenForClient(new string[] { "https://management.core.windows.net//.default" }).ExecuteAsync().ConfigureAwait(false);
    }

    private static async Task RegisterCacheAsync(IPublicClientApplication pca)
    {
        var options = new StorageCreationPropertiesBuilder(Path.GetFileName(CacheFilePath), Path.GetDirectoryName(CacheFilePath))
            .WithUnprotectedFile()
            .Build();
        var helper = await MsalCacheHelper.CreateAsync(options).ConfigureAwait(false);
        helper.RegisterCache(pca.UserTokenCache);
    }

    private static async Task RegisterCacheAsync(IConfidentialClientApplication pca)
    {
        var options = new StorageCreationPropertiesBuilder(Path.GetFileName(CacheFilePath), Path.GetDirectoryName(CacheFilePath))
            .WithUnprotectedFile()
            .Build();
        var helper = await MsalCacheHelper.CreateAsync(options).ConfigureAwait(false);
        helper.RegisterCache(pca.AppTokenCache);
    }
}