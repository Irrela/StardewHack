﻿using System;
using System.Reflection.Emit;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewHack.AlwaysScrollMap
{
    public class ModConfig {
        /** Whether the mod is enabled upon load. */
        public bool EnabledIndoors = true;
        public bool EnabledOutdoors = false;
        /** Which key should be used to toggle always scroll map. */
        public SButton ToggleScroll = SButton.OemSemicolon;
    }
    
    public class ModEntry : HackWithConfig<ModEntry, ModConfig>
    {
        public static bool Enabled() {
            if (Game1.currentLocation.IsOutdoors)
                return getInstance().config.EnabledOutdoors;
            else
                return getInstance().config.EnabledIndoors;
        }

        public override void Entry(IModHelper helper) {
            base.Entry(helper);
            Helper.Events.Input.ButtonPressed += ToggleScroll;
        }

        private void ToggleScroll(object sender, ButtonPressedEventArgs e) {
            if (e.Button.Equals(config.ToggleScroll)) {
                if (StardewValley.Game1.currentLocation.IsOutdoors) {
                    config.EnabledOutdoors ^= true;
                } else {
                    config.EnabledIndoors ^= true;
                }
            }
        }

        [BytecodePatch("StardewValley.Game1::UpdateViewPort")]
        void Game1_UpdateViewPort()
        {
            var range = FindCode(
                // if (!Game1.viewportFreeze ...
                Instructions.Ldsfld(typeof(Game1), nameof(Game1.viewportFreeze))
            );
            range.Extend(
                // if (Game1.currentLocation.forceViewportPlayerFollow)
                // (Note: currentLocation is a field on PC and a property on android).
                Instructions.Ldfld(typeof(GameLocation), nameof(GameLocation.forceViewportPlayerFollow)),
                OpCodes.Brfalse
            );
            // Encapsulate with if (!State.Enabled) {
            range.Prepend(
                Instructions.Call(typeof(ModEntry), nameof(ModEntry.Enabled)),
                Instructions.Brtrue(AttachLabel(range.End[0]))
            );
            range.ReplaceJump(2, range[0]);
        }
    }
}

