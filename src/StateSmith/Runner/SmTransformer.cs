using StateSmith.Common;
using StateSmith.Compiling;
using StateSmith.output.C99BalancedCoder1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateSmith.Runner;

public class TransformationStep
{
    public string Id { get; init; }
    public Action<Statemachine> action;

    public TransformationStep(string id, Action<Statemachine> action)
    {
        Id = id;
        this.action = action;
    }
}

public class SmTransformer
{
    public readonly List<TransformationStep> transformationPipeline = new();

    readonly Statemachine sm;

    public SmTransformer(Statemachine sm)
    {
        this.sm = sm;
    }

    public void RunTransformationPipeline()
    {
        foreach (var step in transformationPipeline)
        {
            step.action(sm);
        }
    }

    public void InsertAfterFirstMatch(string id, TransformationStep step)
    {
        int index = GetMatchIndex(id);
        transformationPipeline.Insert(index + 1, step);
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


public class DefaultSmTransformer : SmTransformer
{
    public enum Id
    {
        RemoveNotesVertices,
        SupportParentAlias,
        SupportEntryExit,
        SupportHistory,
        SupportOrderAndElse,
        Valdation1,
        DefaultUnspecifiedEventsAsDoEvent,
        AddUsedEventsToSm,
        FinalValdation,
    };

    public DefaultSmTransformer(Statemachine sm, CNameMangler mangler) : base(sm)
    {
        AddStep(Id.RemoveNotesVertices, (sm) => NotesProcessor.Process(sm));
        AddStep(Id.SupportParentAlias, (sm) => ParentAliasStateProcessor.Process(sm));
        AddStep(Id.SupportEntryExit, (sm) => EntryExitProcessor.Process(sm));
        AddStep(Id.SupportHistory, (sm) => HistoryProcessor.Process(sm, mangler));
        AddStep(Id.SupportOrderAndElse, (sm) => OrderAndElseProcessor.Process(sm)); // should happen after most steps as it orders behaviors
        AddStep(Id.Valdation1, (sm) => Validate(sm));
        AddStep(Id.DefaultUnspecifiedEventsAsDoEvent, (sm) => DefaultToDoEventVisitor.Process(sm));
        AddStep(Id.AddUsedEventsToSm, (sm) => AddUsedEventsToSmClass.Process(sm));
        AddStep(Id.FinalValdation, (sm) => Validate(sm));
    }

    private void AddStep(Id id, Action<Statemachine> action)
    {
        transformationPipeline.Add(new($"{nameof(DefaultSmTransformer)}.{id}", action));
    }

    public static void Validate(Statemachine sm)
    {
        var validator = new SpecificVertexValidator();
        sm.Accept(validator);
        var validator2 = new VertexValidator();
        sm.Accept(validator2);
    }
}
