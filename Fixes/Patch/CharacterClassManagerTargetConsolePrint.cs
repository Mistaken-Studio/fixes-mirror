// -----------------------------------------------------------------------
// <copyright file="CharacterClassManagerTargetConsolePrint.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.Fixes.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.TargetConsolePrint), typeof(NetworkConnection), typeof(string), typeof(string))]
    internal static class CharacterClassManagerTargetConsolePrint
    {
        private static readonly Dictionary<ReferenceHub, GameConsoleTransmission> GameConsoleTransmissions = new Dictionary<ReferenceHub, GameConsoleTransmission>();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            var gct = generator.DeclareLocal(typeof(GameConsoleTransmission));
            var label1 = generator.DefineLabel();
            var label2 = generator.DefineLabel();

            newInstructions[0].WithLabels(label1);
            newInstructions[2].WithLabels(label2);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // Current
                // GetComponent<GameConsoleTransmission>().SendToClient(connection, text, color);

                // Target
                // GameConsoleTransmissions[ReferenceHub].SendToClient(connection, text, color)

                // Result
                // if(!ReferenceHubAwake.GameConsoleTransmissions.TryGetValue(this._hub, out gct))
                //      GetComponent<GameConsoleTransmission>()
                // else
                //      gct
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CharacterClassManagerTargetConsolePrint), nameof(CharacterClassManagerTargetConsolePrint.GameConsoleTransmissions))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CharacterClassManager), nameof(CharacterClassManager._hub))),
                new CodeInstruction(OpCodes.Ldloca_S, gct),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dictionary<ReferenceHub, GameConsoleTransmission>), nameof(Dictionary<ReferenceHub, GameConsoleTransmission>.TryGetValue))),
                new CodeInstruction(OpCodes.Brfalse_S, label1),
                new CodeInstruction(OpCodes.Ldloc, gct),
                new CodeInstruction(OpCodes.Br_S, label2),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }

        [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.Awake))]
        private static class ReferenceHubAwake
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

                newInstructions.InsertRange(0, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CharacterClassManagerTargetConsolePrint), nameof(CharacterClassManagerTargetConsolePrint.GameConsoleTransmissions))),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ReferenceHub), nameof(ReferenceHub.GetComponent), generics: new Type[] { typeof(GameConsoleTransmission) })),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dictionary<ReferenceHub, GameConsoleTransmission>), nameof(Dictionary<ReferenceHub, GameConsoleTransmission>.Add))),
                });

                for (int i = 0; i < newInstructions.Count; i++)
                    yield return newInstructions[i];

                NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

                yield break;
            }
        }

        [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
        private static class ReferenceHubOnDestroy
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

                newInstructions.InsertRange(0, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CharacterClassManagerTargetConsolePrint), nameof(CharacterClassManagerTargetConsolePrint.GameConsoleTransmissions))),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dictionary<ReferenceHub, GameConsoleTransmission>), nameof(Dictionary<ReferenceHub, GameConsoleTransmission>.Remove), parameters: new Type[] { typeof(ReferenceHub) })),
                    new CodeInstruction(OpCodes.Pop),
                });

                for (int i = 0; i < newInstructions.Count; i++)
                    yield return newInstructions[i];

                NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

                yield break;
            }
        }
    }
}
