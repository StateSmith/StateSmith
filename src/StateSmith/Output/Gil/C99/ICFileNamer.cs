#nullable enable

namespace StateSmith.Output.Gil.C99;

public interface ICFileNamer
{
    string MakeHFileName();
    string MakeCFileName();
}
