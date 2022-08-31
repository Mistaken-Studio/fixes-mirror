// -----------------------------------------------------------------------
// <copyright file="FixExiledSCP914ControllerPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(Exiled.API.Features.Scp914), nameof(Exiled.API.Features.Scp914.Scp914Controller), MethodType.Getter)]
    internal class FixExiledSCP914ControllerPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            newInstructions.InsertRange(2, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality")),
            });

            foreach (var t in newInstructions)
                yield return t;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }
}
