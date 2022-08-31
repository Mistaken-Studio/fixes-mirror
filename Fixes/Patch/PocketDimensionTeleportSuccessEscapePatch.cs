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
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Brtrue_S, skipLabel),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldc_I4, (int)MapGeneration.RoomName.Hcz106),
                new CodeInstruction(OpCodes.Ldc_I4, (int)MapGeneration.FacilityZone.HeavyContainment),
                new CodeInstruction(OpCodes.Ldc_I4, (int)MapGeneration.RoomShape.Endroom),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MapGeneration.RoomIdUtils), nameof(MapGeneration.RoomIdUtils.FindRooms))),
                new CodeInstruction(OpCodes.Call, AccessTools.FirstMethod(typeof(System.Linq.Enumerable), (x) =>
                    {
                        return x.Name == "Single" && x.GetGenericArguments().Length == 1 && x.GetParameters().Length == 1 && x.ContainsGenericParameters;
                    }).MakeGenericMethod(typeof(MapGeneration.RoomIdentifier))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc_0),
                new CodeInstruction(OpCodes.Ldstr, "PocketDimensionTeleportSuccessEscapePatch works"),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Log), nameof(Log.Info), new System.Type[] { typeof(string) })),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }
    }
}
