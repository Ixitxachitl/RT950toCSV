using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class ModulationChannelInfo : SerializableSection
{
    public ModulationChannelInfo() { ApplyDefaults(); }
    protected ModulationChannelInfo(SerializationInfo info, StreamingContext context)
        : base(info, context) { ApplyDefaults(); }

    public string FmFrequency          { get => ReadString("fmFreq");            set => WriteString("fmFreq", value); }
    public string FmName               { get => ReadString("fmName");            set => WriteString("fmName", value); }
    public string AmFrequency          { get => ReadString("amFreq");            set => WriteString("amFreq", value); }
    public string AmName               { get => ReadString("amName");            set => WriteString("amName", value); }
    public string SsbFrequency         { get => ReadString("ssbFreq");           set => WriteString("ssbFreq", value); }
    public string SsbBandwidth         { get => ReadString("ssbBandwidth");      set => WriteString("ssbBandwidth", value); }
    public string SsbBeatFrequencyOffset { get => ReadString("ssbBeatFreqOffset"); set => WriteString("ssbBeatFreqOffset", value); }
    public string SsbName              { get => ReadString("ssbName");           set => WriteString("ssbName", value); }

    private void ApplyDefaults()
    {
        EnsureDefault("fmFreq",           string.Empty);
        EnsureDefault("fmName",           string.Empty);
        EnsureDefault("amFreq",           string.Empty);
        EnsureDefault("amName",           string.Empty);
        EnsureDefault("ssbFreq",          string.Empty);
        EnsureDefault("ssbBandwidth",     string.Empty);
        EnsureDefault("ssbBeatFreqOffset",string.Empty);
        EnsureDefault("ssbName",          string.Empty);
    }
}
