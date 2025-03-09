# REPOLib
[![GitHub](https://img.shields.io/badge/GitHub-REPOLib-brightgreen?style=for-the-badge&logo=GitHub)](https://github.com/ZehsTeam/REPOLib)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![NuGet Version](https://img.shields.io/nuget/v/zehs.repolib?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Zehs.REPOLib)

#### Library for adding content to R.E.P.O.

## Features
- Registering network prefabs.
- Registering valuables.
- Registering enemys.

## Usage
<details><summary>Click to expand</summary><br>

Reference [REPOLib](https://www.nuget.org/packages/Zehs.REPOLib) in your project's `.csproj` file.
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

Registering a valuable to a specific level.
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

        // Valuables Presets:
        // "Valuables - Generic"
        // "Valuables - Wizard"
        // "Valuables - Manor"
        // "Valuables - Arctic"

        List<string> presets = new List<string> { "Valuables - Wizard" };

        // Register a valuable.
        REPOLib.Modules.Valuables.RegisterValuable(prefab, presets);
    }
}
```


Registering a enemy.
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
        EnemySetup enemy = assetBundle.LoadAsset<EnemySetup>("your_enemy_setup");

        // Register a valuable.
        REPOLib.Modules.Enemies.RegisterEnemy(enemy);
    }
}
```
</details>

> [!NOTE]
> Registering valuables automatically registers them as a network prefab. 
> (Same for enemy spawn objects)

> [!IMPORTANT]
> You should only register network prefabs and features from your plugins awake function.

> [!TIP]
> You can enable extended logging in the config settings to get more info about features being registered, custom network prefabs being spawned, and more.

## Contribute
Anyone is free to contribute.

https://github.com/ZehsTeam/REPOLib

## Developer Contact
#### Report bugs, suggest features, or provide feedback:  
- **GitHub Issues Page:** [REPOLib](https://github.com/ZehsTeam/REPOLib/issues)
- **Email:** crithaxxog@gmail.com
- **Twitch:** [CritHaxXoG](https://www.twitch.tv/crithaxxog)
- **YouTube:** [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

[![kofi](https://i.imgur.com/jzwECeF.png)](https://ko-fi.com/zehsteam)