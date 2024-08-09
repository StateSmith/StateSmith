#nullable enable

namespace StateSmith.Output.Gil.C99;

public interface IGilToC99Customizer
{
    void Setup();
    string MakeHFileName();
    string MakeHGuard();
    string MakeCFileName();
    string MakeEnumDeclaration(string enumName);
}
