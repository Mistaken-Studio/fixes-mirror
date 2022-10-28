// -----------------------------------------------------------------------
// <copyright file="NetworkConnectionIdentityPatches.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Disarming;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;

#pragma warning disable SA1118 // Parameters should span multiple lines

namespace Mistaken.Fixes.Patch
{
    [HarmonyPatch]
    internal static class NetworkConnectionIdentityPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerRequestReceived));
            yield return AccessTools.Method(typeof(AttachmentsServerHandler), nameof(AttachmentsServerHandler.ServerReceiveChangeRequest));
            yield return AccessTools.Method(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived));
            yield return AccessTools.Method(typeof(AttachmentsServerHandler), nameof(AttachmentsServerHandler.ServerReceivePreference));
            yield return AccessTools.Method(typeof(DisarmingHandlers), nameof(DisarmingHandlers.ServerProcessDisarmMessage));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label returnLabel = generator.DefineLabel();

            newInstructions[newInstructions.Count - 1].WithLabels(returnLabel);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(NetworkConnection), nameof(NetworkConnection.identity))),
                new(OpCodes.Ldnull),
                new(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality")),
                new(OpCodes.Brtrue_S, returnLabel),
            });

            foreach (var instruction in newInstructions)
                yield return instruction;

            NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}