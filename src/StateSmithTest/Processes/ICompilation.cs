#nullable enable

namespace StateSmithTest.Processes;

public interface ICompilation
{
    SimpleProcess Run(string runArgs = "");
}
