// -----------------------------------------------------------------------
// <copyright file="FixServerDyingPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(NetworkConnection), "TransportReceive")]
    internal class FixServerDyingPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label label = generator.DefineLabel();

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Callvirt);

            newInstructions[index].WithLabels(label);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(NetworkConnection), nameof(NetworkConnection.connectionId))),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ceq),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ret),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }
}
