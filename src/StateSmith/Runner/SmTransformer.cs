using StateSmith.Common;
using StateSmith.SmGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateSmith.Runner;

public class SmTransformer
{
    public readonly List<TransformationStep> transformationPipeline = new();

    public void RunTransformationPipeline(StateMachine sm)
    {
        foreach (var step in transformationPipeline)
        {
            step.action(sm);
        }
    }

    public void InsertAfterFirstMatch(Enum id, TransformationStep step)
    {
        InsertAfterFirstMatch(id.ToString(), step);
    }

    public void InsertAfterFirstMatch(string id, TransformationStep step)
    {
        int index = GetMatchIndex(id);
        transformationPipeline.Insert(index + 1, step);
    }

    public void InsertBeforeFirstMatch(Enum id, TransformationStep step)
    {
        InsertBeforeFirstMatch(id.ToString(), step);
    }

    public void InsertBeforeFirstMatch(string id, TransformationStep step)
    {
        int index = GetMatchIndex(id);
        transformationPipeline.Insert(index, step);
    }

    public int GetMatchIndex(string id)
    {
        int index = transformationPipeline.FindIndex(s => s.Id == id);
        if (index == -1) throw new ArgumentOutOfRangeException($"{nameof(TransformationStep)} with id `{id}` was not found");
        return index;
    }
}
