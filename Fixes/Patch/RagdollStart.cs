﻿// -----------------------------------------------------------------------
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
    [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Start))]
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
                new(OpCodes.Dup),
                new(OpCodes.Brtrue_S, label),
                new(OpCodes.Pop),
                new(OpCodes.Ret),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
