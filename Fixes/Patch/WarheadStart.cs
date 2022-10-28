// -----------------------------------------------------------------------
// <copyright file="WarheadStart.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(Warhead), nameof(Warhead.Start))]
    internal static class WarheadStart
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            var index = newInstructions.FindIndex(x => x.opcode == OpCodes.Callvirt) + 1;

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(Server), nameof(Server.Host))),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Newobj, AccessTools.GetDeclaredConstructors(typeof(StartingEventArgs), null)[0]),
                new(OpCodes.Call, AccessTools.Method(typeof(Exiled.Events.Handlers.Warhead), nameof(Exiled.Events.Handlers.Warhead.OnStarting))),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
