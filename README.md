# REPOLib
[![GitHub](https://img.shields.io/badge/GitHub-REPOLib-brightgreen?style=for-the-badge&logo=GitHub)](https://github.com/ZehsTeam/REPOLib)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Zehs/REPOLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/repo/p/Zehs/REPOLib/)
[![NuGet Version](https://img.shields.io/nuget/v/zehs.repolib?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Zehs.REPOLib)

**Library for adding content to R.E.P.O.**

## Features
- **Registering network prefabs.**
- **Registering valuables.**
- **Registering items.**
- **Registering enemies.**
- **Registering levels.**
- ResourcesHelper to help get network prefab IDs.
- Method to spawn network prefabs. (Which works in both multiplayer and singleplayer)
- Methods to get valuables and spawn valuables.
- Methods to get items and spawn items.
- Methods to get enemies and spawn enemies.
- Registering custom chat /commands
    - Built-in dev mode commands: `Spawn Valuable`, `Spawn Item`, `Spawn Enemy`
- **Fixing audio mixer groups.**
- Making networked events.
- **Registering features without code using the [REPOLib-Sdk](https://github.com/ZehsTeam/REPOLib-Sdk).**

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


<details><summary>Levels</summary><br>

Registering a level.
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
        Level level = assetBundle.LoadAsset<Level>("your_level");

        // Register a level.
        REPOLib.Modules.Levels.RegisterLevel(level);
    }
}
```

</details>

<details><summary>Chat commands</summary><br>

Registering a chat /command.
```cs
using REPOLib.Commands;

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

<details><summary>Fixing audio mixer groups</summary><br>

Fixing audio mixer groups on a prefab and their children.
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
        GameObject prefab = assetBundle.LoadAsset<GameObject>("your_prefab");

        // Fix the audio mixer groups on a prefab and their children.
        REPOLib.Modules.Utilities.FixAudioMixerGroups(prefab);
    }
}
```
Registering any features will automatically fix their prefabs audio mixer groups.
</details>


<details><summary>Networked events</summary><br>

Creating a networked event.
```cs
using ExitGames.Client.Photon;
using REPOLib.Modules;

[BepInPlugin("You.YourMod", "YourMod", "1.0.0")]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class YourMod : BaseUnityPlugin
{
    // ...

    public static NetworkedEvent ExampleEvent;

    private void Awake()
    {
        // ...

        ExampleEvent = new NetworkedEvent("My Example Event", HandleExampleEvent);
    }

    // EventData is from ExitGames.Client.Photon
    private static void HandleExampleEvent(EventData eventData)
    {
        string message = (string)eventData.CustomData;
        Debug.Log($"Received message from example event: {message}");
    }
}
```

Calling a networked event.
```cs
// The data you are sending through your networked event.
string message = "Hello World!";

// Call networked event on everyone. (This works in singleplayer)
ExampleEvent.RaiseEvent(message, REPOLib.Modules.NetworkingEvents.RaiseAll, SendOptions.SendReliable);

// Call networked event on everyone but yourself. (This works in singleplayer)
ExampleEvent.RaiseEvent(message, REPOLib.Modules.NetworkingEvents.RaiseOthers, SendOptions.SendReliable);

// Call networked event on the master client. (This works in singleplayer)
ExampleEvent.RaiseEvent(message, REPOLib.Modules.NetworkingEvents.RaiseMasterClient, SendOptions.SendReliable);
```
</details>
</details>

> [!NOTE]
> Registering features (Valuables, Items, Enemies, etc...) automatically registers their prefabs as a network prefab. 

> [!NOTE]
> Registering features (Valuables, Items, Enemies, etc...) automatically fixes their prefabs audio mixer groups. 

> [!TIP]
> You can enable extended logging in the config settings to get more info about features being registered, custom network prefabs being spawned, and more.

## Chat Commands
> [!IMPORTANT]
> You must enable `DeveloperMode` in the config settings to use developer mode commands.

Chat commands currently only work in multiplayer since you need access to the in-game chat to use commands.

This mod comes with a few built-in chat commands:

### 1. Spawn Valuable `/spawnvaluable <name>`
This command will spawn a valuable in front of you.\
Replace `<name>` with the name of the valuable prefab.\
Names are not case-sensitive.\
Example usage: `/spawnvaluable diamond`\
This command has multiple aliases: `/spawnval`, `/sv`\
<ins>**This command requires developer mode to be enabled.**</ins>\
<ins>**This command is host-only!**</ins>

### 2. Spawn Item `/spawnitem <name>`
This command will spawn an item in front of you.\
Replace `<name>` with the name of the item or item prefab.\
Names are not case-sensitive.\
Example usage: `/spawnitem gun`\
This command has one alias: `/si`\
<ins>**This command requires developer mode to be enabled.**</ins>\
<ins>**This command is host-only!**</ins>

### 3. Spawn Enemy `/spawnenemy <name>`
This command will spawn an enemy on top of you after a few seconds.\
Replace `<name>` with the name of the enemy or enemy prefab.\
Names are not case-sensitive.\
Example usage: `/spawnenemy huntsman`\
This command has one alias: `/se`\
<ins>**This command requires developer mode to be enabled.**</ins>\
<ins>**This command is host-only!**</ins>

> [!TIP]
> Commands can be enabled/disabled in the config settings.

If you are a mod developer and want to add your own custom chat commands to your mod, check the `Usage > Chat commands` section.

## Contribute
Anyone is free to contribute.

https://github.com/ZehsTeam/REPOLib

To set up the project, copy the `REPOLib.csproj.user.example` file to `REPOLib.csproj.user`. If needed, change the settings found in that file.

## Bundle Loading

REPOLib loads any bundles under the `plugins` folder with the `.repobundle` extension. These bundles are then scanned for `Mod` and `Content` assets, which allows codeless registration of features in tandem with [REPOLib-Sdk](https://github.com/Zehs/REPOLib-Sdk).

Bundles are loaded asynchronously, which enables other mods to do their initialization while files are being read from disk, which in turn leads to shorter startup times. Hence, using this system is the preferred way to use this library, even if you're already writing your own plugin code.

> [!WARNING]
> If you're writing a mod that interacts with modded content, remember that all REPOLib content may not be registered by game start because of async bundle loading.
>
> To work around this, either do your initialization at a later stage (for example when joining a lobby) or subscribe to the `REPOLib.BundleLoader.OnAllBundlesLoaded` event (however this requires a dependency on REPOLib).


If you want more control over the loading of bundles, you can use the public APIs from `REPOLib.BundleLoader`.

```cs
using REPOLib;
using UnityEngine;
using System.Collections;

BundleLoader.LoadBundle(
    "/path/to/bundle",
    // Callback when the bundle has finished loading, which is guaranteed to happen before the player joins a lobby.
    // Note that this needs to return an IEnumerator.
    OnBundleLoaded,
    // If this is true, REPOLib will load and register all Content assets from the bundle, as if it was loaded automatically.
    // Defaults to false.
    loadContents: true
);

IEnumerator OnBundleLoaded(AssetBundle bundle) {
    Debug.Log("My bundle was loaded!");
    
    // Do some more (asynchronous) setup logic,
    // or, if loadContents is false, load and register your content
    
    yield break;
}
```

> [!IMPORTANT]
> If you are loading bundles manually, **do not** give them the `.repobundle` extension, as that will make them load twice.

## Developer Contact
**Report bugs, suggest features, or provide feedback:**

| **Discord Server** | **Forum** | **Post** |  
|--------------------|-----------|----------|  
| [R.E.P.O. Modding Server](https://discord.com/invite/vPJtKhYAFe) | `#released-mods` | [REPOLib](https://discord.com/channels/1344557689979670578/1346055794533339217) |

- **GitHub Issues Page:** [REPOLib](https://github.com/ZehsTeam/REPOLib/issues)
- **Email:** crithaxxog@gmail.com
- **Twitch:** [CritHaxXoG](https://www.twitch.tv/crithaxxog)
- **YouTube:** [Zehs](https://www.youtube.com/channel/UCb4VEkc-_im0h8DKXlwmIAA)

[![kofi](https://i.imgur.com/jzwECeF.png)](https://ko-fi.com/zehsteam)