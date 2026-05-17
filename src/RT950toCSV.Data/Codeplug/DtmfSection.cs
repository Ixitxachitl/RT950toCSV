using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class DtmfSection : SerializableSection
{
    private const int GroupCount = 22;

    public DtmfSection() { ApplyDefaults(); }
    protected DtmfSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { ApplyDefaults(); }

    public string CurrentId => ReadString("tB_DTMFCurId", "12345");

    public string[] CodeGroups
    {
        get
        {
            if (Values.TryGetValue("dtmfCodeGroup", out var value) && value is string[] list && list.Length == GroupCount)
                return list;
            var created = new string[GroupCount];
            for (var i = 0; i < created.Length; i++) created[i] = string.Empty;
            created[0] = "101010";
            Values["dtmfCodeGroup"] = created;
            return created;
        }
    }

    private void ApplyDefaults()
    {
        EnsureDefault("tB_DTMFCurId",        "12345");
        EnsureDefault("cbB_FrByte5",          0);
        EnsureDefault("cbB_PTTID",            0);
        EnsureDefault("cbB_LastTimeSend",     1);
        EnsureDefault("cbB_LastTimeStop",     1);
        EnsureDefault("cbB_FrByte9to15",      0);
        EnsureDefault("cbB_FrByte16to23",     0);
        EnsureDefault("cbB_FrByte24to31",     0);
        if (!Values.ContainsKey("dtmfCodeGroup"))
        {
            var codes = new string[GroupCount];
            for (var i = 0; i < codes.Length; i++) codes[i] = string.Empty;
            codes[0] = "101010";
            Values["dtmfCodeGroup"] = codes;
        }
    }
}
