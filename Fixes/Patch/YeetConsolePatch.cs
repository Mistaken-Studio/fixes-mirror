// -----------------------------------------------------------------------
// <copyright file="YeetConsolePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MEC;
using Mirror;
using Mistaken.API;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.TargetConsolePrint))]
    internal static class YeetConsolePatch
    {
        public static IEnumerator<float> UpdateConsolePrint()
        {
            ConsoleMessages.Clear();
            int rid = RoundPlus.RoundId;
            while (rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(0.5f);
                foreach (var message in ConsoleMessages.ToArray())
                {
                    if (message.Connection is null || message.Transmission == null)
                    {
                        ConsoleMessages.Remove(message);
                        continue;
                    }

                    message.Transmission.SendToClient(message.Connection, message.Text, message.Color);
                    ConsoleMessages.Remove(message);
                }
            }
        }

        private static List<Message> ConsoleMessages { get; set; } = new List<Message>();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ldarg_3) + 1;

            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Message), new Type[]
                {
                    typeof(GameConsoleTransmission), typeof(NetworkConnection), typeof(string), typeof(string),
                })),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(YeetConsolePatch), nameof(YeetConsolePatch.Add))),
            });

            for (int i = 0; i < newInstructions.Count; i++)
                yield return newInstructions[i];

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);

            yield break;
        }

        private static void Add(Message msg) => ConsoleMessages.Add(msg);

        private struct Message
        {
            public Message(GameConsoleTransmission transmission, NetworkConnection connection, string text, string color)
            {
                this.Transmission = transmission;
                this.Connection = connection;
                this.Text = text;
                this.Color = color;
            }

            public GameConsoleTransmission Transmission { get; set; }

            public NetworkConnection Connection { get; set; }

            public string Text { get; set; }

            public string Color { get; set; }
        }
    }
}
