﻿using AutoDI;
using AutoDI.Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

// ReSharper disable once CheckNamespace
partial class ModuleWeaver
{
    private void InjectInitCall(MethodReference initMethod)
    {
        if (ModuleDefinition.EntryPoint != null)
        {
            InternalLogDebug("Injecting AutoDI init call", DebugLogLevel.Verbose);

            var injector = new Injector(ModuleDefinition.EntryPoint);
            injector.Insert(OpCodes.Ldnull);
            injector.Insert(OpCodes.Call, initMethod);
        }
        else
        {
            InternalLogDebug($"No entry point in {ModuleDefinition.FileName}. Skipping container injection.", DebugLogLevel.Default);
        }
    }
}