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
    internal static class FixServerDyingPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label continueLabel = generator.DefineLabel();

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Callvirt);

            newInstructions[index].WithLabels(continueLabel);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Dup),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(NetworkConnection), nameof(NetworkConnection.connectionId))),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ceq),
                new(OpCodes.Brfalse_S, continueLabel),
                new(OpCodes.Pop),
                new(OpCodes.Ret),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
