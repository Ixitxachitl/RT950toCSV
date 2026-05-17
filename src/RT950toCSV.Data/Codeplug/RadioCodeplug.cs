using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class RadioCodeplug : SerializableSection
{
    public const int ZoneCount    = 15;
    public const int SlotsPerZone = 64;
    public const int ChannelSlots = ZoneCount * SlotsPerZone;

    public static readonly string[] DefaultZoneNames =
    {
        "Zone 1",  "Zone 2",  "Zone 3",  "Zone 4",  "Zone 5",
        "Zone 6",  "Zone 7",  "Zone 8",  "Zone 9",  "Zone 10",
        "Zone 11", "Zone 12", "Zone 13", "Zone 14", "Zone 15"
    };

    public RadioCodeplug()
    {
        var channelData = new ChannelDataSection();
        Values["channelData"]    = channelData;
        Frequency  = new FrequencyModeSection();  Values["freqModeData"]    = Frequency;
        Functions  = new FunctionConfigSection(); Values["funConfigData"]   = Functions;
        Dtmf       = new DtmfSection();           Values["dtmfData"]        = Dtmf;
        Modulation = new ModulationSection();     Values["modulationData"]  = Modulation;
        Aprs       = new AprsSection();           Values["aprsData"]        = Aprs;
    }

    protected RadioCodeplug(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ReadObject("channelData", () => new ChannelDataSection());
        Frequency  = ReadObject("freqModeData", () => new FrequencyModeSection());
        Functions  = ReadObject("funConfigData",() => new FunctionConfigSection());
        Dtmf       = ReadObject("dtmfData",     () => new DtmfSection());
        Modulation = ReadObject("modulationData",() => new ModulationSection());
        Aprs       = ReadObject("aprsData",     () => new AprsSection());
    }

    private ChannelDataSection ChannelData =>
        ReadObject<ChannelDataSection>("channelData", () => new ChannelDataSection());

    public ChannelProfile[]      Channels   => ChannelData.Channels;
    public FrequencyModeSection  Frequency  { get; }
    public FunctionConfigSection Functions  { get; }
    public DtmfSection           Dtmf       { get; }
    public ModulationSection     Modulation { get; }
    public AprsSection           Aprs       { get; }

    public string[] ZoneNames => ChannelData.ZoneNames;

    public void SetZoneName(int zoneIndex, string name)
    {
        if (zoneIndex >= 0 && zoneIndex < ZoneCount)
            ChannelData.ZoneNames[zoneIndex] = name;
    }
}
