# REPOLib
#### Library for adding content to R.E.P.O.

## <img src="https://i.imgur.com/TpnrFSH.png" width="20px"> Download

Download [REPOLib](https://thunderstore.io/c/repo/p/Zehs/REPOLib/) on Thunderstore.

## Features
- Registering network prefabs.
- Registering valuables.

## Usage
<details><summary>Click to expand</summary><br>

Reference REPOLib in your project's `.csproj` file.
```
<ItemGroup>
  <PackageReference Include="Zehs.REPOLib" Version="1.*" />
</ItemGroup>
```

Add REPOLib as a dependency to your plugin class.
```cs
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
```

```cs
[BepInPlugin("You.YourMod", "YourMod", "1.0.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class YourMod : BaseUnityPlugin
{
    // ...
}
```

Registering a network prefab.
```cs
[BepInPlugin("You.YourMod", "YourMod", "1.0.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class YourMod : BaseUnityPlugin
{
    // ...

    private void Awake()
    {
        // ...

        AssetBundle assetBundle = AssetBundle.LoadFromFile("your_assetbundle_file_path");
        GameObject prefab = assetBundle.LoadAsset<GameObject>("your_network_prefab");

        // Register a network prefab.
        REPOLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(prefab);
    }
}
```

Registering a valuable.
```cs
[BepInPlugin("You.YourMod", "YourMod", "1.0.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class YourMod : BaseUnityPlugin
{
    // ...

    private void Awake()
    {
        // ...

        AssetBundle assetBundle = AssetBundle.LoadFromFile("your_assetbundle_file_path");
        GameObject prefab = assetBundle.LoadAsset<GameObject>("your_valuable_prefab");

        // Register a valuable.
        REPOLib.Modules.Valuables.RegisterValuable(prefab);
    }
}
```

</details>

> [!NOTE]
> Registering a valuable will automatically register it as a network prefab.

> [!IMPORTANT]
> You should only register network prefabs and valuables in your plugins awake function.

## Contribute
Anyone is free to contribute.

https://github.com/ZehsTeam/REPOLib

## Developer Contact
#### Report bugs, suggest features, or provide feedback:  
- **GitHub Issues Page:** [REPOLib](https://github.com/ZehsTeam/REPOLib/issues)
- **Email:** crithaxxog@gmail.com
- **Twitch:** [CritHaxXoG](https://www.twitch.tv/crithaxxog)
- **YouTube:** [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

[<img src="https://i.imgur.com/81tRL6D.png" width="200px">](https://ko-fi.com/zehsteam)