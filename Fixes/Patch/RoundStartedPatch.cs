// -----------------------------------------------------------------------
// <copyright file="RoundStartedPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Mistaken.Fixes.Patch
{
#pragma warning disable SA1118
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.SetStartClassList))]
    internal static class RoundStartedPatch
    {
        public static bool AlreadyStarted { get; set; } = false;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var label = generator.DefineLabel();

            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            newInstructions[newInstructions.Count - 1].WithLabels(label);

            newInstructions.InsertRange(newInstructions.Count - 2, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(RoundStartedPatch), "AlreadyStarted")),
                new CodeInstruction(OpCodes.Brtrue, label),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(RoundStartedPatch), "AlreadyStarted")),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }
    }
}
