using StateSmith.SmGraph;
using System;

namespace StateSmith.Runner;

public class StandardSmTransformer : SmTransformer
{
    readonly HistoryProcessor historyProcessor;

    public enum TransformationId
    {
        Standard_RemoveNotesVertices,
        Standard_SupportRenderConfigVerticesAndRemove,
        Standard_SupportParentAlias,
        Standard_SupportEntryExit,
        Standard_SupportPrefixingModder,
        /// <summary>
        /// https://github.com/StateSmith/StateSmith/issues/138
        /// </summary>
        Standard_NameConflictResolution,
        Standard_SupportHistory,
        /// <summary>
        /// https://github.com/StateSmith/StateSmith/issues/136
        /// </summary>
        Standard_SupportElseGuard,
        Standard_SupportOrderAndElse,
        /// <summary>
        /// See https://github.com/StateSmith/StateSmith/issues/108
        /// </summary>
        Standard_SupportAlternateTriggers,
        Standard_Validation1,
        Standard_DefaultUnspecifiedEventsAsDoEvent,
        Standard_AddUsedEventsToSm,
        Standard_FinalValidation,
    };

    // this ctor used for Dependency Injection
    public StandardSmTransformer(RenderConfigVerticesProcessor renderConfigVerticesProcessor, HistoryProcessor historyProcessor, StateNameConflictResolver nameConflictResolver)
    {
        AddStep(TransformationId.Standard_RemoveNotesVertices, (sm) => NotesProcessor.Process(sm));
        AddStep(TransformationId.Standard_SupportRenderConfigVerticesAndRemove, (sm) => renderConfigVerticesProcessor.Process());
        AddStep(TransformationId.Standard_SupportParentAlias, (sm) => ParentAliasStateProcessor.Process(sm));
        AddStep(TransformationId.Standard_SupportEntryExit, (sm) => EntryExitProcessor.Process(sm));
        AddStep(TransformationId.Standard_SupportPrefixingModder, (sm) => PrefixingModder.Process(sm)); // must happen before name conflict resolution
        AddStep(TransformationId.Standard_NameConflictResolution, (sm) => nameConflictResolver.ResolveNameConflicts(sm)); // must happen before supporting history https://github.com/StateSmith/StateSmith/issues/168
        AddStep(TransformationId.Standard_SupportHistory, (sm) => historyProcessor.Process(sm));
        AddStep(TransformationId.Standard_SupportElseGuard, (sm) => ElseGuardProcessor.Process(sm)); // must happen before ordering step
        AddStep(TransformationId.Standard_SupportOrderAndElse, (sm) => OrderAndElseProcessor.Process(sm)); // should happen after most steps as it orders behaviors
        AddStep(TransformationId.Standard_SupportAlternateTriggers, (sm) => SupportAlternateTriggersProcessor.Process(sm));
        AddStep(TransformationId.Standard_Validation1, (sm) => Validate(sm));
        AddStep(TransformationId.Standard_DefaultUnspecifiedEventsAsDoEvent, (sm) => DefaultToDoEventVisitor.Process(sm));
        AddStep(TransformationId.Standard_AddUsedEventsToSm, (sm) => AddUsedEventsToSmClass.Process(sm));
        AddStep(TransformationId.Standard_FinalValidation, (sm) => Validate(sm));
        this.historyProcessor = historyProcessor;
    }

    private void AddStep(TransformationId id, Action<StateMachine> action)
    {
        transformationPipeline.Add(new TransformationStep(id, action));
    }

    public static void Validate(StateMachine sm)
    {
        var validator = new SpecificVertexValidator();
        sm.Accept(validator);
        var validator2 = new VertexValidator();
        sm.Accept(validator2);
    }
}
