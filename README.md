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
- Fixing audio mixer groups.
- Making networked events.
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

> [!IMPORTANT]
> You should only register network prefabs and features (Valuables, Items, Enemies, etc...) from your plugin's awake function.

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

> [!TIP]
> Commands can be enabled/disabled in the config settings.

If you are a mod developer and want to add your own custom chat commands to your mod, check the `Usage > Chat commands` section.

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