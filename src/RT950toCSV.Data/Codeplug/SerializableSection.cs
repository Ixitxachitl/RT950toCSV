using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace RT950toCSV.Data.Codeplug;

[Serializable]
public abstract class SerializableSection : ISerializable
{
    protected Dictionary<string, object?> Values { get; }

    protected SerializableSection()
    {
        Values = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    protected SerializableSection(SerializationInfo info, StreamingContext context)
        : this()
    {
        foreach (SerializationEntry entry in info)
        {
            Values[entry.Name] = entry.Value;
        }
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        foreach (var pair in Values)
        {
            info.AddValue(pair.Key, pair.Value);
        }
    }

    protected string ReadString(string name, string fallback = "")
    {
        if (Values.TryGetValue(name, out var value))
        {
            return value as string ?? fallback;
        }
        return fallback;
    }

    protected void WriteString(string name, string? value, string fallback = "")
    {
        Values[name] = string.IsNullOrEmpty(value) ? fallback : value;
    }

    protected int ReadInt(string name, int fallback = 0)
    {
        if (Values.TryGetValue(name, out var value))
        {
            if (value is int i) return i;
            if (value is IConvertible convertible)
            {
                try { return convertible.ToInt32(CultureInfo.InvariantCulture); }
                catch { }
            }
        }
        return fallback;
    }

    protected void WriteInt(string name, int value) => Values[name] = value;

    protected T ReadObject<T>(string name, Func<T> factory) where T : class
    {
        if (Values.TryGetValue(name, out var value) && value is T typed)
            return typed;
        var created = factory();
        Values[name] = created;
        return created;
    }

    protected T[] ReadArray<T>(string name, int length, Func<T> factory)
    {
        if (Values.TryGetValue(name, out var value) && value is T[] typed && typed.Length == length)
        {
            for (var i = 0; i < typed.Length; i++)
            {
                if (typed[i] == null)
                    typed[i] = factory();
            }
            return typed;
        }
        var result = new T[length];
        for (var i = 0; i < result.Length; i++)
            result[i] = factory();
        Values[name] = result;
        return result;
    }

    protected void EnsureDefault(string name, object? value)
    {
        if (!Values.ContainsKey(name))
            Values[name] = value;
    }
}
