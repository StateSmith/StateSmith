#nullable enable

using System.Collections.Generic;

namespace StateSmith.Output.UserConfig;

public interface IAutoVarsParser
{
    List<string> ParseIdentifiers(string code);
}
