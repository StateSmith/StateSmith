#nullable enable

using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.Algos.Balanced2;
using StateSmith.Output.Gil.C99;
using StateSmith.Output.Gil.Cpp;
using StateSmith.Output.Gil.CSharp;
using StateSmith.Output.Gil.Java;
using StateSmith.Output.Gil.JavaScript;
using StateSmith.Output.Gil.Python;
using StateSmith.Output.Gil.TypeScript;
using StateSmith.Output.UserConfig.AutoVars;
using System;

namespace StateSmith.Runner;

/// <summary>
/// Used to load different settings depending on algorithm and target language selected.
/// </summary>
public class AlgoTranspilerCustomizer
{
    public void Customize(DiServiceProvider sp, AlgorithmId algorithmId, TranspilerId transpilerId, AlgoBalanced1Settings algoBalanced1Settings, CodeStyleSettings style)
    {
        if (algorithmId == AlgorithmId.Default)
        {
            algorithmId = AlgorithmId.Balanced2;
        }

        algoBalanced1Settings.outputSwitchDefault = false;  // default to false. Needed because of default case below.

        switch (transpilerId)
        {
            case TranspilerId.Default:
            case TranspilerId.C99:
                {
                    // TODO remove. causes spec2 tests to fail
                    sp.AddSingletonT<IGilTranspiler, GilToC99>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CExpansionVarsPathProvider>();
                    algoBalanced1Settings.outputSwitchDefault = true;
                }
                break;

            case TranspilerId.Cpp:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToCpp>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CppExpansionVarsPathProvider>();
                    // sp.AddSingletonT<NameMangler, CamelCaseNameMangler>();
                    algoBalanced1Settings.skipClassIndentation = false;
                    algoBalanced1Settings.varsStructAsClass = true;
                    algoBalanced1Settings.outputSwitchDefault = true;

                    // https://github.com/StateSmith/StateSmith/issues/411
                    if (algorithmId != AlgorithmId.Balanced2)
                    {
                        throw new Exception("Cpp transpiler currently only supports `AlgorithmId.Balanced2`. You can use C99 transpiler or reply to https://github.com/StateSmith/StateSmith/issues/411 .");
                    }
                }
                break;

            case TranspilerId.CSharp:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToCSharp>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
                    sp.AddSingletonT<NameMangler, PascalFuncCamelVarNameMangler>(); // TODO fix spec tests and remove
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;

            case TranspilerId.JavaScript:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToJavaScript>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();    // todo - rename to something common
                    // sp.AddSingletonT<NameMangler, CamelCaseNameMangler>();
                    // sp.AddSingletonT<IAutoVarsParser, JsAutoVarsParser>();
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;

            case TranspilerId.Java:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToJava>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
                    // sp.AddSingletonT<NameMangler, CamelCaseNameMangler>();
                    algoBalanced1Settings.skipClassIndentation = false;

                    // https://github.com/StateSmith/StateSmith/issues/395
                    if (algorithmId != AlgorithmId.Balanced2)
                    {
                        throw new Exception("Java transpiler currently only supports `AlgorithmId.Balanced2`. Please reply to https://github.com/StateSmith/StateSmith/issues/395 .");
                    }
                }
                break;

            case TranspilerId.Python:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToPython>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, PythonExpansionVarsPathProvider>();
                    // sp.AddSingletonT<NameMangler, CamelCaseNameMangler>();
                    // sp.AddSingletonT<IAutoVarsParser, PythonAutoVarsParser>();

                    algoBalanced1Settings.skipClassIndentation = false;
                    algoBalanced1Settings.outputEnumMemberCount = false;
                    algoBalanced1Settings.varsStructAsClass = true;
                    algoBalanced1Settings.useIfTrueIfNoGuard = true;
                    algoBalanced1Settings.allowSingleLineSwitchCase = false;

                    if (!style.BracesOnNewLines)
                    {
                        throw new Exception("Python transpiler currently only supports `style.BracesOnNewLines = true`.");
                    }

                    // https://github.com/StateSmith/StateSmith/issues/395
                    if (algorithmId != AlgorithmId.Balanced2)
                    {
                        throw new Exception("Python transpiler currently only supports `AlgorithmId.Balanced2`. Please reply to https://github.com/StateSmith/StateSmith/issues/398 .");
                    }
                }
                break;

            case TranspilerId.TypeScript:
                {
                    // sp.AddSingletonT<IGilTranspiler, GilToTypeScript>();
                    // sp.AddSingletonT<IExpansionVarsPathProvider, CSharpExpansionVarsPathProvider>();
                    // sp.AddSingletonT<NameMangler, CamelCaseNameMangler>();
                    // sp.AddSingletonT<IAutoVarsParser, TypeScriptAutoVarsParser>();
                    algoBalanced1Settings.varsStructAsClass = true;
                    algoBalanced1Settings.skipClassIndentation = false;
                    //style.BracesOnNewLines = false; // todolow - it would be nice to support this

                    // https://github.com/StateSmith/StateSmith/issues/407
                    if (algorithmId != AlgorithmId.Balanced2)
                    {
                        throw new Exception("TypeScript transpiler currently only supports `AlgorithmId.Balanced2`. Please reply to https://github.com/StateSmith/StateSmith/issues/407 .");
                    }
                }
                break;
        }
    }

}


