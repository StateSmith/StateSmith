using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;

/*
 * This file is intended to provide language agnostic helpers and expansions
 */
namespace Spec.Spec1;

public class Spec1GenericVarExpansions : SpecGenericVarExpansions
{
    #pragma warning disable IDE1006 // Naming Styles
    string count => AutoVarName();
    #pragma warning restore IDE1006 // Naming Styles
}

public class Spec1Fixture : SpecFixture
{
    public static string Spec1Directory => SpecInputDirectoryPath + "1/";

    public static void CompileAndRun(IRenderConfigC renderConfigC, string outputDir)
    {
        var diagramFile = Spec1Directory + "Spec1Sm.graphml";
        CompileAndRun(renderConfigC, diagramFile, outputDir);
    }
}
