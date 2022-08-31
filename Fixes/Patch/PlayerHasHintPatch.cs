// -----------------------------------------------------------------------
// <copyright file="PlayerHasHintPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using Exiled.API.Features;
using HarmonyLib;
using Hints;
using MEC;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
    internal static class PlayerHasHintPatch
    {
        private static readonly Dictionary<Player, CoroutineHandle> PlayerHasHintCoroutines = new Dictionary<Player, CoroutineHandle>();
        private static readonly MethodInfo HasHintSetMethod = typeof(Player).GetProperty(nameof(Player.HasHint), BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);

        private static void Postfix(HintDisplay __instance, Hint hint)
        {
            if (__instance == null || __instance.gameObject is null || !(Player.Get(__instance.gameObject) is Player player))
                return;

            if (PlayerHasHintCoroutines.TryGetValue(player, out CoroutineHandle oldcoroutine))
                Timing.KillCoroutines(oldcoroutine);

            PlayerHasHintCoroutines[player] = Timing.RunCoroutine(HasHintToFalse(player, hint.DurationScalar));

            if (!player.HasHint)
            {
                HasHintSetMethod.Invoke(player, new object[] { true });
            }
        }

        private static IEnumerator<float> HasHintToFalse(Player player, float duration)
        {
            yield return Timing.WaitForSeconds(duration);

            if (player.GameObject is null)
                yield break;

            HasHintSetMethod.Invoke(player, new object[] { false });
            PlayerHasHintCoroutines.Remove(player);
        }
    }
}
