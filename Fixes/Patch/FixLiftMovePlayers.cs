// -----------------------------------------------------------------------
// <copyright file="FixLiftMovePlayers.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

#pragma warning disable SA1118 // Parameter should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.MovePlayers))]
    internal static class FixLiftMovePlayers
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label returnLabel = generator.DefineLabel();

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Lift), nameof(Lift.elevators))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FixLiftMovePlayers), nameof(FixLiftMovePlayers.IsNull))),
                new CodeInstruction(OpCodes.Brtrue_S, returnLabel),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }

        private static bool IsNull(Lift.Elevator[] lifts) =>
            lifts.Any(x => x.target == null);
    }
}
