// -----------------------------------------------------------------------
// <copyright file="FixPlayerShowHintPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;
using Hints;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(Player), nameof(Player.ShowHint))]
    internal static class FixPlayerShowHintPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldnull);

            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Newarr, typeof(HintEffect)),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_R4, 1f),
                new CodeInstruction(OpCodes.Ldc_R4, 0f),
                new CodeInstruction(OpCodes.Ldc_R4, 1f),
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(AlphaEffect), new Type[]
                {
                    typeof(float), typeof(float), typeof(float),
                })),
                new CodeInstruction(OpCodes.Stelem_Ref),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }
    }
}
