#nullable enable

using System.Collections.Generic;

namespace StateSmith.Output.UserConfig.AutoVars;

public interface IAutoVarsParser
{
    List<string> ParseIdentifiers(string code);
}
