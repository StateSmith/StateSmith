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
    public void Customize(AlgorithmId algorithmId, TranspilerId transpilerId, AlgoBalanced1Settings algoBalanced1Settings, CodeStyleSettings style)
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
                    algoBalanced1Settings.outputSwitchDefault = true;
                }
                break;

            case TranspilerId.Cpp:
                {
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
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;

            case TranspilerId.JavaScript:
                {
                    algoBalanced1Settings.skipClassIndentation = false;
                }
                break;

            case TranspilerId.Java:
                {
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


