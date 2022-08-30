// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Hints;

namespace Mistaken.Fixes
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "Fixes";

        /// <inheritdoc/>
        public override string Prefix => "MFixes";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Higher - 1;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(5, 2, 2);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            this.harmony = new HarmonyLib.Harmony("com.mistaken.fixes");
            this.harmony.PatchAll();

            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(HintDisplay).GetMethod(nameof(HintDisplay.Show), BindingFlags.Instance | BindingFlags.Public));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();

            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;

            new FixItemsDisappearOnEscapeHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            this.harmony.UnpatchAll();

            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private HarmonyLib.Harmony harmony;

        private void Server_WaitingForPlayers()
        {
            Patches.RoundStartedPatch.AlreadyStarted = false;
            MEC.Timing.RunCoroutine(Patches.YeetConsolePatch.UpdateConsolePrint());
        }
    }
}
