using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace StateSmith.Common;

public static class NullHelper
{
    public static T ThrowIfNull<T>([NotNull] this T? value, string message = "")
    {
        if (value == null) 
            throw new ArgumentNullException(nameof(value), message);

        return value;
    }
}
