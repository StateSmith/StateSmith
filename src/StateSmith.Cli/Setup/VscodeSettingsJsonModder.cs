using StateSmith.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StateSmith.Cli.Setup;

public class VscodeSettingsJsonModder
{
    /// <summary>
    /// Returns null if nothing was changed.
    /// </summary>
    /// <param name="jsonStr"></param>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public string? AddHedietVscodePlugin(string jsonStr, string plugin)
    {
        JObject jsonObj = JObject.Parse(jsonStr).ThrowIfNull();
        JArray? jArray = (JArray?)jsonObj["hediet.vscode-drawio.plugins"];

        // don't add if it already exists
        if (jArray != null)
        {
            foreach (var item in jArray)
            {
                if (item["file"]?.Value<string>() == plugin)
                {
                    return null;
                }
            }
        }

        if (jArray == null)
        {
            jArray = new JArray();
            jsonObj["hediet.vscode-drawio.plugins"] = jArray;
        }

        jArray.Add(new JObject(new JProperty("file", plugin)));

        return jsonObj.ToString(Formatting.Indented); // TODOLOW: preserve original formatting
    }
}
