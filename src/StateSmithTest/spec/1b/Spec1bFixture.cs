using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;

/*
 * This file is intended to provide language agnostic helpers and expansions
 */
namespace Spec.Spec1b;

public class Spec1bGenericVarExpansions : SpecGenericVarExpansions
{
    #pragma warning disable IDE1006 // Naming Styles
    string print(string line) => $"print(\"{line}\")";
    string a() => print(AutoNameCopy()+"(); ");
    string b() => print(AutoNameCopy()+"(); ");
    string c() => print(AutoNameCopy()+"(); ");
    string d() => print(AutoNameCopy()+"(); ");
    string e() => print(AutoNameCopy()+"(); ");
    string t() => print(AutoNameCopy()+"(); ");
    string g() => print(AutoNameCopy()+"() "); // guard
    #pragma warning restore IDE1006 // Naming Styles
}

public class Spec1bFixture : SpecFixture
{
    public static string Spec1Directory => SpecInputDirectoryPath + "1b/";

    public static void CompileAndRun(IRenderConfigC renderConfigC, string outputDir)
    {
        var diagramFile = Spec1Directory + "Spec1bSm.graphml";
        CompileAndRun(renderConfigC, diagramFile, outputDir, useTracingModder: false);
    }
}
