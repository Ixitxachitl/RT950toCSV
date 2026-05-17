using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class VfoSettings : SerializableSection
{
    public VfoSettings() { ApplyDefaults(); }
    protected VfoSettings(SerializationInfo info, StreamingContext context)
        : base(info, context) { ApplyDefaults(); }

    public string ReceiveFrequency { get => ReadString("tB_RxFreq", "400.12500"); set => WriteString("tB_RxFreq", value, "400.12500"); }
    public string ReceiveTone      { get => ReadString("cbB_RxQT", "OFF");        set => WriteString("cbB_RxQT", value, "OFF"); }
    public string TransmitTone     { get => ReadString("cbB_TxQT", "OFF");        set => WriteString("cbB_TxQT", value, "OFF"); }
    public int BusyLockout         { get => ReadInt("cbB_BusyLockout", 1);        set => WriteInt("cbB_BusyLockout", value); }
    public int OffsetDirection     { get => ReadInt("cbB_OffsetDir");             set => WriteInt("cbB_OffsetDir", value); }
    public int SignallingGroup     { get => ReadInt("cbB_SignallingGroup");        set => WriteInt("cbB_SignallingGroup", value); }
    public int TransmitPower       { get => ReadInt("cbB_TxPower");               set => WriteInt("cbB_TxPower", value); }
    public string Offset           { get => ReadString("tB_OffsetFreq", "000.0000"); set => WriteString("tB_OffsetFreq", value, "000.0000"); }

    private void ApplyDefaults()
    {
        EnsureDefault("tB_RxFreq",           "400.12500");
        EnsureDefault("cbB_RxQT",            "OFF");
        EnsureDefault("cbB_TxQT",            "OFF");
        EnsureDefault("cbB_FrVFOByte12",     0);
        EnsureDefault("cbB_BusyLockout",     1);
        EnsureDefault("cbB_OffsetDir",       0);
        EnsureDefault("cbB_SignallingGroup", 0);
        EnsureDefault("cbB_FrVFOByte15",     0);
        EnsureDefault("cbB_TxPower",         0);
        EnsureDefault("cbB_Scram",           0);
        EnsureDefault("cbB_LearnFHSS",       0);
        EnsureDefault("cbB_BandWide",        0);
        EnsureDefault("cbB_Encrypt",         0);
        EnsureDefault("cbB_RxModulation",    0);
        EnsureDefault("cbB_FreqBand",        1);
        EnsureDefault("cbB_StepFreq",        0);
        EnsureDefault("tB_OffsetFreq",       "000.0000");
        EnsureDefault("cbB_FrVFOByte27to31", 0);
    }
}
