using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RT950toCSV.Data.Codeplug;

namespace RT950toCSV.App.Serialization;

public sealed class KdhSerializationBinder : SerializationBinder
{
    private const string TargetAssembly = "BT-RT950PRO_CPS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

    // Deserialization: KDH type name → our CLR type.
    // KDH.ChannelData is the container object (channelList + arrayZoneName).
    // KDH.Channel     is an individual channel's data record.
    private static readonly Dictionary<string, Type> TypeMap = new(StringComparer.Ordinal)
    {
        ["KDH.RadioData"]           = typeof(RadioCodeplug),
        ["KDH.ChannelData"]         = typeof(ChannelDataSection),   // container wrapper
        ["KDH.Channel"]             = typeof(ChannelProfile),        // individual channel
        ["KDH.Channel[]"]           = typeof(ChannelProfile[]),
        ["KDH.FreqModeData"]        = typeof(FrequencyModeSection),
        ["KDH.VFOData"]             = typeof(VfoSettings),
        ["KDH.FunConfigData"]       = typeof(FunctionConfigSection),
        ["KDH.DTMFData"]            = typeof(DtmfSection),
        ["KDH.ModulationData"]      = typeof(ModulationSection),
        ["KDH.ModulationChannel"]   = typeof(ModulationChannelInfo),
        ["KDH.ModulationChannel[]"] = typeof(ModulationChannelInfo[]),
        ["KDH.APRSData"]            = typeof(AprsSection)
    };

    // Serialization: our CLR type → KDH type name.
    private static readonly Dictionary<Type, (string Assembly, string Type)> ReverseMap = new()
    {
        [typeof(RadioCodeplug)]          = (TargetAssembly, "KDH.RadioData"),
        [typeof(ChannelDataSection)]     = (TargetAssembly, "KDH.ChannelData"),
        [typeof(ChannelProfile)]         = (TargetAssembly, "KDH.Channel"),
        [typeof(ChannelProfile[])]       = (TargetAssembly, "KDH.Channel[]"),
        [typeof(FrequencyModeSection)]   = (TargetAssembly, "KDH.FreqModeData"),
        [typeof(VfoSettings)]            = (TargetAssembly, "KDH.VFOData"),
        [typeof(FunctionConfigSection)]  = (TargetAssembly, "KDH.FunConfigData"),
        [typeof(DtmfSection)]            = (TargetAssembly, "KDH.DTMFData"),
        [typeof(ModulationSection)]      = (TargetAssembly, "KDH.ModulationData"),
        [typeof(ModulationChannelInfo)]  = (TargetAssembly, "KDH.ModulationChannel"),
        [typeof(ModulationChannelInfo[])]= (TargetAssembly, "KDH.ModulationChannel[]"),
        [typeof(AprsSection)]            = (TargetAssembly, "KDH.APRSData")
    };

    public override Type BindToType(string? assemblyName, string? typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            throw new SerializationException("Type name was not supplied.");
        if (TypeMap.TryGetValue(typeName, out var mapped))
            return mapped;

        // Handle array types whose element is in the map.
        if (typeName.EndsWith("[]", StringComparison.Ordinal))
        {
            var elem = typeName[..^2];
            if (TypeMap.TryGetValue(elem, out var elemType))
                return elemType.MakeArrayType();
        }

        throw new SerializationException($"Unsupported type '{typeName}'.");
    }

    public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        if (ReverseMap.TryGetValue(serializedType, out var mapping))
        {
            assemblyName = mapping.Assembly;
            typeName     = mapping.Type;
            return;
        }

        // .NET 9: BindToName is not called for array types in member-type-info records.
        // This fallback handles the rare cases where it IS called with an array type.
        if (serializedType.IsArray && serializedType.GetArrayRank() == 1)
        {
            var elem = serializedType.GetElementType()!;
            if (ReverseMap.TryGetValue(elem, out var elemMapping))
            {
                assemblyName = elemMapping.Assembly;
                typeName     = elemMapping.Type + "[]";
                return;
            }
        }

        assemblyName = serializedType.Assembly.FullName;
        typeName     = serializedType.FullName;
    }
}
