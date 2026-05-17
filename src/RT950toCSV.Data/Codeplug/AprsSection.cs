using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class AprsSection : SerializableSection
{
    public AprsSection() { }
    protected AprsSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
