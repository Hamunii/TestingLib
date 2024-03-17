using TestingLib.Attributes;
using TestingLib.Internal.PatchImpl;

namespace TestingLib.Modules;
/// <summary>
/// Contains methods that patch various things in the game.
/// </summary>
[DevTools(Visibility.Whitelist, Available.Always)]
public static class Patch {

    /// <summary>
    /// Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.
    /// </summary>
    public static TogglablePatch IsEditor = new IsEditor();

    /// <summary>
    /// Patches the game to allow infinite sprinting by always setting SprintMeter to full.
    /// </summary>
    public static TogglablePatch InfiniteSprint = new InfiniteSprint();

    /// <summary>
    /// Skips the spawn player animation so you can start moving and looking around as soon as you spawn.
    /// </summary>
    [DevTools(Visibility.ConfigOnly)]
    public static TogglablePatch SkipSpawnPlayerAnimation = new SkipSpawnPlayerAnimation();

    /// <summary>
    /// Instead of dying, set health to full instead.
    /// <br/><br/>
    /// This helps with testing taking damage from your enemy, without death being a concern.
    /// </summary>
    public static TogglablePatch OnDeathHeal = new OnDeathHeal();

    /// <summary>
    /// Allows jumping at any moment and by performing a double jump, the movement will become much<br/>
    /// faster and a lot more responsive, and running will also increase jump height and gravity.
    /// <br/><br/>
    /// <b>Note:</b> This completely overrides PlayerControllerB's <c>Jump_performed()</c> method.
    /// </summary>
    public static TogglablePatch MovementCheat = new MovementCheat();
    

    /// <summary>
    /// Credits get always set to <c>100 000 000</c>.
    /// </summary>
    public static TogglablePatch InfiniteCredits = new InfiniteCredits();

    /// <summary>
    /// Skips the check for ammo when using the shotgun.
    /// </summary>
    public static TogglablePatch InfiniteShotgunAmmo = new InfiniteShotgunAmmo();

    /// <summary>
    /// Hooks nearly every method in the base game and provides a LOT of log information.<br/>
    /// Mainly useful for inspecting where a certain variable gets changed, which is currently unsupported.<br/>
    /// Warning: will generate large log files and kills performance if Debug logs are shown on console.
    /// </summary>
    [DevTools(Visibility.Whitelist, Available.PlayerSpawn, Permission.AllClients, defaultValue: false)]
    public static TogglablePatch LogGameMethods = new LogGameMethods();
    
    /// <summary>
    /// Enables all patches in <c>TestingLib.Patch</c>:
    /// <br/>
    /// <br/><c>Patch.IsEditor</c>
    /// <br/><c>Patch.SkipSpawnPlayerAnimation</c>
    /// <br/><c>Patch.OnDeathHeal</c>
    /// <br/><c>Patch.MovementCheat</c>
    /// <br/><c>Patch.InfiniteSprint</c>
    /// <br/><c>Patch.InfiniteCredits</c>
    /// <br/><c>Patch.InfiniteShotgunAmmo</c>
    /// </summary>
    [DevTools(Visibility.MenuOnly)]
    public static void PatchAll() {
        IsEditor.Enabled = true;
        SkipSpawnPlayerAnimation.Enabled = true;
        OnDeathHeal.Enabled = true;
        MovementCheat.Enabled = true;
        InfiniteSprint.Enabled = true;
        InfiniteCredits.Enabled = true;
        InfiniteShotgunAmmo.Enabled = true;
        // LogGameMethods.Enabled = true; I don't think we want to call this
    }

    /// <summary>
    /// Unpatches all applied patches.
    /// </summary>
    [DevTools(Visibility.MenuOnly)]
    public static void UnpatchAll(){
        IsEditor.Enabled = false;
        SkipSpawnPlayerAnimation.Enabled = false;
        OnDeathHeal.Enabled = false;
        MovementCheat.Enabled = false;
        InfiniteSprint.Enabled = false;
        InfiniteCredits.Enabled = false;
        InfiniteShotgunAmmo.Enabled = false;
        // LogGameMethods.Enabled = false;
    }
}