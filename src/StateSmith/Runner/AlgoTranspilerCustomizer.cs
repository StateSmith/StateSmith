using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.Gil.CSharp;
using StateSmith.Output.Gil.JavaScript;
using StateSmith.Output.UserConfig;

#nullable enable

namespace StateSmith.Runner;

/// <summary>
/// Used to load different settings depending on algorithm and target language selected.
/// </summary>
public class AlgoTranspilerCustomizer
{
    public void Customize(DiServiceProvider sp, AlgorithmId algorithmId, TranspilerId transpilerId)
    {
        if (algorithmId == AlgorithmId.Default)
            algorithmId = AlgorithmId.Balanced1;

        if (algorithmId != AlgorithmId.Balanced1)
            throw new System.ArgumentException("Only Balanced1 algorithm is supported right now.");

        if (transpilerId == TranspilerId.Default)
            transpilerId = TranspilerId.C99;

        AlgoBalanced1Settings algoBalanced1Settings = new();
        sp.AddSingletonT(algoBalanced1Settings);

        switch (transpilerId)
        {
            case TranspilerId.C99:
                {
                    sp.AddSingletonT<IGilTranspiler, GilToC99>();
                    sp.AddSingletonT<IExpansionVarsPathProvider, CExpansionVarsPathProvider>();
                }
                break;

            case TranspilerId.CSharp:
                {
                    sp.AddSingletonT<IGilTranspiler, GilToCSharp>();
                    sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
                    sp.AddSingletonT<NameMangler, PascalFuncCamelVarNameMangler>();
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;

            case TranspilerId.JavaScript:
                {
                    sp.AddSingletonT<IGilTranspiler, GilToJavaScript>();
                    sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();    // todo - rename to something common
                    sp.AddSingletonT<NameMangler, PascalFuncCamelVarNameMangler>(); // fixme: use camelCase
                    sp.AddSingletonT<IAutoVarsParser, JsAutoVarsParser>();
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;
        }
    }

}


