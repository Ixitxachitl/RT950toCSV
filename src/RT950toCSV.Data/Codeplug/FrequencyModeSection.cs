using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class FrequencyModeSection : SerializableSection
{
    public FrequencyModeSection() { ApplyDefaults(); }
    protected FrequencyModeSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { ApplyDefaults(); }

    public VfoSettings VfoA => ReadObject("vfoA", () => new VfoSettings { ReceiveFrequency = "400.12500" });
    public VfoSettings VfoB => ReadObject("vfoB", () => new VfoSettings { ReceiveFrequency = "435.12500" });
    public VfoSettings VfoC => ReadObject("vfoC", () => new VfoSettings { ReceiveFrequency = "469.97500" });

    private void ApplyDefaults()
    {
        ReadObject("vfoA", () => new VfoSettings { ReceiveFrequency = "400.12500" });
        ReadObject("vfoB", () => new VfoSettings { ReceiveFrequency = "435.12500" });
        ReadObject("vfoC", () => new VfoSettings { ReceiveFrequency = "469.97500" });
    }
}
