using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class ChannelProfile : SerializableSection
{
    public ChannelProfile() { ApplyDefaults(); }

    protected ChannelProfile(SerializationInfo info, StreamingContext context)
        : base(info, context) { ApplyDefaults(); }

    public string Name           { get => ReadString("chName");       set => WriteString("chName", value); }
    public string ReceiveFrequency  { get => ReadString("rxFreq");    set => WriteString("rxFreq", value); }
    public string TransmitFrequency { get => ReadString("txFreq");    set => WriteString("txFreq", value); }
    public string ReceiveTone    { get => ReadString("rxQT", "OFF");  set => WriteString("rxQT", value, "OFF"); }
    public string TransmitTone   { get => ReadString("txQT", "OFF");  set => WriteString("txQT", value, "OFF"); }
    public int SignallingGroup   { get => ReadInt("signallingGroup"); set => WriteInt("signallingGroup", value); }
    public int PttId             { get => ReadInt("pttId");          set => WriteInt("pttId", value); }
    public int TransmitPower     { get => ReadInt("txPower");        set => WriteInt("txPower", value); }
    public int Scrambler         { get => ReadInt("scram");          set => WriteInt("scram", value); }
    public int FrequencyHopLearning { get => ReadInt("learnFHSS");   set => WriteInt("learnFHSS", value); }
    public int BandwidthSetting  { get => ReadInt("bandWide");       set => WriteInt("bandWide", value); }
    public int Encryption        { get => ReadInt("encrypt");        set => WriteInt("encrypt", value); }
    public int BusyChannelLockout { get => ReadInt("busyLockout");   set => WriteInt("busyLockout", value); }
    public int ScanList          { get => ReadInt("scanAdd");        set => WriteInt("scanAdd", value); }
    public int TransmitEnable    { get => ReadInt("enableTx", 1);    set => WriteInt("enableTx", value); }
    public int ReceiveModulation { get => ReadInt("rxModulation");   set => WriteInt("rxModulation", value); }
    public string HopCode        { get => ReadString("fhssCode");    set => WriteString("fhssCode", value); }

    private void ApplyDefaults()
    {
        EnsureDefault("chName", string.Empty);
        EnsureDefault("rxFreq", string.Empty);
        EnsureDefault("txFreq", string.Empty);
        EnsureDefault("rxQT", "OFF");
        EnsureDefault("txQT", "OFF");
        EnsureDefault("signallingGroup", 0);
        EnsureDefault("pttId", 0);
        EnsureDefault("txPower", 0);
        EnsureDefault("scram", 0);
        EnsureDefault("learnFHSS", 0);
        EnsureDefault("bandWide", 0);
        EnsureDefault("encrypt", 0);
        EnsureDefault("busyLockout", 0);
        EnsureDefault("scanAdd", 0);
        EnsureDefault("enableTx", 1);
        EnsureDefault("rxModulation", 0);
        EnsureDefault("fhssCode", string.Empty);
    }
}
