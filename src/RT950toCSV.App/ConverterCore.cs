using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RT950toCSV.App.Serialization;
using RT950toCSV.Data.Codeplug;

namespace RT950toCSV.App;

#pragma warning disable SYSLIB0011
internal static class ConverterCore
{
    internal const int ZoneCount       = RadioCodeplug.ZoneCount;
    internal const int ChannelsPerZone = RadioCodeplug.SlotsPerZone;

    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        TrimOptions       = TrimOptions.Trim,
        NewLine           = Environment.NewLine,
        MissingFieldFound = null,
        HeaderValidated   = null
    };

    private static readonly BinaryFormatter Formatter;

    static ConverterCore()
    {
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        Formatter = new BinaryFormatter { Binder = new KdhSerializationBinder() };
    }

    internal static int ExportToChirpCsv(string inputPath, string outputPath)
    {
        var radio   = LoadRadioData(inputPath);
        var records = EnumerateRecords(radio)
            .Where(r => !string.IsNullOrWhiteSpace(r.RxFreq))
            .ToList();

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? Directory.GetCurrentDirectory());

        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(false));
        using var csv    = new CsvWriter(writer, CsvConfig);
        csv.WriteHeader<ChirpRecord>();
        csv.NextRecord();
        foreach (var record in records)
        {
            var location = (record.Zone - 1) * ChannelsPerZone + record.Slot;
            csv.WriteRecord(ChirpRecord.FromChannelRecord(record, location));
            csv.NextRecord();
        }
        return records.Count;
    }

    private static IEnumerable<ChannelRecord> EnumerateRecords(RadioCodeplug radio)
    {
        for (var i = 0; i < radio.Channels.Length; i++)
        {
            var zone = (i / ChannelsPerZone) + 1;
            var slot = (i % ChannelsPerZone) + 1;
            yield return ChannelRecord.FromChannel(radio.Channels[i], zone, slot);
        }
    }

    internal static RadioCodeplug LoadRadioData(string path)
    {
        using var stream = File.OpenRead(path);
        return (RadioCodeplug)Formatter.Deserialize(stream);
    }

    internal static void SaveRadioData(string path, RadioCodeplug radio)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path)) ?? Directory.GetCurrentDirectory());

        // BinaryFormatter does not call BindToName for array types used in member-type-info
        // records (ClassWithMembersAndTypes). Those are written with the raw CLR name.
        // We patch the two affected strings after serialization.
        using var ms = new MemoryStream();
        Formatter.Serialize(ms, radio);
        var data = PatchNrbfStrings(ms.ToArray());
        File.WriteAllBytes(path, data);
    }

    // Pairs of (CLR type name, KDH type name) that appear in member-type-info records.
    private static readonly (string From, string To)[] TypeNamePatches =
    [
        // channelList inside KDH.ChannelData: CLR array name → KDH.Channel[]
        ("RT950toCSV.Data.Codeplug.ChannelProfile[]",        "KDH.Channel[]"),
        // modulationChannels inside KDH.ModulationData: CLR array name → KDH.ModulationChannel[]
        ("RT950toCSV.Data.Codeplug.ModulationChannelInfo[]", "KDH.ModulationChannel[]"),
    ];

    /// <summary>
    /// Replaces LEB128-prefixed NRBF strings in the serialized stream.
    /// Each string in the NRBF format is preceded by a LEB128-encoded length.
    /// </summary>
    private static byte[] PatchNrbfStrings(byte[] data)
    {
        foreach (var (from, to) in TypeNamePatches)
        {
            var fromBytes = BuildNrbfString(from);
            var toBytes   = BuildNrbfString(to);
            data = ReplaceOnce(data, fromBytes, toBytes);
        }
        return data;
    }

    private static byte[] BuildNrbfString(string s)
    {
        var str = Encoding.UTF8.GetBytes(s);
        // LEB128 encode the byte length
        var leb = new List<byte>();
        var n   = str.Length;
        do
        {
            var b = (byte)(n & 0x7F);
            n >>= 7;
            if (n > 0) b |= 0x80;
            leb.Add(b);
        } while (n > 0);
        return [.. leb, .. str];
    }

    private static byte[] ReplaceOnce(byte[] data, byte[] from, byte[] to)
    {
        var span = data.AsSpan();
        for (var i = 0; i <= data.Length - from.Length; i++)
        {
            if (span.Slice(i, from.Length).SequenceEqual(from))
            {
                var result = new byte[data.Length - from.Length + to.Length];
                span[..i].CopyTo(result);
                to.CopyTo(result, i);
                span[(i + from.Length)..].CopyTo(result.AsSpan(i + to.Length));
                return result;
            }
        }
        return data; // not found — leave unchanged
    }

    // ── CHIRP CSV support ────────────────────────────────────────────────────

    internal static (int Imported, int Skipped) ImportFromChirpCsv(
        string csvPath, string outputPath, string templatePath)
    {
        var radio    = LoadRadioData(templatePath);
        var rows     = ReadChirpRecords(csvPath);
        var imported = 0;
        var skipped  = 0;

        foreach (var row in rows)
        {
            var zone = row.ToZone();
            var slot = row.ToSlot();

            if (zone < 1 || zone > RadioCodeplug.ZoneCount ||
                slot < 1 || slot > RadioCodeplug.SlotsPerZone)
            {
                skipped++;
                continue;
            }

            var index = (zone - 1) * ChannelsPerZone + (slot - 1);
            row.ToChannelRecord().ApplyToChannel(radio.Channels[index]);
            imported++;
        }

        SaveRadioData(outputPath, radio);
        return (imported, skipped);
    }

    internal static List<ChirpRecord> ReadChirpRecords(string csvPath)
    {
        using var reader = new StreamReader(csvPath, Encoding.UTF8, true);
        using var csv    = new CsvReader(reader, CsvConfig);
        return csv.GetRecords<ChirpRecord>().ToList();
    }
}
#pragma warning restore SYSLIB0011
