# TestingLib

**WARNING: This is not ready for use yet! Things can and will change without warning when it is updated. This library is used on the [experimental branch of LC-ExampleEnemy](https://github.com/Hamunii/LC-ExampleEnemy/tree/experimental)**.

This is a tool intended for making testing of enemy mods faster in Lethal Company. This is intended to be used in debug builds of your mods. For example:

```cs
private void Awake() {
    Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    var ExampleEnemy = Assets.MainAssetBundle.LoadAsset<EnemyType>("ExampleEnemy");
    // ...
    #if DEBUG
    TestingLib.Patch.isEditor();
    TestingLib.Patch.SkipSpawnPlayerAnimation();
    TestingLib.Macro.OnPlayerSpawn.ToggleTestRoom();
    TestingLib.Macro.OnPlayerSpawn.TeleportSelf(TestingLib.Macro.OnPlayerSpawn.TeleportLocation.Outside);
    TestingLib.Macro.OnPlayerSpawn.SpawnEnemyInFrontOfSelf(ExampleEnemy);
    #endif
}
```

## Modules

### TestingLib.Patch

`isEditor()`  
Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.

`SkipSpawnPlayerAnimation()`  
Skips the spawn player animation so you can start moving and looking around as soon as you spawn.

### TestingLib.Macro.OnPlayerSpawn

The following macros apply as soon as the player spawns:

`ToggleTestRoom();`  
Toggles the testing room from the debug menu.


`TeleportSelf(TeleportLocation location = 0);`  
- `TeleportLocation.Inside = 1`
- `TeleportLocation.Outside = 2`  

Teleports you to the location specified, in the test room.

`SpawnEnemyInFrontOfSelf(EnemyType enemy);`  
Spawns the specified enemy in front of you. Currently somewhat broken: enemy might appear invisible.

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
    StartCoroutine(TestingLib.Enemy.DrawPathIEnumerator(line, agent.path, transform));
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

And lastly, add `TestingLib.dll` to your plugins folder.