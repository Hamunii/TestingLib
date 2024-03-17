using System;

namespace TestingLib.Modules;
/// <summary>
/// Contains Events that can be subscribed to.
/// </summary>
public static class OnEvent {
    /// <summary>
    /// Event for when player spawns.
    /// <br/><br/>
    /// Called on <c>On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation</c>.
    /// </summary>
    public static event Action PlayerSpawn = null!;
    /// <summary>
    /// Hook into <c>PlayerControllerB.SpawnPlayerAnimation</c>.
    /// </summary>
    internal static void Init(){
        On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation += PlayerControllerB_SpawnPlayerAnimation;
    }

    private static void PlayerControllerB_SpawnPlayerAnimation(On.GameNetcodeStuff.PlayerControllerB.orig_SpawnPlayerAnimation orig, GameNetcodeStuff.PlayerControllerB self)
    {
        PlayerSpawn?.Invoke();
        if(Patch.SkipSpawnPlayerAnimation.Enabled) {
            Plugin.Logger.LogInfo("Patch: SkipSpawnPlayerAnimation");
        }
        else{
            orig(self);
        }
    }
}