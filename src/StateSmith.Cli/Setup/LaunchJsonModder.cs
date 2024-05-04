using System.Text.RegularExpressions;

namespace StateSmith.Cli.Setup;

public class LaunchJsonModder
{
    const string newContent = @"
""type"": ""coreclr"",
""request"": ""launch"",
""program"": ""${env:HOME}/.dotnet/tools/dotnet-script"",
""args"": [
<indent>""${file}""
],
""windows"": {
<indent>""program"": ""${env:USERPROFILE}/.dotnet/tools/dotnet-script.exe"",
},
""logging"": {
<indent>""moduleLoad"": false
},
""cwd"": ""${workspaceRoot}"",";

    public static string? MaybeMod(string launchJson)
    {
        string indent = "    ";

        // find tab or space at start of line
        Regex indentRegex = new(@"(?xm)
            ^ [{] \s*
            (
                ^[\t ]+
            )
            ");

        var indentMatch = indentRegex.Match(launchJson);
        if (indentMatch.Success)
        {
            indent = indentMatch.Groups[1].Value;
        }

        var r = new Regex(@"(?xm)
            (?<leadingSpace> ^ [ \t]* )?
            (?<start>            
                ""name"" \s* : \s* "".NET[ ]Script[ ]Debug"" \s*,
            )
            (?<content> [\s\S]*? )
            (?<end>            
              \s*  ""stopAtEntry"" \s* :
            )
        ");

        var newJson = r.Replace(launchJson, (Match m) =>
        {
            var leadingSpace = m.Groups["leadingSpace"].Value;
            var start = m.Groups["start"].Value;
            var content = m.Groups["content"].Value;
            var end = m.Groups["end"].Value;

            content = newContent.ReplaceLineEndings("\n" + leadingSpace).Replace("<indent>", indent);

            return $@"{leadingSpace}{start}{content}{end}";
        });

        if (newJson != launchJson)
        {
            return newJson;
        }

        return null;
    }
}
