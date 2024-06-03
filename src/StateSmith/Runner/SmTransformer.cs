#nullable enable

using StateSmith.SmGraph;
using System;
using System.Collections.Generic;

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

    public void Swap(Enum id1, Enum id2)
    {
        Swap(id1.ToString(), id2.ToString());
    }

    public void Swap(string id1, string id2)
    {
        int index1 = GetMatchIndex(id1);
        int index2 = GetMatchIndex(id2);
        var temp = transformationPipeline[index1];
        transformationPipeline[index1] = transformationPipeline[index2];
        transformationPipeline[index2] = temp;
    }

    //--------------------------------------------------------------------------------

    public void InsertAfterFirstMatch(Enum id, Action<StateMachine> action)
    {
        InsertAfterFirstMatch(id.ToString(), new TransformationStep(action));
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

    //--------------------------------------------------------------------------------

    public void InsertBeforeFirstMatch(Enum id, Action<StateMachine> action)
    {
        InsertBeforeFirstMatch(id.ToString(), new TransformationStep(action));
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

    public int GetMatchIndex(Enum id)
    {
        return GetMatchIndex(id.ToString());
    }

    public int GetMatchIndex(string id)
    {
        int index = transformationPipeline.FindIndex(s => s.Id == id);
        if (index == -1) throw new ArgumentOutOfRangeException($"{nameof(TransformationStep)} with id `{id}` was not found");
        return index;
    }
}
