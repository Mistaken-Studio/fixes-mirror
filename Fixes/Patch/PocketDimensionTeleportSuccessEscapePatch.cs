// -----------------------------------------------------------------------
// <copyright file="PocketDimensionTeleportSuccessEscapePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.SuccessEscape), typeof(ReferenceHub))]
    internal static class PocketDimensionTeleportSuccessEscapePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldloc_0) + 1;

            Label skipLabel = generator.DefineLabel();
            newInstructions[index].WithLabels(skipLabel);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Dup),
                new(OpCodes.Brtrue_S, skipLabel),
                new(OpCodes.Pop),
                new(OpCodes.Ldc_I4, (int)MapGeneration.RoomName.Hcz106),
                new(OpCodes.Ldc_I4, (int)MapGeneration.FacilityZone.HeavyContainment),
                new(OpCodes.Ldc_I4, (int)MapGeneration.RoomShape.Endroom),
                new(OpCodes.Call, AccessTools.Method(typeof(MapGeneration.RoomIdUtils), nameof(MapGeneration.RoomIdUtils.FindRooms))),
                new(OpCodes.Call, AccessTools.FirstMethod(typeof(System.Linq.Enumerable), (x) =>
                    {
                        return x.Name == "Single" && x.GetGenericArguments().Length == 1 && x.GetParameters().Length == 1 && x.ContainsGenericParameters;
                    }).MakeGenericMethod(typeof(MapGeneration.RoomIdentifier))),
                new(OpCodes.Dup),
                new(OpCodes.Stloc_0),
                new(OpCodes.Ldstr, "PocketDimensionTeleportSuccessEscapePatch works"),
                new(OpCodes.Call, AccessTools.Method(typeof(Log), nameof(Log.Info), new[] { typeof(string) })),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
