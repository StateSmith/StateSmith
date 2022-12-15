#nullable enable

using StateSmith.compiler.Visitors;
using StateSmith.Compiling;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StateSmith.output.C99BalancedCoder1
{
    public class PseudoStateHandlerBuilder
    {
        public OutputFile output;
        public CNameMangler mangler;

        Dictionary<PseudoStateVertex, string> functionNameMap = new();
        HashList<NamedVertex, PseudoStateVertex> parentMapping = new();

        public void Gather(Statemachine sm)
        {
            LambdaVertexBreadthWalker walker = new() { visitAction = v => {
                if (v is PseudoStateVertex pseudoState && v.IncomingTransitions.Count > 1)
                {
                    Add(pseudoState);
                }
            } };

            walker.Walk(sm);
        }

        public void MapParents()
        {
            foreach (var pseudoState in functionNameMap.Keys)
            {
                parentMapping.AddIfMissing((NamedVertex)pseudoState.Parent, pseudoState);
            }
        }

        public void Add(PseudoStateVertex pseudoStateVertex)
        {
            var functionName = Vertex.Describe(pseudoStateVertex);
            functionName = Regex.Replace(functionName, @"[.()]+", "_") + "_transition";
            functionNameMap.Add(pseudoStateVertex, functionName);
        }

        public Dictionary<PseudoStateVertex, string>.ValueCollection GetAllFunctionNames()
        {
            return functionNameMap.Values;
        }

        public void OutputFunctionSignature(PseudoStateVertex pseudoStateVertex)
        {
            var functionName = GetFunctionName(pseudoStateVertex);
            output.Append($"static void {functionName}({mangler.SmStructTypedefName}* self)");
        }

        public void OutputAllPrototypes()
        {
            foreach (var vertex in functionNameMap.Keys)
            {
                OutputFunctionSignature(vertex);
                output.AppendLine(";");
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

        public void OutputFunction(PseudoStateVertex pseudoStateVertex, Action<PseudoStateVertex> renderLambda)
        {
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
}
