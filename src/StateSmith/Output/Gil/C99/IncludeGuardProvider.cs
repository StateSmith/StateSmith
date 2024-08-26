#nullable enable

using StateSmith.Output.UserConfig;
using System.Text;

// spell-checker: ignore customizer

namespace StateSmith.Output.Gil.C99;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/112
/// </summary>
public class IncludeGuardProvider
{
    private readonly string? IncludeGuardLabel;

    public IncludeGuardProvider(RenderConfigCVars renderConfigC, IOutputInfo outputInfo)
    {
        if (string.IsNullOrWhiteSpace(renderConfigC.IncludeGuardLabel))
        {
            IncludeGuardLabel = null;
        }
        else
        {
            IncludeGuardLabel = renderConfigC.IncludeGuardLabel;
            IncludeGuardLabel = IncludeGuardLabel.Replace("{fileName}", outputInfo.BaseFileName);
            IncludeGuardLabel = IncludeGuardLabel.Replace("{FILENAME}", outputInfo.BaseFileName.ToUpper());
        }
    }

    public void OutputIncludeGuardTop(StringBuilder hFileSb)
    {
        if (IncludeGuardLabel == null)
        {
            hFileSb.AppendLine("#pragma once  // You can also specify normal include guard. See https://github.com/StateSmith/StateSmith/issues/112");
        }
        else
        {
            hFileSb.AppendLine($"#ifndef {IncludeGuardLabel}");
            hFileSb.AppendLine($"#define {IncludeGuardLabel}");
        }
    }

    public void OutputIncludeGuardBottom(StringBuilder hFileSb)
    {
        if (IncludeGuardLabel != null)
        {
            hFileSb.AppendLine($"#endif // {IncludeGuardLabel}");
        }
    }
}
