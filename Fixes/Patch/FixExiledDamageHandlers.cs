// -----------------------------------------------------------------------
// <copyright file="FixExiledDamageHandlers.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using HarmonyLib;

#pragma warning disable SA1118 // Parameter should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(DamageHandler), MethodType.Constructor, new Type[] { typeof(Player), typeof(Player) })]
    [HarmonyPatch(typeof(DamageHandler), MethodType.Constructor, new Type[] { typeof(Player), typeof(PlayerStatsSystem.DamageHandlerBase) })]
    internal static class FixExiledDamageHandlers
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label label = generator.DefineLabel();

            newInstructions[0].WithLabels(label);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Brtrue_S, label),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Server), nameof(Server.Host))),
                new CodeInstruction(OpCodes.Starg, 1),
            });

            foreach (CodeInstruction t in newInstructions)
                yield return t;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }
}
