// -----------------------------------------------------------------------
// <copyright file="YeetConsolePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MEC;
using Mirror;
using Mistaken.API;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.TargetConsolePrint))]
    internal static class YeetConsolePatch
    {
        public static IEnumerator<float> UpdateConsolePrint()
        {
            _consoleMessages.Clear();
            int rid = RoundPlus.RoundId;

            while (rid == RoundPlus.RoundId)
            {
                yield return Timing.WaitForSeconds(0.5f);

                foreach (var message in _consoleMessages.ToArray())
                {
                    if (message.Connection is null || message.Transmission == null)
                    {
                        _consoleMessages.Remove(message);
                        continue;
                    }

                    message.Transmission.SendToClient(message.Connection, message.Text, message.Color);
                    _consoleMessages.Remove(message);
                }
            }
        }

        private static readonly List<Message> _consoleMessages = new();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex((CodeInstruction x) => x.opcode == OpCodes.Ldarg_3) + 1;

            newInstructions.RemoveAt(index);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Newobj, AccessTools.Constructor(typeof(Message), new[]
                {
                    typeof(GameConsoleTransmission), typeof(NetworkConnection), typeof(string), typeof(string),
                })),
                new(OpCodes.Call, AccessTools.Method(typeof(YeetConsolePatch), nameof(YeetConsolePatch.Add))),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static void Add(Message msg) => _consoleMessages.Add(msg);

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
