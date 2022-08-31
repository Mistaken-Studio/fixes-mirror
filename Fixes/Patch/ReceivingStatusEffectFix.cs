// -----------------------------------------------------------------------
// <copyright file="ReceivingStatusEffectFix.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using CustomPlayerEffects;
using HarmonyLib;

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(Visuals939), nameof(Visuals939.Disabled))]
    internal static class ReceivingStatusEffectFix
    {
        private static void Prefix()
        {
            Visuals939.EnabledEffects.RemoveAll(x => x == null || x.gameObject.Equals(null));
        }
    }
}
