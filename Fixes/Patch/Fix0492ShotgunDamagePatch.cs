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

namespace Mistaken.Fixes.Patch
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
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(CharacterClassManager), nameof(CharacterClassManager.CurClass))),
                new(OpCodes.Ldc_I4, (int)RoleType.Scp0492),
                new(OpCodes.Ceq),
                new(OpCodes.Or),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
