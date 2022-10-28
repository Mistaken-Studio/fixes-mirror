// -----------------------------------------------------------------------
// <copyright file="RoundStartedPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
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
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(RoundStartedPatch), nameof(AlreadyStarted))),
                new(OpCodes.Brtrue, label),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Call, AccessTools.PropertySetter(typeof(RoundStartedPatch), nameof(AlreadyStarted))),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
