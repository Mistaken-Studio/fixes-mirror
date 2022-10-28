// -----------------------------------------------------------------------
// <copyright file="RoundInProgressPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using HarmonyLib;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.RoundInProgress))]
    internal static class RoundInProgressPatch
    {
        private static bool Prefix(ref bool __result)
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
