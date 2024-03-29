﻿// -----------------------------------------------------------------------
// <copyright file="DebugPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Mistaken.Fixes.Patch
{
    internal static class DebugPatch
    {
        private static int _instructionCounter = 0;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var fld = AccessTools.Field(typeof(DebugPatch), nameof(_instructionCounter));

            yield return new(OpCodes.Ldc_I4_M1);
            yield return new(OpCodes.Stsfld, fld);

            int x = 0;

            foreach (var item in instructions)
            {
                Log.Info($"[{x}] {item}");
                yield return item;
                yield return new(OpCodes.Ldc_I4, x++);
                yield return new(OpCodes.Stsfld, fld);
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Log.Error($"Exception on instruction {_instructionCounter}");
                Log.Error(__exception.Message);
                Log.Error(__exception.StackTrace);
            }

            return null;
        }
    }
}