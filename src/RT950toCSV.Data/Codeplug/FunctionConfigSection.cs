using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class FunctionConfigSection : SerializableSection
{
    public FunctionConfigSection() { }
    protected FunctionConfigSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
