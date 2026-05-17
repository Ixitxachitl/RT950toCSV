using System;
using System.Globalization;
using CsvHelper.Configuration.Attributes;
using RT950toCSV.Data.Codeplug;

namespace RT950toCSV.App;

/// <summary>
/// Represents one row from a CHIRP-exported CSV file and knows how to map it
/// to/from an RT950 <see cref="ChannelRecord"/>.
/// </summary>
public class ChirpRecord
{
    [Name("Location")]     public int    Location     { get; set; }
    [Name("Name")]         public string Name         { get; set; } = "";
    [Name("Frequency")]    public string Frequency    { get; set; } = "";
    [Name("Duplex")]       public string Duplex       { get; set; } = "";
    [Name("Offset")]       public string Offset       { get; set; } = "";
    [Name("Tone")]         public string ToneMode     { get; set; } = "";
    [Name("rToneFreq")]    public string TxToneFreq   { get; set; } = "";
    [Name("cToneFreq")]    public string RxToneFreq   { get; set; } = "";
    [Name("DtcsCode")]     public string DtcsCode     { get; set; } = "";
    [Name("DtcsPolarity")] public string DtcsPolarity { get; set; } = "";
    [Name("RxDtcsCode")]   public string RxDtcsCode   { get; set; } = "";
    [Name("CrossMode")]    public string CrossMode    { get; set; } = "";
    [Name("Mode")]         public string Mode         { get; set; } = "";
    [Name("TStep")]        public string TStep        { get; set; } = "5.00";
    [Name("Skip")]         public string Skip         { get; set; } = "";
    [Name("Power")]        public string Power        { get; set; } = "";
    [Name("Comment")]      public string Comment      { get; set; } = "";
    [Name("URCALL")]       public string UrcCall      { get; set; } = "";
    [Name("RPT1CALL")]     public string Rpt1Call     { get; set; } = "";
    [Name("RPT2CALL")]     public string Rpt2Call     { get; set; } = "";
    [Name("DVCODE")]       public string DvCode       { get; set; } = "";

    // CHIRP Location (1-based) → RT950 Zone/Slot
    public int ToZone() => ((Location - 1) / RadioCodeplug.SlotsPerZone) + 1;
    public int ToSlot() => ((Location - 1) % RadioCodeplug.SlotsPerZone) + 1;

    public ChannelRecord ToChannelRecord()
    {
        var rxStr = FormatFreq(Frequency);
        var txStr = CalcTxFreq();
        var (txQt, rxQt) = MapTones();

        return new ChannelRecord
        {
            Zone         = ToZone(),
            ZoneName     = $"Zone {ToZone()}",
            Slot         = ToSlot(),
            ChName       = Name,
            RxFreq       = rxStr,
            TxFreq       = txStr,
            TxQT         = txQt,
            RxQT         = rxQt,
            BandWide     = Mode == "FM" ? 1 : 0,                           // FM=wide, NFM/AM=narrow
            EnableTx     = Duplex.Equals("off", StringComparison.OrdinalIgnoreCase) ? 0 : 1,
            ScanAdd      = string.IsNullOrEmpty(Skip) ? 1 : 0,
            TxPower      = MapPower()
        };
    }

    // ── Frequency helpers ────────────────────────────────────────────────────

    private string CalcTxFreq()
    {
        if (!decimal.TryParse(Frequency, NumberStyles.Any, CultureInfo.InvariantCulture, out var rx))
            return Frequency;
        if (!decimal.TryParse(Offset, NumberStyles.Any, CultureInfo.InvariantCulture, out var off))
            off = 0m;

        var tx = Duplex switch
        {
            "+" => rx + off,
            "-" => rx - off,
            _   => rx           // simplex or "off"
        };
        return FormatFreq(tx);
    }

    private static string FormatFreq(string freq)
    {
        if (!decimal.TryParse(freq, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
            return freq;
        return FormatFreq(f);
    }

    private static string FormatFreq(decimal freq) =>
        freq.ToString("0.00000", CultureInfo.InvariantCulture);

    // ── Tone helpers ─────────────────────────────────────────────────────────

    private (string tx, string rx) MapTones() =>
        (ToneMode?.Trim() ?? "") switch
        {
            "Tone"  => (Ctcss(TxToneFreq), "OFF"),
            "TSQL"  => (Ctcss(TxToneFreq), Ctcss(RxToneFreq)),
            "DTCS"  => (Dcs(DtcsCode, DtcsPolarity, txSide: true),
                        Dcs(RxDtcsCode, DtcsPolarity, txSide: false)),
            "Cross" => MapCross(),
            _       => ("OFF", "OFF")
        };

    private (string tx, string rx) MapCross()
    {
        if (string.IsNullOrEmpty(CrossMode)) return ("OFF", "OFF");
        var parts = CrossMode.Split("->", 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return ("OFF", "OFF");

        var tx = parts[0] == "Tone"   ? Ctcss(TxToneFreq)
               : parts[0] == "DTCS"  ? Dcs(DtcsCode, DtcsPolarity, txSide: true)
               : "OFF";
        var rx = parts[1] == "Tone"   ? Ctcss(RxToneFreq)
               : parts[1] == "DTCS"  ? Dcs(RxDtcsCode, DtcsPolarity, txSide: false)
               : "OFF";
        return (tx, rx);
    }

    private static string Ctcss(string tone)
    {
        if (string.IsNullOrWhiteSpace(tone)) return "OFF";
        if (decimal.TryParse(tone, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
            return f.ToString("0.0", CultureInfo.InvariantCulture);
        return "OFF";
    }

    /// <summary>DCS code in RT950 format, e.g. "D023N".</summary>
    private static string Dcs(string code, string polarity, bool txSide)
    {
        if (string.IsNullOrWhiteSpace(code)) return "OFF";
        // Polarity string is two chars e.g. "NN", "RN"; first char = TX side, second = RX side
        var pol = txSide
            ? (polarity?.Length >= 1 && polarity[0] == 'R' ? "I" : "N")
            : (polarity?.Length >= 2 && polarity[1] == 'R' ? "I" : "N");
        return $"D{code}{pol}";
    }

    // ── Power ────────────────────────────────────────────────────────────────

    private int MapPower()
    {
        if (string.IsNullOrWhiteSpace(Power)) return 0;
        var p = Power.Trim().ToUpperInvariant();
        if (p is "HIGH" or "H") return 2;
        if (p is "MED" or "MEDIUM" or "M") return 1;
        if (p is "LOW" or "L") return 0;
        // Parse wattage like "4.0W", "2.5W"
        if (decimal.TryParse(p.TrimEnd('W'), NumberStyles.Any, CultureInfo.InvariantCulture, out var w))
        {
            if (w >= 4m) return 2;
            if (w >= 2m) return 1;
        }
        return 0;
    }

    // ── Export: RT950 ChannelRecord → ChirpRecord ─────────────────────────

    public static ChirpRecord FromChannelRecord(ChannelRecord r, int location)
    {
        var (toneMode, rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, crossMode) =
            ExportTones(r.TxQT, r.RxQT);
        var (duplex, offset) = ExportDuplex(r.RxFreq, r.TxFreq, r.EnableTx);

        return new ChirpRecord
        {
            Location     = location,
            Name         = r.ChName,
            Frequency    = NormalizeFreq(r.RxFreq),
            Duplex       = duplex,
            Offset       = offset,
            ToneMode     = toneMode,
            TxToneFreq   = rToneFreq,
            RxToneFreq   = cToneFreq,
            DtcsCode     = dtcsCode,
            DtcsPolarity = dtcsPol,
            RxDtcsCode   = rxDtcsCode,
            CrossMode    = crossMode,
            Mode         = r.BandWide == 1 ? "FM" : "NFM",
            TStep        = "5.00",
            Skip         = r.ScanAdd == 1 ? "" : "S",
            Power        = r.TxPower switch { 2 => "4.0W", 1 => "2.0W", _ => "1.0W" }
        };
    }

    private static string NormalizeFreq(string freq)
    {
        if (decimal.TryParse(freq, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
            return f.ToString("0.000000", CultureInfo.InvariantCulture);
        return freq;
    }

    private static (string duplex, string offset) ExportDuplex(string rxFreq, string txFreq, int enableTx)
    {
        if (enableTx == 0) return ("off", "0.000000");
        if (!decimal.TryParse(rxFreq, NumberStyles.Any, CultureInfo.InvariantCulture, out var rx) ||
            !decimal.TryParse(txFreq, NumberStyles.Any, CultureInfo.InvariantCulture, out var tx))
            return ("", "0.000000");
        if (tx == rx) return ("", "0.000000");
        if (tx > rx)  return ("+", (tx - rx).ToString("0.000000", CultureInfo.InvariantCulture));
        return ("-", (rx - tx).ToString("0.000000", CultureInfo.InvariantCulture));
    }

    // RT950 DCS format: "D023N" (normal) or "D023I" (inverted)
    private static bool ExportIsDcs(string t)   => !string.IsNullOrEmpty(t) && t.Length >= 4 && t[0] == 'D' && char.IsDigit(t[1]);
    private static bool ExportIsCtcss(string t) => !string.IsNullOrEmpty(t) && t != "OFF" && !ExportIsDcs(t);
    private static string ExportDcsCode(string t)      => t.Length >= 4 ? t[1..4] : "023";
    private static bool   ExportDcsInverted(string t)  => t.EndsWith("I", StringComparison.Ordinal);

    private static (string toneMode, string rToneFreq, string cToneFreq,
                    string dtcsCode, string dtcsPol, string rxDtcsCode, string crossMode)
        ExportTones(string txQt, string rxQt)
    {
        var txOff   = string.IsNullOrEmpty(txQt) || txQt == "OFF";
        var rxOff   = string.IsNullOrEmpty(rxQt) || rxQt == "OFF";
        var txCtcss = !txOff && ExportIsCtcss(txQt);
        var rxCtcss = !rxOff && ExportIsCtcss(rxQt);
        var txDcs   = !txOff && ExportIsDcs(txQt);
        var rxDcs   = !rxOff && ExportIsDcs(rxQt);

        var rToneFreq  = txCtcss ? txQt : "88.5";
        var cToneFreq  = rxCtcss ? rxQt : "88.5";
        var dtcsCode   = txDcs   ? ExportDcsCode(txQt) : rxDcs ? ExportDcsCode(rxQt) : "023";
        var rxDtcsCode = rxDcs   ? ExportDcsCode(rxQt) : "023";
        var txPol      = txDcs && ExportDcsInverted(txQt) ? "R" : "N";
        var rxPol      = rxDcs && ExportDcsInverted(rxQt) ? "R" : "N";
        var dtcsPol    = txPol + rxPol;

        if (txOff   && rxOff)   return ("",      rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "Tone->Tone");
        if (txCtcss && rxOff)   return ("Tone",  rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "Tone->Tone");
        if (txCtcss && rxCtcss) return ("TSQL",  rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "Tone->Tone");
        if (txDcs   && rxDcs)   return ("DTCS",  rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "DTCS->DTCS");
        if (txCtcss && rxDcs)   return ("Cross", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "Tone->DTCS");
        if (txDcs   && rxCtcss) return ("Cross", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "DTCS->Tone");
        if (txOff   && rxCtcss) return ("Cross", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "->Tone");
        if (txOff   && rxDcs)   return ("Cross", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "->DTCS");
        if (txDcs   && rxOff)   return ("Cross", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "DTCS->");
        return ("", rToneFreq, cToneFreq, dtcsCode, dtcsPol, rxDtcsCode, "Tone->Tone");
    }
}
