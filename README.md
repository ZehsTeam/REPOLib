# REPOLib
[![GitHub](https://img.shields.io/badge/GitHub-REPOLib-brightgreen?style=for-the-badge&logo=GitHub)](https://github.com/ZehsTeam/REPOLib)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![NuGet Version](https://img.shields.io/nuget/v/zehs.repolib?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Zehs.REPOLib)

**Library for adding content to R.E.P.O.**

## Features
- Registering network prefabs.
- Registering valuables.
- Registering items.
- Registering enemies.
- Registering custom chat /commands
    - Built-in dev mode commands: Spawn Valuable, Spawn Item
- Registering features without code using the [REPOLib-Sdk](https://github.com/ZehsTeam/REPOLib-Sdk).

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

<details><summary>Network prefabs</summary><br>

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

</details>

<details><summary>Valuables</summary><br>

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

</details>

<details><summary>Items</summary><br>

Registering an item.
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
        Item item = assetBundle.LoadAsset<Item>("your_item");

        // Register an item.
        REPOLib.Modules.Items.RegisterItem(item);
    }
}
```
</details>

<details><summary>Enemies</summary><br>

Registering an enemy.
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

        // Register an enemy.
        REPOLib.Modules.Enemies.RegisterEnemy(enemy);
    }
}
```
</details>

<details><summary>Chat commands</summary><br>

Registering a chat /command.
```cs
public static class YourCommand
{
    // ...

    [CommandInitializer]
    public static void Initialize()
    {
        // Perform any setup or caching
    }

    [CommandExecution(
        "Your Command Name",
        "Description of what the command does and how to use it.",
        enabledByDefault: true,
        requiresDeveloperMode: false,
        )]
    [CommandAlias("yourcommand")]
    [CommandAlias("yourcmd")]
    public static void Execute(string args)
    {
        // ...
    }
}
```
</details>
</details>

> [!NOTE]
> Registering valuables, items, and enemies automatically registers their prefabs as a network prefab. 

> [!IMPORTANT]
> You should only register network prefabs and features from your plugin's awake function.

> [!TIP]
> You can enable extended logging in the config settings to get more info about features being registered, custom network prefabs being spawned, and more.

> [!TIP]
> You can enable developer mode in the config settings to get access to the `/spawnvaluable <name>` and `/spawnitem <name>` chat commands (`/sv` and `/si` for short). HOST ONLY!

## Contribute
Anyone is free to contribute.

https://github.com/ZehsTeam/REPOLib

To set up the project, copy the `REPOLib.csproj.user.example` file to `REPOLib.csproj.user`. If needed, change the settings found in that file.

## Developer Contact
**Report bugs, suggest features, or provide feedback:**
- **GitHub Issues Page:** [REPOLib](https://github.com/ZehsTeam/REPOLib/issues)
- **Email:** crithaxxog@gmail.com
- **Twitch:** [CritHaxXoG](https://www.twitch.tv/crithaxxog)
- **YouTube:** [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

[![kofi](https://i.imgur.com/jzwECeF.png)](https://ko-fi.com/zehsteam)