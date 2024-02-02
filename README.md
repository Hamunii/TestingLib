# TestingLib

**WARNING: This is not ready for use yet! Things can and will change without warning when it is updated. This library is used on the [experimental branch of LC-ExampleEnemy](https://github.com/Hamunii/LC-ExampleEnemy/tree/experimental)**.

This is a tool intended for making testing of enemy mods faster in Lethal Company. This is intended to be used in debug builds of your mods. For example:

```cs
// ... in your Plugin class:
private void Awake() {
    Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    var ExampleEnemy = Assets.MainAssetBundle.LoadAsset<EnemyType>("ExampleEnemy");
    // ...
    #if DEBUG
    TestingLib.Patch.All();
    TestingLib.OnEvent.PlayerSpawn_20ms_delay += OnEvent_PlayerSpawn_20ms_delay;
    #endif
}

#if DEBUG
private void OnEvent_PlayerSpawn_20ms_delay()
{
    TestingLib.Tools.TeleportSelf(TestingLib.Tools.TeleportLocation.Outside);
    TestingLib.Tools.SpawnEnemyInFrontOfSelf(ExampleEnemy.enemyName);
}
#endif
// ...
```

## TestingLib Modules

### TestingLib.Patch

Contains patching methods that can be run at any time or are time sensitive.

`All()`  
Patches all methods in `Patch.AnyTime` and `Patch.TimeSensitive`:  
`IsEditor()`  
`InfiniteSprint()`  
`SkipSpawnPlayerAnimation()`  
`ToggleTestRoom()` // runs on `OnEvent.PlayerSpawn`

#### TestingLib.Patch.AnyTime

`IsEditor()`  
Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.

`SkipSpawnPlayerAnimation()`  
Skips the spawn player animation so you can start moving and looking around as soon as you spawn.

`InfiniteSprint()`  
Patches the game to allow infinite sprinting by always setting SprintMeter to full.

#### TestingLib.Patch.TimeSensitive

Contains patching methods that can't be run at any moment, mostly too early.  

Every method in this class has information on when it can/should be run.

`ToggleTestRoom()`  
Toggles the testing room from the debug menu.  
Should be ran on `OnEvent.PlayerSpawn` or later.

### TestingLib.OnEvent

Contains Events that can be subscribed to.

`PlayerSpawn`  
Event for when player spawns.

`PlayerSpawn_20ms_delay`  
Event 20 milliseconds after when player spawns.

**Why:** Sometimes, what feels like a chance of 1/8, enemies don't get rendered until player steps on the ship.<br/>
This is if we teleport from the ship immediately, and that causes the game to sometimes not register that the player ever stood on the ship.
So as a hacky workaround for now, we are waiting 20 ms to make sure the game has registered that the player is on the ship.

If you can find the part in the game's code which causes this (and how to make it think the player was on the ship), please open an issue or a pull request.

### TestingLib.Tools

Contains helpful methods for testing.

`TeleportSelf(TeleportLocation location = 0)`  
- `TeleportLocation.Inside = 1`
- `TeleportLocation.Outside = 2`  
Teleports you to the location specified in the test level.

`SpawnEnemyInFrontOfSelf(string enemyName)`  
Will find the enemy by name, and spawn it. Limitation: will only spawn one enemy.  
Previously was somewhat broken: enemy might have appeared invisible. No idea if I fixed it or not.

### TestingLib.Enemy

`DrawPath(LineRenderer line, NavMeshPath path, Transform fromPosition)`  
Draws the NavMeshAgent's pathfinding. Should be used in `DoAIInterval()`. Do note that you need to add line renderer in your enemy prefab. This can be done as such:
```cs
// ... in your enemy class:

#if DEBUG
LineRenderer line;
#endif

public override void Start()
{
    base.Start();
    // ...
    #if DEBUG
    line = gameObject.AddComponent<LineRenderer>();
    line.widthMultiplier = 0.2f; // reduce width of the line
    #endif
}

public override void DoAIInterval()
{
    base.DoAIInterval();
    // ...
    #if DEBUG
    StartCoroutine(TestingLib.Enemy.DrawPath(line, agent.path, transform));
    #endif
}
```

## Using This For Yourself

Add this to your into your plugin class:

```diff
 [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
+[BepInDependency(TestingLib.Plugin.ModGUID, BepInDependency.DependencyFlags.SoftDependency)] 
 public class Plugin : BaseUnityPlugin {
     // ...
 }
```
Also include a reference in your csproj file:

```diff
<ItemGroup>
+  <Reference Include="TestingLib"><HintPath>./my/path/to/TestingLib.dll</HintPath></Reference>
</ItemGroup>
```
Also keep the `TestingLib.xml` file next to `TestingLib.dll` to get method documentation.

And lastly, add `TestingLib.dll` to your plugins folder.