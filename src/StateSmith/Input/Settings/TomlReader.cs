#nullable enable

using StateSmith.Common;
using StateSmith.Output;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace StateSmith.Input.Settings;

/// <summary>
/// https://github.com/StateSmith/StateSmith/issues/335
/// </summary>
public class TomlReader
{
    RenderConfigAllVars renderConfigAllVars;
    RunnerSettings smRunnerSettings;

    public TomlReader(RenderConfigAllVars renderConfigAllVars, RunnerSettings smRunnerSettings)
    {
        this.renderConfigAllVars = renderConfigAllVars;
        this.smRunnerSettings = smRunnerSettings;
    }

    public void Read(string toml)
    {
        var model = Toml.ToModel(toml);

        foreach (var key in model.Keys)
        {
            switch (key)
            {
                case "RenderConfig":
                    ReadIntoRenderConfig(renderConfigAllVars, (TomlTable)model[key]);
                    break;
                case "SmRunnerSettings":
                    ReadIntoRecursively(smRunnerSettings, (TomlTable)model[key]);
                    break;

                default:
                    throw new ArgumentException($"Unknown key found in toml settings: {key}");
            }
        }
    }

    private static void ReadIntoRenderConfig(RenderConfigAllVars renderConfigAllVars, TomlTable tomlTable)
    {
        foreach (var key in tomlTable.Keys)
        {
            ReadIntoRenderConfigMember(renderConfigAllVars, tomlTable, key);
        }
    }

    /// <summary>
    /// We have a special adapter for RenderConfig because we need to re-map non-table value to BaseVars.
    /// For example, the following toml should map to RenderConfigAllVars.Base.CFileExtension:
    ///     [RenderConfig]
    ///     CFileExtension = ".inc"
    /// </summary>
    private static void ReadIntoRenderConfigMember(RenderConfigAllVars renderConfigAllVars, TomlTable tomlTable, string tomlKey)
    {
        var tomlValue = tomlTable[tomlKey];

        if (tomlValue is not TomlTable)
        {
            // non-tables are base vars
            ReadIntoObjectMemberRecursively(renderConfigAllVars.Base, tomlTable, tomlKey);
        }
        else
        {
            ReadIntoObjectMemberRecursively(renderConfigAllVars, tomlTable, tomlKey);
        }
    }

    private static void ReadIntoRecursively(object obj, TomlTable tomlTable)
    {
        foreach (var tomlKey in tomlTable.Keys)
        {
            ReadIntoObjectMemberRecursively(obj, tomlTable, tomlKey);
        }
    }

    private static void ReadIntoObjectMemberRecursively(object obj, TomlTable tomlTable, string tomlKey)
    {
        var tomlValue = tomlTable[tomlKey];

        PropertyInfo? property = obj.GetType().GetProperty(tomlKey);
        FieldInfo? field = obj.GetType().GetField(tomlKey);

        if (property == null && field == null)
            throw new ArgumentException($"Unknown key found in toml settings: `{tomlKey}`");

        object? objMemberOriginalValue = property?.GetValue(obj) ?? field?.GetValue(obj);

        tomlValue = ConvertTomlValue(tomlValue, objMemberOriginalValue);

        if (tomlValue is TomlTable tomlTableValue)
        {
            ReadIntoRecursively(objMemberOriginalValue.ThrowIfNull(), tomlTableValue);
        }
        else
        {
            property?.SetValue(obj, tomlValue);
            field?.SetValue(obj, tomlValue);
        }
    }

    private static object ConvertTomlValue(object tomlValue, object? objMemberOriginalValue)
    {
        // nums are parsed as strings, so we need to convert them
        if (objMemberOriginalValue?.GetType().IsEnum ?? false)
        {
            tomlValue = Enum.Parse(objMemberOriginalValue.GetType(), (string)tomlValue);
        }
        // some strings are indented, we need to de-indent them
        else if (tomlValue is string s)
        {
            tomlValue = StringUtils.DeIndent(s);
        }

        return tomlValue;
    }
}
