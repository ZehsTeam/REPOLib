# v4.2.0

- Fixed registering network prefabs that were already registered with the same prefab ID and GameObject throwing an error instead of returning the already registered PrefabRef.
  - This *should* fix some mods that previously weren't working with REPOLib v4.1.0
- Made bundle setter in SDK Content ScriptableObjects public. (#85)

## v4.1.0

- Updated for R.E.P.O. v0.4.4 (#84)

### Changes ported from vanilla

- Improved cosmetic save file backup logic. (#84)
- Switched level loading images to use PrefabRef. (#84)

### Additional Changes

- Fixed config failing to initialize if a patch fails to load. (#84)
- Fixed modded cosmetics not initially showing in multiplayer. (#84)

## v4.0.4

- Fixed gaining infinite tokens if an exception occurs whilst attempting to save the meta files. (#82)

## v4.0.3

- Fixed vanilla meta save backups not being created. (#81)

## v4.0.2

- Fixed customize menu not working. (#76)
- Implemented the meta save file backup logic from v0.4.2 (#76)
- Improved the duplicate check for cosmetics (#76)

## v4.0.1

- Fixed enemies spawned with REPOLib not being able to see players.

## v4.0.0

- Updated for R.E.P.O. v0.4.0 (#72)
- Added support for registering cosmetics. (#72)
  - Added Cosmetics module.
  - Added CosmeticContent to SDK.
- Fixed vanilla developer mode being disabled when enabled by vanilla or other mods. (#70)

## v3.0.4

- Updated the documentation URL in the README.
- Updated the BepInExPack dependency target version in the manifest.

## v3.0.3

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

<br><br>
<details><summary>Older Versions</summary>

# v2.1.0

- Added Upgrades module. (#29)
- Fixed not being able to spawn vanilla network prefabs in singleplayer.
- Made all extension methods and a few other non-module things private.
  - Existing mods using these things should still work, but it is recommended that they switch to another solution.

## v2.0.1

- Fixed BundleLoader causing an infinite loading screen when loading invalid content.
- Moved documentation to the [R.E.P.O. Modding Wiki](https://repomods.com/apis/repolib/overview.html).
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

- Added `RaiseMasterClient` to [`REPOLib.Modules.NetworkingEvents` class.
- Added `RaiseEvent` method to [`REPOLib.Modules.NetworkedEvent` class.
  - This method works to call in singleplayer.

## v1.4.0

- Added REPOLib as default dependency to the `Mod` asset. (#11)
- Added `REPOLib.Modules.Utilities` class that contains a function `public static void FixAudioMixerGroups(GameObject prefab);` to fix the audio mixer groups on a prefab and their children. (#10)
- Registering features (Valuables, Items, Enemies, etc...) will now automatically fix their prefabs audio mixer groups.
- Added `REPOLib.Modules.NetworkedEvent` class to easily manage your event codes when using `PhotonNetwork.RaiseEvent();` (#12)

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

- Small improvement to `NetworkPrefabs.cs`, `Valuables.cs`, `CustomPrefabPool.cs`, `LevelValuablesExtension.cs`, and other.
- Added `public static IReadOnlyList<GameObject> RegisteredValuables { get; }` to `Valuables.cs`

## v1.0.1

- Updated mod icon.

## v1.0.0

- Initial release.
