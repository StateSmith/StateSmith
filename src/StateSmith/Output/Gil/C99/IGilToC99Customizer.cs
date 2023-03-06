#nullable enable

namespace StateSmith.Output.Gil.C99;

public interface IGilToC99Customizer
{
    string MakeHFileName();
    string MakeCFileName();
    string MakeEnumDeclaration(string enumName);
}
