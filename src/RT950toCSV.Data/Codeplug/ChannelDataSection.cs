using System;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

/// <summary>
/// Maps to <c>KDH.ChannelData</c> in the Radtel CPS binary format.
/// This is a container object that wraps the channel array and zone-name array.
/// It is not a single channel; individual channels are <see cref="ChannelProfile"/>
/// (KDH.Channel).
/// </summary>
[Serializable]
public sealed class ChannelDataSection : SerializableSection
{
    public ChannelDataSection()
    {
        // Initialize channel array
        var channels = new ChannelProfile[RadioCodeplug.ChannelSlots];
        for (var i = 0; i < channels.Length; i++)
            channels[i] = new ChannelProfile();
        Values["channelList"]   = channels;

        // Initialize zone names (15 zones, empty by default — CPS fills them from radio)
        Values["arrayZoneName"] = new string[RadioCodeplug.ZoneCount];
    }

    private ChannelDataSection(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>960 channel slots (15 zones × 64).</summary>
    public ChannelProfile[] Channels =>
        ReadArray<ChannelProfile>("channelList", RadioCodeplug.ChannelSlots, () => new ChannelProfile());

    /// <summary>15 zone name strings.</summary>
    public string[] ZoneNames
    {
        get
        {
            if (Values.TryGetValue("arrayZoneName", out var v) && v is string[] arr)
                return arr;
            var names = new string[RadioCodeplug.ZoneCount];
            Values["arrayZoneName"] = names;
            return names;
        }
        set => Values["arrayZoneName"] = value;
    }
}
