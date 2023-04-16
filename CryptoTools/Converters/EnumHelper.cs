using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace CryptoTools.Converters;

public static class EnumHelper
{
    public static List<EnumWrapper<CompressionLevel>> CompressionLevelValues =>
        Enum.GetValues(typeof(CompressionLevel))
            .Cast<CompressionLevel>()
            .Select(value => new EnumWrapper<CompressionLevel>(value))
            .ToList();
}