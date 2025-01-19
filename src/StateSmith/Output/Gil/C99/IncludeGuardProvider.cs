#nullable enable

using System.Text;

namespace StateSmith.Output.Gil.C99;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/112
/// </summary>
public class IncludeGuardProvider
{
    private readonly string? includeGuardLabel;

    public IncludeGuardProvider(string includeGuardLabel, IOutputInfo outputInfo)
    {
        if (string.IsNullOrWhiteSpace(includeGuardLabel))
        {
            this.includeGuardLabel = null;
        }
        else
        {
            this.includeGuardLabel = includeGuardLabel;
            this.includeGuardLabel = this.includeGuardLabel.Replace("{fileName}", outputInfo.BaseFileName);
            this.includeGuardLabel = this.includeGuardLabel.Replace("{FILENAME}", outputInfo.BaseFileName.ToUpper());
        }
    }

    public void OutputIncludeGuardTop(StringBuilder hFileSb)
    {
        if (includeGuardLabel == null)
        {
            hFileSb.AppendLine("#pragma once  // You can also specify normal include guard. See https://github.com/StateSmith/StateSmith/blob/main/docs/settings.md");
        }
        else
        {
            hFileSb.AppendLine($"#ifndef {includeGuardLabel}");
            hFileSb.AppendLine($"#define {includeGuardLabel}");
        }
    }

    public void OutputIncludeGuardBottom(StringBuilder hFileSb)
    {
        if (includeGuardLabel != null)
        {
            hFileSb.AppendLine($"#endif // {includeGuardLabel}");
        }
    }
}
