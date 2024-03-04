using UnityEngine;
using HarmonyLib;
using System.Collections;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using static TestingLib.Attributes;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Diagnostics;
using MonoMod.RuntimeDetour;

namespace TestingLib {
    /// <summary>
    /// Contains methods that patch various things in the game.
    /// </summary>
    [DevTools(Visibility.Whitelist, Available.Always)]
    public class Patch {
        internal static bool shouldSkipSpawnPlayerAnimation = false;
        private static List<EventHook>? allHooks;
        private static Hook? isEditorHook;

        /// <summary>
        /// Patches the game to think it is running in Unity Editor, allowing us to use the in-game debug menu.
        /// </summary>
        public static void IsEditor(){
            if(isEditorHook == null)
                isEditorHook = new Hook(AccessTools.DeclaredPropertyGetter(typeof(Application), nameof(Application.isEditor)), Application_isEditor);
            else
                isEditorHook.Apply();
            //shouldDebug = true;
        }
        private static bool Application_isEditor(){
            return true;
        }

        /// <summary>
        /// Patches the game to allow infinite sprinting by always setting SprintMeter to full.
        /// </summary>
        public static void InfiniteSprint(){
            // Make sure we don't subscribe to event twice
            On.GameNetcodeStuff.PlayerControllerB.Update -= InfiniteSprint_PlayerControllerB_Update;
            On.GameNetcodeStuff.PlayerControllerB.Update += InfiniteSprint_PlayerControllerB_Update;
        }
        private static void InfiniteSprint_PlayerControllerB_Update(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self)
        {
            orig(self);
            self.sprintMeter = 1;
        }

        /// <summary>
        /// Skips the spawn player animation so you can start moving and looking around as soon as you spawn.
        /// </summary>
        [DevTools(Visibility.ConfigOnly)]
        public static void SkipSpawnPlayerAnimation(){
            shouldSkipSpawnPlayerAnimation = true;
        }

        /// <summary>
        /// Instead of dying, set health to full instead.
        /// <br/><br/>
        /// This helps with testing taking damage from your enemy, without death being a concern.
        /// </summary>
        public static void OnDeathHeal(){
            On.GameNetcodeStuff.PlayerControllerB.KillPlayer -= OnDeathHeal_PlayerControllerB_KillPlayer;
            On.GameNetcodeStuff.PlayerControllerB.KillPlayer += OnDeathHeal_PlayerControllerB_KillPlayer;
        }
        private static void OnDeathHeal_PlayerControllerB_KillPlayer(On.GameNetcodeStuff.PlayerControllerB.orig_KillPlayer orig, GameNetcodeStuff.PlayerControllerB self, Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
        {   
            self.health = 100;
            self.MakeCriticallyInjured(enable: false);
            HUDManager.Instance.UpdateHealthUI(self.health, hurtPlayer: false);
            // Lazy hacky method to get rid of the broken glass effect.
            self.DamagePlayer(damageNumber: 0, hasDamageSFX: false);
        }

        /// <summary>
        /// Allows jumping at any moment and by performing a double jump, the movement will become much<br/>
        /// faster and a lot more responsive, and running will also increase jump height and gravity.
        /// <br/><br/>
        /// <b>Note:</b> This completely overrides PlayerControllerB's <c>Jump_performed()</c> method.
        /// </summary>
        public static void MovementCheat(){
            On.GameNetcodeStuff.PlayerControllerB.Jump_performed -= MovementCheat_PlayerControllerB_Jump_performed;
            On.GameNetcodeStuff.PlayerControllerB.Jump_performed += MovementCheat_PlayerControllerB_Jump_performed;
            On.GameNetcodeStuff.PlayerControllerB.Update -= MovementCheat_PlayerControllerB_Update;
            On.GameNetcodeStuff.PlayerControllerB.Update += MovementCheat_PlayerControllerB_Update;
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

        /// <summary>
        /// Credits get always set to <c>100 000 000</c>.
        /// </summary>
        public static void InfiniteCredits(){
            On.Terminal.RunTerminalEvents -= Terminal_RunTerminalEvents;
            On.Terminal.RunTerminalEvents += Terminal_RunTerminalEvents;
        }
        private static void Terminal_RunTerminalEvents(On.Terminal.orig_RunTerminalEvents orig, Terminal self, TerminalNode node)
        {
            self.groupCredits = 100000000;
            orig(self, node);
        }

        /// <summary>
        /// Skips the check for ammo when using the shotgun.
        /// </summary>
        public static void InfiniteShotgunAmmo(){
            IL.ShotgunItem.ItemActivate -= ShotgunItem_ItemActivate;
            IL.ShotgunItem.ItemActivate += ShotgunItem_ItemActivate;
        }

        private static void ShotgunItem_ItemActivate(ILContext il)
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

        /// <summary>
        /// Hooks nearly every method in the base game and provides a LOT of log information.<br/>
        /// Mainly useful for inspecting where a certain variable gets changed, which is currently unsupported.<br/>
        /// Warning: will generate large log files and kill performance if Debug logs are shown on console.<br/>
        /// </summary>
        [DevTools(Visibility.Whitelist, Available.PlayerSpawn, Permission.AllClients, initDefaultValue: false)]
        public static void LogGameMethods(){
            if(allHooks != null){
                foreach(var hook in allHooks){
                    hook.Apply();
                }
                return;
            }
            InitGenericHooks();
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
                    Plugin.Logger.LogInfo($"({idx}/{finalAmount} methods hooked) | Took a long time hooking (took {elapsedTime}ms of total {watchTotal.ElapsedMilliseconds}ms) {eventInfo.DeclaringType}::{eventInfo.Name}");
                }
            }
            watchTotal.Stop();
            Plugin.Logger.LogInfo($"({idx}/{finalAmount} methods hooked) | Took {watchTotal.ElapsedMilliseconds}ms in total");
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

        /// <summary>
        /// Calls all methods in <c>TestingLib.Patch</c>:
        /// <br/>
        /// <br/><c>Patch.IsEditor()</c>
        /// <br/><c>Patch.SkipSpawnPlayerAnimation()</c>
        /// <br/><c>Patch.OnDeathHeal()</c>
        /// <br/><c>Patch.MovementCheat()</c>
        /// <br/><c>Patch.InfiniteSprint()</c>
        /// <br/><c>Patch.InfiniteCredits()</c>
        /// <br/><c>Patch.InfiniteShotgunAmmo()</c>
        /// </summary>
        [DevTools(Visibility.MenuOnly)]
        public static void PatchAll() {
            IsEditor();
            SkipSpawnPlayerAnimation();
            OnDeathHeal();
            MovementCheat();
            InfiniteSprint();
            InfiniteCredits();
            InfiniteShotgunAmmo();
            // LogGameMethods(); I don't think we want to call this
        }

        /// <summary>
        /// Unpatches all applied patches.
        /// </summary>
        [DevTools(Visibility.MenuOnly)] // Used for unpatching when joining game as non-host in DevTools.
        public static void UnpatchAll(){
            isEditorHook?.Undo();
            On.GameNetcodeStuff.PlayerControllerB.Update -= InfiniteSprint_PlayerControllerB_Update;
            shouldSkipSpawnPlayerAnimation = false;
            On.GameNetcodeStuff.PlayerControllerB.KillPlayer -= OnDeathHeal_PlayerControllerB_KillPlayer;
            On.GameNetcodeStuff.PlayerControllerB.Jump_performed -= MovementCheat_PlayerControllerB_Jump_performed;
            On.GameNetcodeStuff.PlayerControllerB.Update -= MovementCheat_PlayerControllerB_Update;
            On.Terminal.RunTerminalEvents -= Terminal_RunTerminalEvents;
            IL.ShotgunItem.ItemActivate -= ShotgunItem_ItemActivate;
            if(allHooks != null){
                foreach(var hook in allHooks){
                    hook.Undo();
                }
            }
        }
    }
}