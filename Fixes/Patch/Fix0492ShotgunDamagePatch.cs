// -----------------------------------------------------------------------
// <copyright file="Fix0492ShotgunDamagePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PlayerStatsSystem;

#pragma warning disable SA1118 // Parameter should span multiple lines

namespace Mistaken.Fixes.Patches
{
    [HarmonyPatch(typeof(FirearmDamageHandler), nameof(FirearmDamageHandler.ProcessDamage))]
    internal static class Fix0492ShotgunDamagePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Brfalse_S);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.characterClassManager))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CharacterClassManager), nameof(CharacterClassManager.CurClass))),
                new CodeInstruction(OpCodes.Ldc_I4, (int)RoleType.Scp0492),
                new CodeInstruction(OpCodes.Ceq),
                new CodeInstruction(OpCodes.Or),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }
}
