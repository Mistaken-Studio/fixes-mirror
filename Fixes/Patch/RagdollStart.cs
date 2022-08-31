// -----------------------------------------------------------------------
// <copyright file="RagdollStart.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.Fixes.Patch
{
    // [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Start))]
    internal static class RagdollStart
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var label = generator.DefineLabel();

            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            var index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ldarg_0);

            newInstructions[index].WithLabels(label);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Brtrue_S, label),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }
    }
}
