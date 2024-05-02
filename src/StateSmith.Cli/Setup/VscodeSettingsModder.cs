using StateSmith.Common;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StateSmith.Cli.Setup;

public class VscodeSettingsModder
{
    public string AddHedietVscodePlugin(string jsonStr, string plugin)
    {
        JObject jsonObj = JObject.Parse(jsonStr).ThrowIfNull();

        JArray? jArray = (JArray?)jsonObj["hediet.vscode-drawio.plugins"];

        if (jArray == null)
        {
            jArray = new JArray();
            jsonObj["hediet.vscode-drawio.plugins"] = jArray;
        }

        // TODO: don't add if it already exists
        jArray.Add(new JObject(new JProperty("file", plugin)));

        return jsonObj.ToString(Formatting.Indented); // TODOLOW: preserve original formatting
    }
}
