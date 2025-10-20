## Venly SDK for Unity3D

# Version 3.1.0
- Wallet API
- Token API
- Pay API

# Quick start

Initialization (call once at startup)
```csharp
using Venly;
using Venly.Core;
using UnityEngine;

public class VenlyBootstrap : MonoBehaviour
{
    void Start()
    {
        if (!VenlyAPI.IsInitialized)
        {
            VenlyUnity.Initialize();
        }
    }
}
```

Create a user (PIN signing)
```csharp
using System.Threading.Tasks;
using Venly;
using Venly.Models.Wallet;

async Task<VyUserDto> CreateUserAsync(string pin, string reference = "my-user")
{
    var request = new VyCreateUserRequest
    {
        Reference = reference,
        SigningMethod = new VyCreatePinSigningMethodRequest { Value = pin }
    };

    var result = await VenlyAPI.Wallet.CreateUser(request);
    if (!result.Success) throw result.Exception;
    return result.Data;
}
```

Get NFTs for a wallet (non-async VyTask)
```csharp
using Venly;
using Venly.Models.Wallet;
using UnityEngine;

void LoadNfts(string walletId)
{
    VenlyAPI.Wallet.GetNfts(walletId)
        .OnSuccess(nfts =>
        {
            // handle NFTs (e.g., display or cache)
            Debug.Log($"Loaded {nfts.Length} NFTs");
        })
        .OnFail(ex =>
        {
            Debug.LogError(ex);
        });
}
```

# Backend Provider Support
- DevMode (Editor Only)
- PlayFab
- Beamable
- Custom

# API Explorer (Sample)
- Wallet API samples
- Token API samples
- User Login/Create Flow (Beamable/PlayFab)

# Sample Project (VenlyDash)
- [Source Code](https://github.com/ArkaneNetwork/Unity-SDK-Samples/tree/main/VenlyDash-Sample)
- [Play (WebGL)](https://venly.me/venlydash)

# Documentation
Documentation can be found [here](https://docs.venly.io/docs/getting-started-with-unity)
