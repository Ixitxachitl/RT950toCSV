# RT950toCSV

A Windows desktop utility for converting BT-RT950 Pro radio codeplug files (`.dat`) to and from [CHIRP](https://chirp.danplanet.com/)-compatible CSV files.

## Overview

The BT-RT950 Pro is a handheld radio programmed via a proprietary CPS (Customer Programming Software) that saves its codeplug as a binary `.dat` file. This tool bridges that format with CHIRP, the popular open-source radio programming application, allowing you to:

- **Export** a `Radio.dat` codeplug → CHIRP CSV (for editing channels in CHIRP or any spreadsheet)
- **Import** a CHIRP CSV → a new `Radio.dat` (ready to load back into the CPS)

## Features

- Reads and writes the RT950's binary NRBF-serialized `.dat` format
- Produces CSV files compatible with CHIRP's import/import workflow
- Maps all key channel fields: RX/TX frequencies, CTCSS/DCS tones, bandwidth, power, scan list membership, and more
- Supports the full channel capacity: **15 zones × 64 channels = 960 channel slots**
- Template-based import: channel data is merged into an existing `Radio.dat` so that VFO settings, DTMF codes, scan lists, and all other radio settings are preserved

## Requirements

- Windows
- [.NET 9](https://dotnet.microsoft.com/download/dotnet/9) runtime (or SDK to build from source)

## Usage

### Export: DAT → CHIRP CSV

1. Launch the application.
2. In the **Export** panel, click **Browse…** and select your `Radio.dat` file.
3. Click **Save CSV…** and choose a destination for the output CSV.
4. Click **Export DAT → CSV**.

The output CSV is written in CHIRP's standard column format and can be opened directly in CHIRP via *File → Import*.

### Import: CHIRP CSV → DAT

1. In the **Import** panel, click **Browse…** next to *CHIRP CSV* and select your edited CSV file.
2. Click **Browse…** next to *Template .dat* and select the original `Radio.dat` from your radio. This file provides all radio settings (VFO, DTMF, scan lists, etc.) that are not stored in the CSV.
3. Click **Save .dat…** and choose an output path for the new codeplug.
4. Click **Import CSV → DAT**.

> **Note:** Rows whose `Location` value falls outside the supported channel range (1–960) are skipped and reported in the log.

## Channel Field Mapping

| CHIRP Field   | RT950 Field          | Notes                                      |
|---------------|----------------------|--------------------------------------------|
| `Location`    | Zone / Slot          | `Location = (Zone-1)*64 + Slot`            |
| `Name`        | Channel name         |                                            |
| `Frequency`   | RX frequency         |                                            |
| `Duplex/Offset` | TX frequency       | Calculates TX from RX ± offset             |
| `Tone/rToneFreq/cToneFreq` | TX/RX CTCSS | Maps CHIRP tone modes to RT950 tone strings |
| `DtcsCode`    | TX/RX DCS code       |                                            |
| `Mode`        | Bandwidth            | `FM` = wide, `NFM`/`AM` = narrow           |
| `Power`       | TX power             | `High`/`Low`/`Mid`                         |
| `Skip`        | Scan list add        | Empty skip = add to scan, `S` = exclude    |

## Building from Source

```bash
git clone <repo-url>
cd RT950toCSV
dotnet build RT950toCSV.sln --configuration Release
```

### Publish a self-contained executable

```bash
dotnet publish src/RT950toCSV.App/RT950toCSV.App.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o publish/
```

The single-file executable will be placed in the `publish/` folder.

## Project Structure

```
RT950toCSV.sln
src/
  RT950toCSV.App/          # WinForms UI + conversion logic
    MainForm.cs            # Main window (Export / Import panels)
    ConverterCore.cs       # Core DAT ↔ CSV conversion routines
    ChannelRecord.cs       # Intermediate channel model
    ChirpRecord.cs         # CHIRP CSV row model (CsvHelper mapping)
    Serialization/
      KdhSerializationBinder.cs  # Remaps KDH type names for BinaryFormatter
  RT950toCSV.Data/         # Codeplug data model
    Codeplug/
      RadioCodeplug.cs     # Root codeplug object (15 zones, 64 slots each)
      ChannelProfile.cs    # Individual channel settings
      ChannelDataSection.cs
      ModulationSection.cs
      ... (other sections)
```

## Dependencies

| Package | Purpose |
|---------|---------|
| [CsvHelper](https://joshclose.github.io/CsvHelper/) | Reading and writing CHIRP-format CSV files |
| `System.Runtime.Serialization.Formatters` | Deserializing the RT950's NRBF binary format |

## Technical Notes

The RT950 Pro CPS saves its codeplug using .NET's `BinaryFormatter` (NRBF format) with KDH-specific type names. This tool:

1. Remaps those type names at deserialization time via a custom `SerializationBinder`.
2. After re-serialization, patches a small number of CLR array type names back to the KDH names expected by the CPS, so the resulting file loads correctly.

## License

See [LICENSE](LICENSE) if present, or contact the author for terms.
