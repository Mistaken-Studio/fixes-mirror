// -----------------------------------------------------------------------
// <copyright file="FixShotgunPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patches
{
    [HarmonyPatch(typeof(BuckshotHitreg), nameof(BuckshotHitreg.ServerPerformShot))]
    internal static class FixShotgunPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Stloc_2) - 1;
            int end = newInstructions.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ble_Un_S) - 2;

            newInstructions.RemoveRange(index, end - index + 1);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(BuckshotHitreg), nameof(BuckshotHitreg.Hits))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FixShotgunPatch), nameof(FixShotgunPatch.ApplyHits))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc_2),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }

        private static float ApplyHits(BuckshotHitreg instance, Dictionary<IDestructible, List<BuckshotHitreg.ShotgunHit>> hits)
        {
            float num = 0f;
            foreach (IDestructible target in hits.Keys)
            {
                float damage = 0f;
                Vector3 hitPosition = Vector3.zero;
                foreach (BuckshotHitreg.ShotgunHit hit in hits[target])
                {
                    damage += hit.Damage;
                    hitPosition += hit.RcResult.point;
                    instance.PlaceBloodDecal(hit.RcRay, hit.RcResult, target);
                }

                hitPosition /= hits[target].Count;

                if (target.Damage(damage, new PlayerStatsSystem.FirearmDamageHandler(instance.Firearm, damage, false), hitPosition))
                    num += damage;
            }

            return num;
        }
    }
}
