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
        private static readonly Dictionary<Player, CoroutineHandle> _playerHasHintCoroutines = new();
        private static readonly MethodInfo _hasHintSetMethod = typeof(Player).GetProperty(nameof(Player.HasHint), BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);

        private static void Postfix(HintDisplay __instance, Hint hint)
        {
            if (__instance == null || __instance.gameObject is null || (Player.Get(__instance.gameObject) is not Player player))
                return;

            if (_playerHasHintCoroutines.TryGetValue(player, out CoroutineHandle oldcoroutine))
                Timing.KillCoroutines(oldcoroutine);

            _playerHasHintCoroutines[player] = Timing.RunCoroutine(HasHintToFalse(player, hint.DurationScalar));

            if (!player.HasHint)
            {
                _hasHintSetMethod.Invoke(player, new object[] { true });
            }
        }

        private static IEnumerator<float> HasHintToFalse(Player player, float duration)
        {
            yield return Timing.WaitForSeconds(duration);

            if (player.GameObject is null)
                yield break;

            _hasHintSetMethod.Invoke(player, new object[] { false });
            _playerHasHintCoroutines.Remove(player);
        }
    }
}
