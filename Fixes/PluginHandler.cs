// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using Hints;
using Mistaken.Fixes.Patch;
using Mistaken.Updater.API.Config;

namespace Mistaken.Fixes
{
    internal sealed class PluginHandler : Plugin<Config>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "Fixes";

        public override string Prefix => "MFixes";

        public override PluginPriority Priority => PluginPriority.Higher - 1;

        public override Version RequiredExiledVersion => new(5, 2, 2);

        public AutoUpdateConfig AutoUpdateConfig => new()
        {
            Type = SourceType.GITLAB,
            Url = "https://git.mistaken.pl/api/v4/projects/110",
        };

        public override void OnEnabled()
        {
            Instance = this;

            _harmony.PatchAll();

            Exiled.Events.Events.DisabledPatchesHashSet.Add(typeof(HintDisplay).GetMethod(nameof(HintDisplay.Show), BindingFlags.Instance | BindingFlags.Public));
            Exiled.Events.Events.Instance.ReloadDisabledPatches();

            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;

            new FixItemsDisappearOnEscapeHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _harmony.UnpatchAll();

            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private static readonly Harmony _harmony = new("com.mistaken.fixes");

        private void Server_WaitingForPlayers()
        {
            RoundStartedPatch.AlreadyStarted = false;
            Mistaken.API.Diagnostics.Module.RunSafeCoroutine(YeetConsolePatch.UpdateConsolePrint(), nameof(YeetConsolePatch.UpdateConsolePrint), true);
        }
    }
}
