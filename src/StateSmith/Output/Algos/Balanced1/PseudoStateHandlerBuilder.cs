#nullable enable

using StateSmith.Common;
using StateSmith.SmGraph.Visitors;
using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StateSmith.Output.Algos.Balanced1;

public class PseudoStateHandlerBuilder
{
    /// <summary>
    /// needs to be set before using this class
    /// </summary>
    public OutputFile? output;

    /// <summary>
    /// needs to be set before using this class
    /// </summary>
    public INameMangler? mangler;
    readonly Dictionary<PseudoStateVertex, string> functionNameMap = new();
    readonly HashList<NamedVertex, PseudoStateVertex> parentMapping = new();

    public void Gather(StateMachine sm)
    {
        LambdaVertexBreadthWalker walker = new()
        {
            visitAction = v =>
        {
            if (v is PseudoStateVertex pseudoState && ShouldCreateFunctionHandler(v))
            {
                Add(pseudoState);
            }
        }
        };

        walker.Walk(sm);
    }

    private static bool ShouldCreateFunctionHandler(Vertex v)
    {
        var IncomingTransitionCount = v.IncomingTransitions.Count;

        if (v is InitialState)
        {
            IncomingTransitionCount += v.Parent!.IncomingTransitions.Count;
        }

        return IncomingTransitionCount > 1;
    }

    public void MapParents()
    {
        foreach (var pseudoState in functionNameMap.Keys)
        {
            parentMapping.AddIfValueMissing((NamedVertex)pseudoState.NonNullParent, pseudoState);
        }
    }

    public void Add(PseudoStateVertex pseudoStateVertex)
    {
        string functionName = CreateUniqueName(pseudoStateVertex);
        functionNameMap.Add(pseudoStateVertex, functionName);
    }

    private string CreateUniqueName(PseudoStateVertex pseudoStateVertex)
    {
        var functionName = Vertex.Describe(pseudoStateVertex);
        functionName = Regex.Replace(functionName, @"[.()]+", "_");
        functionName = Regex.Replace(functionName, @"[<>]+", "");
        functionName += "_transition";
        while (functionNameMap.ContainsValue(functionName))
        {
            functionName += "_kid_index" + pseudoStateVertex.FindIndexInParentKids();
        }

        return functionName;
    }

    public Dictionary<PseudoStateVertex, string>.ValueCollection GetAllFunctionNames()
    {
        return functionNameMap.Values;
    }

    public void OutputFunctionSignature(PseudoStateVertex pseudoStateVertex)
    {
        var functionName = GetFunctionName(pseudoStateVertex);
        output.ThrowIfNull().AppendIndented($"private void {functionName}()");
    }

    public void OutputAllPrototypes()
    {
        output.ThrowIfNull();

        foreach (var vertex in functionNameMap.Keys)
        {
            OutputFunctionSignature(vertex);
            output.AppendIndentedLine(";");
            output.RequestNewLineBeforeMoreCode();
        }
    }

    public void OutputFunctionsForParent(NamedVertex parent, Action<PseudoStateVertex> renderLambda)
    {
        if (!parentMapping.Contains(parent))
        {
            return;
        }

        foreach (var pseudoKid in parentMapping.GetValues(parent))
        {
            OutputFunction(pseudoKid, renderLambda);
        }
    }

    protected void OutputFunction(PseudoStateVertex pseudoStateVertex, Action<PseudoStateVertex> renderLambda)
    {
        output.ThrowIfNull();

        OutputFunctionSignature(pseudoStateVertex);
        output.StartCodeBlock();
        renderLambda(pseudoStateVertex);
        output.FinishCodeBlock();
        output.RequestNewLineBeforeMoreCode();
    }

    public string GetFunctionName(PseudoStateVertex pseudoStateVertex)
    {
        return functionNameMap[pseudoStateVertex];
    }

    public string? MaybeGetFunctionName(PseudoStateVertex pseudoStateVertex)
    {
        if (Contains(pseudoStateVertex) == false)
        {
            return null;
        }

        return functionNameMap[pseudoStateVertex];
    }

    private bool Contains(PseudoStateVertex pseudoStateVertex)
    {
        return functionNameMap.ContainsKey(pseudoStateVertex);
    }
}
