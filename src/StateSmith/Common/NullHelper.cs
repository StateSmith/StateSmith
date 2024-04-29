using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace StateSmith.Common
{
    public static class NullHelper
    {
        public static T ThrowIfNull<T>([NotNull] this T? value, string valueExpression = "")
        {
            if (value == null) 
                throw new ArgumentNullException(nameof(value), valueExpression);

            return value;
        }
    }
}
