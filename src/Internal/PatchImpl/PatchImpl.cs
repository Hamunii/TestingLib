using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace TestingLib.Internal.PatchImpl;

internal class IsEditor : TogglablePatch
{
    private static Hook isEditorHook = null!;
    internal override void Init(){
        // For an unkown reason to me, we need to hook this getter early for our patches to apply.
        isEditorHook = new Hook(AccessTools.DeclaredPropertyGetter(typeof(Application), nameof(Application.isEditor)), Override_Application_isEditor);
    }
    internal override void OnEnable(){
        if(isEditorHook == null){
            Plugin.Logger.LogInfo("isEditorHook is null!");
            return;
        }
        isEditorHook.Apply();
    }
    internal override void OnDisable(){
        isEditorHook?.Undo();
    }

    private static bool Override_Application_isEditor(){
        if(Modules.Patch.IsEditor.Enabled)
            return true;
        // We can be here only the first time Application.isEditor is get,
        // and we do this to make sure the debug/test menu is always populated.
        isEditorHook.Undo();
        return true;
    }
}

internal class InfiniteSprint : TogglablePatch
{
    internal override void OnEnable(){
        // Make sure we don't subscribe to event twice
        On.GameNetcodeStuff.PlayerControllerB.Update -= InfiniteSprint_PlayerControllerB_Update;
        On.GameNetcodeStuff.PlayerControllerB.Update += InfiniteSprint_PlayerControllerB_Update;

    }
    internal override void OnDisable(){
        On.GameNetcodeStuff.PlayerControllerB.Update -= InfiniteSprint_PlayerControllerB_Update;
    }

    private static void InfiniteSprint_PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
    {
        orig(self);
        self.sprintMeter = 1;
    }
}

internal class SkipSpawnPlayerAnimation : TogglablePatch
{
    // While this doesn't patch anything, we check its state in another patch
}

internal class OnDeathHeal : TogglablePatch
{
    internal override void OnEnable(){
        On.GameNetcodeStuff.PlayerControllerB.KillPlayer -= OnDeathHeal_PlayerControllerB_KillPlayer;
        On.GameNetcodeStuff.PlayerControllerB.KillPlayer += OnDeathHeal_PlayerControllerB_KillPlayer;
    }
    internal override void OnDisable(){
        On.GameNetcodeStuff.PlayerControllerB.KillPlayer -= OnDeathHeal_PlayerControllerB_KillPlayer;
    }

    private static void OnDeathHeal_PlayerControllerB_KillPlayer(On.GameNetcodeStuff.PlayerControllerB.orig_KillPlayer orig, GameNetcodeStuff.PlayerControllerB self, Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
    {   
        self.health = 100;
        self.MakeCriticallyInjured(enable: false);
        HUDManager.Instance.UpdateHealthUI(self.health, hurtPlayer: false);
        // Lazy hacky method to get rid of the broken glass effect.
        self.DamagePlayer(damageNumber: 0, hasDamageSFX: false);
    }
}

internal class MovementCheat : TogglablePatch
{
    internal override void OnEnable(){
        On.GameNetcodeStuff.PlayerControllerB.Jump_performed -= MovementCheat_PlayerControllerB_Jump_performed;
        On.GameNetcodeStuff.PlayerControllerB.Jump_performed += MovementCheat_PlayerControllerB_Jump_performed;
        On.GameNetcodeStuff.PlayerControllerB.Update -= MovementCheat_PlayerControllerB_Update;
        On.GameNetcodeStuff.PlayerControllerB.Update += MovementCheat_PlayerControllerB_Update;

    }
    internal override void OnDisable(){
        On.GameNetcodeStuff.PlayerControllerB.Jump_performed -= MovementCheat_PlayerControllerB_Jump_performed;
        On.GameNetcodeStuff.PlayerControllerB.Update -= MovementCheat_PlayerControllerB_Update;
    }

    private static void MovementCheat_PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self) {
        if (self.isSpeedCheating){
            self.walkForce *= 0.8f;
            self.walkForce = Vector3.ClampMagnitude(self.walkForce, 0.1f);
            if(self.isSprinting){
                self.fallValue -= 130 * Time.deltaTime;
                self.thisController.Move(self.walkForce * 700 * Time.deltaTime);
            }
            else{
                self.thisController.Move(self.walkForce * 200 * Time.deltaTime);
            }
        }
        orig(self);
    }
    private static void MovementCheat_PlayerControllerB_Jump_performed(On.GameNetcodeStuff.PlayerControllerB.orig_Jump_performed orig, GameNetcodeStuff.PlayerControllerB self, UnityEngine.InputSystem.InputAction.CallbackContext context) {
        self.playerSlidingTimer = 0f;
        self.isJumping = true;
        self.sprintMeter = Mathf.Clamp(self.sprintMeter - 0.08f, 0f, 1f);
        self.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
        if(self.jumpCoroutine != null){
            self.StopCoroutine(self.jumpCoroutine);
            // Cheat stuff
            self.isSpeedCheating = true;
            if(self.isSprinting){
                self.jumpForce = 50f;
            }
            else{
                self.jumpForce = 13f;
            }
            self.jumpCoroutine = self.StartCoroutine(CustomPlayerJump(self));
            return;
        }
        self.jumpCoroutine = self.StartCoroutine(self.PlayerJump());
    }
    // It turns out, using a transpiler on an IEnumerator is not as easy.
    private static IEnumerator CustomPlayerJump(GameNetcodeStuff.PlayerControllerB self) {
        self.playerBodyAnimator.SetBool("Jumping", value: true);
        self.fallValue = self.jumpForce;
        self.fallValueUncapped = self.jumpForce;
        yield return new WaitForSeconds(0.1f);
        self.isJumping = false;
        self.isFallingFromJump = true;
        yield return new WaitUntil(() => self.thisController.isGrounded);
        self.playerBodyAnimator.SetBool("Jumping", value: false);
        self.isFallingFromJump = false;
        self.PlayerHitGroundEffects();
        self.jumpCoroutine = null;
        // Cheat stuff
        self.isSpeedCheating = false;
        self.jumpForce = 13f;
    }
}

internal class InfiniteCredits : TogglablePatch
{
    internal override void OnEnable(){
        On.Terminal.RunTerminalEvents -= InfiniteCredits_Terminal_RunTerminalEvents;
        On.Terminal.RunTerminalEvents += InfiniteCredits_Terminal_RunTerminalEvents;
    }
    internal override void OnDisable(){
        On.Terminal.RunTerminalEvents -= InfiniteCredits_Terminal_RunTerminalEvents;
    }

    private static void InfiniteCredits_Terminal_RunTerminalEvents(On.Terminal.orig_RunTerminalEvents orig, Terminal self, TerminalNode node)
    {
        self.groupCredits = 100000000;
        orig(self, node);
    }
}

internal class InfiniteShotgunAmmo : TogglablePatch
{
    internal override void OnEnable(){
        IL.ShotgunItem.ItemActivate -= InfiniteShotgunAmmo_ShotgunItem_ItemActivate;
        IL.ShotgunItem.ItemActivate += InfiniteShotgunAmmo_ShotgunItem_ItemActivate;
    }
    internal override void OnDisable(){
        IL.ShotgunItem.ItemActivate -= InfiniteShotgunAmmo_ShotgunItem_ItemActivate;
    }

    private static void InfiniteShotgunAmmo_ShotgunItem_ItemActivate(ILContext il)
    {
        /*
        // Find this:

        if (this.shellsLoaded == 0)
        {
            this.StartReloadGun();
            return;
        }

        // And replace: brtrue.s (branch if shellsLoaded is not zero)
        //        with: br       (branch unconditionally)
        //
        // But because brtrue.s pops a value from the stack
        // and br doesn't, we add a Pop before it.
        */
        ILCursor c = new(il);
        c.GotoNext(
            x => x.MatchLdfld<ShotgunItem>("shellsLoaded")
        );

        // ldfld int32 ShotgunItem::shellsLoaded => pop
        c.Remove();
        c.Emit(OpCodes.Pop);

        // brtrue.s IL_0020                      => br IL_0020
        c.Next.OpCode = OpCodes.Br;
    }
}

internal class LogGameMethods : TogglablePatch
{
    internal static List<EventHook>? allHooks;
    
    internal override void OnEnable(){
        if(allHooks != null){
            foreach(var hook in allHooks){
                hook.Apply();
            }
            return;
        }
        InitGenericHooks();
    }
    internal override void OnDisable(){
        if(allHooks != null){
            foreach(var hook in allHooks){
                hook.Undo();
            }
        }
    }

    private static void InitGenericHooks(){
        Type[] types = typeof(IL.GameNetcodeStuff.PlayerControllerB).Assembly.GetTypes();
        allHooks = new List<EventHook>();
        var allEvents = new List<EventInfo>();
        foreach(Type type in types){
            if(type.ToString().StartsWith("On.")                    // not IL hooks
                || type.ToString().StartsWith("IL.Dissonance.")     // Takes time
                || type.ToString().StartsWith("IL.DunGen.")         // Takes time
                || type.ToString().StartsWith("IL.DigitalRuby.")    // Takes time (lightning bolt stuff?)
                ) continue;
            
            List<EventInfo> methods = new List<EventInfo>(
                type.GetEvents()
            );

            foreach (var eventInfo in methods){
                allEvents.Add(eventInfo);
            }
        }
        int idx = 0;
        int finalAmount = allEvents.Count;
        var watchTotal = Stopwatch.StartNew();

        foreach (var eventInfo in allEvents){
            idx++;
            long timeAtBeginning = watchTotal.ElapsedMilliseconds;

            allHooks.Add(new EventHook(eventInfo, GenericPatch));

            long elapsedTime = watchTotal.ElapsedMilliseconds - timeAtBeginning;
            if(elapsedTime >= 10){
                Plugin.Logger.LogInfo($"({idx}/{finalAmount} methods hooked) | Took a long time hooking (took {elapsedTime}ms) {eventInfo.DeclaringType}::{eventInfo.Name}");
            }
        }
        watchTotal.Stop();
        Plugin.Logger.LogInfo($"({idx}/{finalAmount} methods hooked) | Took {watchTotal.ElapsedMilliseconds}ms in total (I don't know where the extra time comes from)");
    }

    private static void GenericPatch(ILContext il){
        ILCursor c = new(il);
        var method = il.Method;
        c.EmitDelegate<Action>(() => {
            StackTrace stackTrace = new StackTrace(); 
            if(stackTrace.GetFrame(2) == null)
                Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) ? Caller: Unknown: stackTrace.GetFrame(2) was null");
            else
                Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) | Caller: {stackTrace.GetFrame(2).GetMethod().DeclaringType}::{stackTrace.GetFrame(2).GetMethod().Name}");
            if(GameNetworkManager.Instance.localPlayerController)
                Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) | > Pos: {GameNetworkManager.Instance.localPlayerController.transform.position}");
            Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) V Start of Method: {method.Name}");
        });
        bool foundReturn;
        do{
            foundReturn = c.TryGotoNext(x => x.MatchRet());
            if(foundReturn){
                c.EmitDelegate<Action>(() => {
                    StackTrace stackTrace = new StackTrace(); 
                    // if(GameNetworkManager.Instance.localPlayerController)
                    //     Plugin.Logger.LogDebug($"--  Pos: {GameNetworkManager.Instance.localPlayerController.transform.position}");
                    if(GameNetworkManager.Instance.localPlayerController)
                        Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) | > Pos: {GameNetworkManager.Instance.localPlayerController.transform.position}");
                    Plugin.Logger.LogDebug($"(frame: {Time.frameCount}) ^ End of Method: {method.Name}");
                });
                c.Index++;
            }
        }
        while(foundReturn == true);
    }
}

/* 
internal class MyPatch : TogglablePatch
{
    internal override void OnEnable(){

    }
    internal override void OnDisable(){

    }
}
*/