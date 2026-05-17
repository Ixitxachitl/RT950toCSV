using RT950toCSV.Data.Codeplug;

namespace RT950toCSV.App;

public class ChannelRecord
{
    public int    Zone            { get; set; }
    public string ZoneName        { get; set; } = string.Empty;
    public int    Slot            { get; set; }
    public string ChName          { get; set; } = string.Empty;
    public string RxFreq          { get; set; } = string.Empty;
    public string TxFreq          { get; set; } = string.Empty;
    public string RxQT            { get; set; } = string.Empty;
    public string TxQT            { get; set; } = string.Empty;
    public int    SignallingGroup  { get; set; }
    public int    PttId           { get; set; }
    public int    TxPower         { get; set; }
    public int    Scram           { get; set; }
    public int    LearnFHSS       { get; set; }
    public int    BandWide        { get; set; }
    public int    Encrypt         { get; set; }
    public int    BusyLockout     { get; set; }
    public int    ScanAdd         { get; set; }
    public int    EnableTx        { get; set; }
    public int    RxModulation    { get; set; }
    public string FhssCode        { get; set; } = string.Empty;

    public static ChannelRecord FromChannel(ChannelProfile channel, int zone, int slot)
    {
        return new ChannelRecord
        {
            Zone           = zone,
            ZoneName       = zone >= 1 && zone <= RadioCodeplug.DefaultZoneNames.Length
                                ? RadioCodeplug.DefaultZoneNames[zone - 1]
                                : string.Empty,
            Slot           = slot,
            ChName         = channel.Name,
            RxFreq         = channel.ReceiveFrequency,
            TxFreq         = channel.TransmitFrequency,
            RxQT           = channel.ReceiveTone,
            TxQT           = channel.TransmitTone,
            SignallingGroup = channel.SignallingGroup,
            PttId          = channel.PttId,
            TxPower        = channel.TransmitPower,
            Scram          = channel.Scrambler,
            LearnFHSS      = channel.FrequencyHopLearning,
            BandWide       = channel.BandwidthSetting,
            Encrypt        = channel.Encryption,
            BusyLockout    = channel.BusyChannelLockout,
            ScanAdd        = channel.ScanList,
            EnableTx       = channel.TransmitEnable,
            RxModulation   = channel.ReceiveModulation,
            FhssCode       = channel.HopCode
        };
    }

    public void ApplyToChannel(ChannelProfile channel)
    {
        channel.Name                 = ChName    ?? string.Empty;
        channel.ReceiveFrequency     = RxFreq    ?? string.Empty;
        channel.TransmitFrequency    = TxFreq    ?? string.Empty;
        channel.ReceiveTone          = string.IsNullOrWhiteSpace(RxQT) ? "OFF" : RxQT;
        channel.TransmitTone         = string.IsNullOrWhiteSpace(TxQT) ? "OFF" : TxQT;
        channel.SignallingGroup      = SignallingGroup;
        channel.PttId                = PttId;
        channel.TransmitPower        = TxPower;
        channel.Scrambler            = Scram;
        channel.FrequencyHopLearning = LearnFHSS;
        channel.BandwidthSetting     = BandWide;
        channel.Encryption           = Encrypt;
        channel.BusyChannelLockout   = BusyLockout;
        channel.ScanList             = ScanAdd;
        channel.TransmitEnable       = EnableTx;
        channel.ReceiveModulation    = RxModulation;
        channel.HopCode              = FhssCode   ?? string.Empty;
    }
}
