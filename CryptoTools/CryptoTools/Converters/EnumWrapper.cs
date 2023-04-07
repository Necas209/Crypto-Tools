using System;

namespace CryptoTools.Converters;

public class EnumWrapper<TEnum>
    where TEnum : struct, Enum
{
    public EnumWrapper(TEnum value)
    {
        Value = value;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public TEnum Value { get; }

    // ReSharper disable once UnusedMember.Global
    public string? Name => Enum.GetName(typeof(TEnum), Value);
}