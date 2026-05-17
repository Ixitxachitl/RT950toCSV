using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public class ModulationSection : SerializableSection
{
    private const int ChannelCount = 16;

    public ModulationSection() { EnsureChannels(); }
    protected ModulationSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { EnsureChannels(); }

    public ModulationChannelInfo[] Channels => ReadArray("modulationChannels", ChannelCount, () => new ModulationChannelInfo());

    private void EnsureChannels() => ReadArray("modulationChannels", ChannelCount, () => new ModulationChannelInfo());
}
