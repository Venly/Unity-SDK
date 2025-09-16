using Beamable;
using Beamable.Common;
using Beamable.Server;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Venly.Core;

internal class VyBeamMicroserviceInvoker
{
    private Type _clientType;
    private MethodInfo _entryMethod;
    private object _clientInstance;
    private BeamContext _beamContext;

    public bool IsActivated => _clientInstance != null;

    public VyBeamMicroserviceInvoker(Type clientType, MethodInfo entryMethod)
    {
        _clientType = clientType;
        _entryMethod = entryMethod;
    }

    public bool ActivateInstance(BeamContext context)
    {
        if (context == null)
            return false;

        if (IsActivated && _beamContext == context)
            return true;

        _beamContext = context;
        _clientInstance = _beamContext.Microservices().GetType().GetMethod("GetClient").MakeGenericMethod(new[] { _clientType }).Invoke(_beamContext.Microservices(), null);

        return IsActivated;
    }

    public Promise<string> Invoke<T>(T data)
    {
        var dataJson = JsonConvert.SerializeObject(data);
        return (Promise<string>)_entryMethod.Invoke(_clientInstance, new[] { dataJson });
    }
}

internal static class VyBeamUtils
{
    public static VyBeamMicroserviceInvoker FindMicroserviceClient(string clientName, string functionName, out string errMsg)
    {
        errMsg = string.Empty;

        if(string.IsNullOrEmpty(clientName))
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Microservice Name is not set.";
            return null;
        }

        if (string.IsNullOrEmpty(functionName))
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Entry Function Name is not set.";
            return null;
        }

        //Find Type
        var msType = FindMicroserviceClientType(clientName);
        if(msType == null)
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Failed to find a MicroserviceClient with name \'{clientName}\'";
            return null;
        }

        //Find MethodInfo
        var msMethod = msType.GetMethod(functionName);
        if(msMethod == null)
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Failed to find Method with name \'{functionName}\' inside \'{clientName}\' MicroserviceClient";
            return null;
        }

        //Verify Method
        if (msMethod.ReturnType != typeof(Promise<string>))
        {
            errMsg = "VyBeamUtils::FindMicroserviceClient >> Entry Function should have \'Promise<string>\' or \'Task<string>\' as return type";
            return null;
        }

        var functionParams = msMethod.GetParameters();
        if (functionParams.Length == 0)
        {
            errMsg = "VyBeamUtils::FindMicroserviceClient >> Entry Function should accept a parameter of type \'string\'";
            return null;
        }

        if (functionParams.Length > 1)
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Entry Function should only have a single parameter (\'string\'). (current length = \'{functionParams.Length}\')";
            return null;
        }

        if (functionParams[0].ParameterType != typeof(string))
        {
            errMsg = $"VyBeamUtils::FindMicroserviceClient >> Entry function's parameters should be of type \'string\' (current type = \'{functionParams[0].ParameterType.Name}\')";
            return null;
        }

        return new VyBeamMicroserviceInvoker(msType, msMethod);
    }

    public static List<Type> FindMicroserviceClientTypesInAssembly(Assembly assembly)
    {
        var msTypes = new List<Type>();

        //Iterate Types
        foreach (var assemblyType in assembly.GetTypes())
        {
            if (assemblyType.IsSubclassOf(typeof(MicroserviceClient)))
                msTypes.Add(assemblyType);
        }

        return msTypes;
    }

    public static Type FindMicroserviceClientType(string clientName)
    {
        var msName = $"{clientName}Client";

        //Search in 'Unity.Beamable.Customer.MicroserviceClients'
        try
        {
            var assembly = Assembly.Load("Unity.Beamable.Customer.MicroserviceClients");
            if (assembly != null)
            {
                var msTypes = FindMicroserviceClientTypesInAssembly(assembly);
                var match = msTypes.FirstOrDefault(t => t.Name == msName);
                if (match != null) return match;
            }
        }
        catch { }

        //Search in Main Assembly
        try
        {
            var assembly = Assembly.Load("Assembly-CSharp");
            if (assembly != null)
            {
                var msTypes = FindMicroserviceClientTypesInAssembly(assembly);
                var match = msTypes.FirstOrDefault(t => t.Name == msName);
                if (match != null) return match;
            }
        }
        catch { }

        //Search all (want to avoid)
        var allTypes = FindMicroserviceClientTypes();
        return allTypes.FirstOrDefault(t => t.Name == msName); 
    }

    public static List<Type> FindMicroserviceClientTypes()
    {
        var msTypes = new List<Type>();

        //Iterate Assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            msTypes.AddRange(FindMicroserviceClientTypesInAssembly(assembly));
        }

        return msTypes;
    }
}
