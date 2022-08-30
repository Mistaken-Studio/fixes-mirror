// -----------------------------------------------------------------------
// <copyright file="RoundInProgressPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using HarmonyLib;

namespace Mistaken.Fixes.Patches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.RoundInProgress))]
    internal static class RoundInProgressPatch
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(ref bool __result)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            try
            {
                if (ReferenceHub.LocalHub.characterClassManager.RoundStarted)
                    __result = !RoundSummary.singleton.RoundEnded;
                else
                    __result = false;
            }
            catch
            {
            }

            return false;
        }
    }
}
