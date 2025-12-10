# v3.0.3

- Forced BundleLoader to register LevelContent assets first.

## v3.0.2

- Fixed custom level ambiences not working.

## v3.0.1

- Fixed getting stuck on the REPOLib loading bundles screen.

## v3.0.0

> [!IMPORTANT] Almost every mod will need to update to support this version of REPOLib.

- Updated for R.E.P.O. v0.3.0
- REPOLib-Sdk valuable and item mods *should* work without needing to update.
- All content register method signatures have changed due to `PrefabRef` being introduced.
  - You will need to recompile your mods for them to work with this version.
- Deprecated some getter methods in `Valuables`, `Items`, `Enemies`, and `Levels` modules.
- Added support for registering debug chat commands using the `Commands` module.
- Removed REPOLib command attributes. Use the `Commands` module instead.
- Removed REPOLib spawn commands since they now exist in vanilla.
- You can now add vanilla valuables to your level valuable presets using `PrefabRef`s.
- Registering enemies requires you to put your spawn objects in the `EnemyContent` ScriptableObject.
- Registering levels requires you to put your modules in the `LevelContent` ScriptableObject.

<details><summary>Older Versions</summary>

# v2.1.0

- Added Upgrades module. (#29)
- Fixed not being able to spawn vanilla network prefabs in singleplayer.
- Made all extension methods and a few other non-module things private.
  - Existing mods using these things should still work, but it is recommended that they switch to another solution.

## v2.0.1

- Fixed BundleLoader causing an infinite loading screen when loading invalid content.
- Moved documentation to the [R.E.P.O. Modding Wiki](https://repomods.com/repolib/overview.html).
- Added XML documentation for most public API classes, methods, and properties. (#40)

## v2.0.0

- Added support for registering levels. (#32)
- Added new async bundle loading system. (#15)
- Added more safety checks when instantiating a network prefab.
- Changed most of the extended logs to use the `Debug` log level, instead of `Info`. (#33)
- Split the `DeveloperMode` config option into two ones:
  - `DeveloperMode` enables developer-only chat commands.
  - `VanillaDeveloperMode` enables the vanilla developer keyboard shortcuts.
- Fixed the SpawnEnemy method ignoring the spawnDespawned parameter always spawning enemies not despawned.
- **Breaking changes:**
  - Changed the signature of `BundleLoader.LoadBundle` (due to async loading).
  - Moved `GenericValuablePresetName` and `ValuablePresets` from `Modules.Valuables` to `Modules.ValuablePresets`.

## v1.5.0

- Added partial name matching to all spawn commands. (#17)
- Added a new developer mode command `Spawn Enemy`.
  - Usage: `/spawnenemy <name>` (`/se` for short)
  - You must enable `DeveloperMode` mode in the config settings to use developer mode commands.
  - Note: developer mode commands are host-only!
- Added methods/properties to the Valuables module.
  - `SpawnValuable` - Spawn a valuable.
  - `AllValuables` and `GetValuables` - Returns a list of all the valuables registered in the game. (Vanilla and Modded)
  - `GetValuableByName` and `TryGetValuableByName` - Returns a valuable prefab that equals the name.
  - `GetValuableThatContainsName` and `TryGetValuableThatContainsName` - Returns a valuable prefab that contains the name.
- Added methods/properties to the Items module.
  - `SpawnItem` - Spawn an item.
  - `AllItems` and `GetItems` - Returns a list of all the items registered in the game. (Vanilla and Modded)
  - `GetItemByName` and `TryGetItemByName` - Returns an item that equals the name.
  - `GetItemThatContainsName` and `TryGetItemThatContainsName` - Returns an item that contains the name.
- Added methods/properties to the Enemies module.
  - `SpawnEnemy` - Spawn an enemy.
  - `AllEnemies` and `GetEnemies` - Returns a list of all the enemies registered in the game. (Vanilla and Modded)
  - `GetEnemyByName` and `TryGetEnemyByName` - Returns an EnemySetup that equals the name.
  - `GetEnemyThatContainsName` and `TryGetEnemyThatContainsName` - Returns an EnemySetup that contains the name.
- Added a method to the NetworkPrefabs module.
  - `SpawnNetworkPrefab` - Spawn a network prefab by providing a prefab ID.
    - This method works in both multiplayer and singleplayer.
    - Note: this will only spawn registered network prefabs.
- You can now toggle developer mode using the REPOConfig mod.
- You can now register enemy groups if you have already registered that enemy previously.

## v1.4.2

- Removed changelog field from the `Mod` asset. (#14)

### REPOLib-Sdk v1.2.0

- Added an `Extra Files` field to the `Mod` asset. (REPOLib-Sdk#7)
  - You can put your changelog file here.

## v1.4.1

- Added [`RaiseMasterClient`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkedEvent.cs#L32) to [`REPOLib.Modules.NetworkingEvents`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkedEvent.cs) class.
- Added [`RaiseEvent`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkedEvent.cs#L128) method to [`REPOLib.Modules.NetworkedEvent`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkedEvent.cs#L101) class.
  - This method works to call in singleplayer.

## v1.4.0

- Added REPOLib as default dependency to the `Mod` asset. (#11)
- Added [`REPOLib.Modules.Utilities`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/Utilities.cs) class that contains a function [`public static void FixAudioMixerGroups(GameObject prefab);`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/Utilities.cs#L28) to fix the audio mixer groups on a prefab and their children. (#10)
- Registering features (Valuables, Items, Enemies, etc...) will now automatically fix their prefabs audio mixer groups.
- Added [`REPOLib.Modules.NetworkedEvent`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkedEvent.cs#L96) class to easily manage your event codes when using [`PhotonNetwork.RaiseEvent();`](https://doc.photonengine.com/pun/current/gameplay/rpcsandraiseevent) (#12)

## v1.3.1

- Added changelog field to the `Mod` asset. (#9)

## v1.3.0

- Added more validation when registering features to prevent conflicts and errors.
- Added support for registering custom chat /commands. (#5)
  - Added some built-in developer mode commands: `/spawnvaluable <name>`, `/spawnitem <name>` (`/sv` and `/si` for short)
    - You must enable `DeveloperMode` mode in the config settings to use developer mode commands.
    - Note: developer mode commands are host-only!

## v1.2.0

- Added support for registering items.
- Added support for registering enemies. (#2)
- Added support for registering features without code using the [REPOLib-Sdk](https://github.com/ZehsTeam/REPOLib-Sdk). (#3)
- Features now register network prefabs to have their prefabId match the Resources folder structure.
- You can no longer manually pass in a prefabId when registering a valuable.

## v1.1.0

- You can now register valuables to specific levels. (#1)
  - Valuables Presets: `Valuables - Generic`, `Valuables - Wizard`, `Valuables - Manor`, `Valuables - Arctic`

## v1.0.2

- Small improvement to [`NetworkPrefabs.cs`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/NetworkPrefabs.cs), [`Valuables.cs`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/Valuables.cs), [`CustomPrefabPool.cs`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Objects/CustomPrefabPool.cs), [`LevelValuablesExtension.cs`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Extensions/LevelValuablesExtension.cs), and other.
- Added `public static IReadOnlyList<GameObject> RegisteredValuables { get; }` to [`Valuables.cs`](https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Modules/Valuables.cs#L11)

## v1.0.1

- Updated mod icon.

## v1.0.0

- Initial release.
