#nullable enable

using StateSmith.SmGraph;
using StateSmith.SmGraph.TriggerMap;
using StateSmith.SmGraph.Validation;
using System;

namespace StateSmith.Runner;

public class StandardSmTransformer : SmTransformer
{
    public enum TransformationId
    {
        Standard_RemoveNotesVertices,
        /// <summary>
        /// https://github.com/StateSmith/StateSmith/issues/335
        /// </summary>
        Standard_TomlConfig,
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
        /// <summary>
        /// https://github.com/StateSmith/StateSmith/issues/161
        /// </summary>
        Standard_TriggerMapping,
        Standard_AddUsedEventsToSm,
        Standard_FinalValidation,
    };

    // this ctor used for Dependency Injection
    public StandardSmTransformer(TomlConfigVerticesProcessor tomlConfigVerticesProcessor, RenderConfigVerticesProcessor renderConfigVerticesProcessor, HistoryProcessor historyProcessor, StateNameConflictResolver nameConflictResolver, TriggerMapProcessor triggerMapProcessor)
    {
        AddStep(TransformationId.Standard_RemoveNotesVertices, (sm) => NotesProcessor.Process(sm));
        AddStep(TransformationId.Standard_TomlConfig, (sm) => tomlConfigVerticesProcessor.Process(sm));
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
        AddStep(TransformationId.Standard_TriggerMapping, (sm) => triggerMapProcessor.Process(sm));
        AddStep(TransformationId.Standard_AddUsedEventsToSm, (sm) => AddUsedEventsToSmClass.Process(sm));
        AddStep(TransformationId.Standard_FinalValidation, (sm) => Validate(sm));
    }

    private void AddStep(TransformationId id, Action<StateMachine> action)
    {
        transformationPipeline.Add(new TransformationStep(id, action));
    }

    public static void Validate(StateMachine sm)
    {
        {
            var validator = new SpecificVertexValidator();
            sm.Accept(validator);
        }
        {
            var validator2 = new VertexValidator();
            sm.Accept(validator2);
        }
        {
            StateNameValidator stateNameValidator = new();
            stateNameValidator.Visit(sm);
        }
    }
}

/// <summary>
/// A marker class for a transformer that runs before the settings are applied.
/// </summary>
public class PreSettingsSmTransformer : StandardSmTransformer
{
    public PreSettingsSmTransformer(TomlConfigVerticesProcessor tomlConfigVerticesProcessor, RenderConfigVerticesProcessor renderConfigVerticesProcessor, HistoryProcessor historyProcessor, StateNameConflictResolver nameConflictResolver, TriggerMapProcessor triggerMapProcessor)
        : base(tomlConfigVerticesProcessor, renderConfigVerticesProcessor, historyProcessor, nameConflictResolver, triggerMapProcessor)
    {
        // Remove everything not needed for diagram settings reading.
        // We don't actually want to validate the diagram, just read the settings.
        // Why? Because it is slower and also we don't want to mess up designs that require special transformers.
        // If a user adds special transformers, they won't be added here as this is a brand new SmRunner and DI setup.
        // https://github.com/StateSmith/StateSmith/issues/349

        RemoveAfterFirstMatch(StandardSmTransformer.TransformationId.Standard_SupportRenderConfigVerticesAndRemove);
        
        // ensure that remove above didn't remove the toml config processor
        if (!HasMatch(StandardSmTransformer.TransformationId.Standard_TomlConfig))
        {
            throw new System.InvalidOperationException("Programming error. Standard_TomlConfig must be present.");
        }

        if (transformationPipeline.Count != 3)
        {
            throw new System.InvalidOperationException("Programming error. Expected only 3 steps in the pipeline.");
        }        
    }
}