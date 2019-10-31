# test-msal

test msal and extensions library

## What the code does

Look at `Main()` of `MockPs.cs`. It basically does:

1. Create a public client application
1. Register cache
1. Acquire token interactively
1. Acquire token silently

## Expected Behavior

Cache file created at

> C:\Users\abc\AppData\Local\.IdentityService\msal.cache

## Actual Behavior

Powershell core: cache file created.

Windows powershell: cache file **not** created.

## How to repro

1. Build
1. Copy the following dlls into `.\TestMsal\bin\Debug\netstandard2.0`
    - `Microsoft.Identity.Client.dll` (.net standard)
    - `Microsoft.Identity.Client.Extensions.Msal.dll` (.net standard)
    - `System.Security.Cryptography.ProtectedData.dll`
1. Open either powershell core or windows powershell and run
    ```powershell
    cd .\TestMsal\bin\Debug\netstandard2.0
    Add-Type -Path .\TestMsal.dll
    [TestMsal.MockPs]::Main()
    ```

## Notes

-   We must use a customer Web UI because the .net standard version of msal does not have a built-in Web UI: https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/blob/1cef43d185bb0a1dcc013d2ec232ae9d4c6c053a/src/client/Microsoft.Identity.Client/Platforms/netstandard13/WebUIFactory.cs#L15
