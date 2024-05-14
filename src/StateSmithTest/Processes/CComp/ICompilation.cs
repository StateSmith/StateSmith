#nullable enable

namespace StateSmithTest.Processes.CComp;

public interface ICompilation
{
    SimpleProcess RunExecutable(string runArgs = "");
}
