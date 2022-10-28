// -----------------------------------------------------------------------
// <copyright file="PlayerMovementSyncForcePosition.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using UnityEngine;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.ForcePosition), typeof(Vector3), typeof(string), typeof(bool), typeof(bool))]
    internal static class PlayerMovementSyncForcePosition
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var label = generator.DefineLabel();
            var label2 = generator.DefineLabel();
            var label3 = generator.DefineLabel();

            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            newInstructions[0].WithLabels(label);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PlayerMovementSync), nameof(PlayerMovementSync.connectionToClient))),
                new(OpCodes.Dup),
                new(OpCodes.Brfalse_S, label2),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(NetworkConnection), nameof(NetworkConnection.isReady))),
                new(OpCodes.Brfalse_S, label3),
                new(OpCodes.Br_S, label),
                new CodeInstruction(OpCodes.Pop).WithLabels(label2),
                new CodeInstruction(OpCodes.Ret).WithLabels(label3),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
