#nullable enable

using StateSmith.Common;
using StateSmith.Output;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace StateSmith.SmGraph;

public class Behavior
{
    public const double DEFAULT_ORDER = 1e6;
    public const double ELSE_ORDER = DEFAULT_ORDER * 10;

    /// <summary>
    /// Only populated for transitions.
    /// </summary>
    public string? DiagramId { get; set; }

    internal Vertex _owningVertex;
    public Vertex OwningVertex => _owningVertex;

    /// <summary>
    /// Allowed to be null
    /// </summary>
    internal Vertex? _transitionTarget;

    public Vertex? TransitionTarget => _transitionTarget;

    /// <summary>
    /// Modifiable triggers list. Prefer using <see cref="Triggers"/> if you don't need to modify the list.
    /// May be unsanitized. See <see cref="TriggerHelper.SanitizeTriggerName(string)"/>.
    /// </summary>
    public List<string> _triggers = new();

    /// <summary>
    /// Readonly list of triggers.
    /// May be unsanitized. See <see cref="TriggerHelper.SanitizeTriggerName(string)"/> or <see cref="SanitizedTriggers"/>.
    /// </summary>
    public IReadOnlyList<string> Triggers => _triggers;

    /// <summary>
    /// You can convert this to a regular list with System.Linq: <code>behavior.SanitizedTriggers.ToList()</code>
    /// </summary>
    public IEnumerable<string> SanitizedTriggers => _triggers.Select(t => TriggerHelper.SanitizeTriggerName(t));

    public double order = DEFAULT_ORDER;
    public string guardCode = "";
    public string actionCode = "";

    // https://github.com/StateSmith/StateSmith/issues/3
    public string? viaEntry;

    // https://github.com/StateSmith/StateSmith/issues/3
    public string? viaExit;

    public Behavior() {
        _owningVertex = new State("nullable_dummy_for_tests");  // todo_low: update test code to do this instead.
    }

    public static Behavior NewEnterBehavior(string actionCode = "")
    {
        return new Behavior(trigger: TriggerHelper.TRIGGER_ENTER, actionCode: actionCode);
    }

    public static Behavior NewExitBehavior(string actionCode = "")
    {
        return new Behavior(trigger: TriggerHelper.TRIGGER_EXIT, actionCode: actionCode);
    }

    public Behavior(string trigger, string guardCode = "", string actionCode = "") : this()
    {
        this._triggers.Add(trigger);
        this.guardCode = guardCode;
        this.actionCode = actionCode;
    }

    public Behavior(string guardCode = "", string actionCode = "", Vertex? transitionTarget = null) : this()
    {
        this.guardCode = guardCode;
        this.actionCode = actionCode;
        _MaybeSetTransitionTarget(transitionTarget);
    }

    public Behavior(Vertex owningVertex, Vertex? transitionTarget = null, string? diagramId = null)
    {
        _owningVertex = owningVertex;
        _MaybeSetTransitionTarget(transitionTarget);
        DiagramId = diagramId;
    }

    private void _MaybeSetTransitionTarget(Vertex? transitionTarget)
    {
        if (transitionTarget != null)
        {
            _transitionTarget = transitionTarget;
            transitionTarget.AddIncomingTransition(this);
        }
    }

    public string GetOrderString()
    {
        if (order == DEFAULT_ORDER)
        {
            return "default";
        }
        return order.ToString();
    }

    [MemberNotNullWhen(true, nameof(TransitionTarget))]
    public bool HasTransition()
    {
        return TransitionTarget != null;
    }

    [MemberNotNullWhen(true, nameof(guardCode))]
    public bool HasGuardCode()
    {
        return IsCodePresent(guardCode);
    }

    [MemberNotNullWhen(true, nameof(actionCode))]
    public bool HasActionCode()
    {
        return IsCodePresent(actionCode);
    }

    [MemberNotNullWhen(true, nameof(viaExit))]
    public bool HasViaExit()
    {
        return IsCodePresent(viaExit);
    }

    [MemberNotNullWhen(true, nameof(viaEntry))]
    public bool HasViaEntry()
    {
        return IsCodePresent(viaEntry);
    }

    private static bool IsCodePresent(string? code)
    {
        return !string.IsNullOrWhiteSpace(code);
    }

    public void AddTrigger(string trigger)
    {
        _triggers.Add(trigger);
    }

    public bool HasAtLeastOneTrigger()
    {
        return Triggers.Any();
    }

    /// <summary>
    /// If there are no triggers, then StateSmith assumes an implicit `do` event/trigger.
    /// </summary>
    /// <returns></returns>
    public bool HasImplicitDoTriggerOnly()
    {
        return HasAtLeastOneTrigger() == false;
    }

    [MemberNotNullWhen(true, nameof(TransitionTarget))]
    public bool IsBlankTransition()
    {
        return (HasTransition() && !HasAtLeastOneTrigger() && !HasGuardCode() && !HasActionCode() && order == DEFAULT_ORDER && !HasViaEntry() && !HasViaExit());
    }

    /// <summary>
    /// Must have had an original target
    /// </summary>
    /// <param name="newTarget"></param>
    public void RetargetTo(Vertex newTarget)
    {
        var oldTarget = TransitionTarget;
        oldTarget!.RemoveIncomingTransition(this);

        _transitionTarget = newTarget;
        newTarget.AddIncomingTransition(this);
    }

    /// <summary>
    /// Must have had an original target
    /// </summary>
    /// <param name="newOwner"></param>
    public void RetargetOwner(Vertex newOwner)
    {
        var oldOwner = OwningVertex;
        oldOwner._behaviors.Remove(this);
        newOwner.AddBehavior(this);
    }

    /// <summary>
    /// Throws if no transition target
    /// </summary>
    /// <returns></returns>
    public TransitionPath FindTransitionPath()
    {
        return OwningVertex.FindTransitionPathTo(TransitionTarget.ThrowIfNull());
    }

    public string DescribeAsUml(bool singleLineFormat = true)
    {
        return new BehaviorDescriber(singleLineFormat).Describe(this);
    }

    public string GetSingleLineGuardCode()
    {
        return BehaviorDescriber.MakeSingleLineCode(guardCode);
    }

    public string GetSingleLineActionCode()
    {
        return BehaviorDescriber.MakeSingleLineCode(actionCode);
    }

    public static void RemoveBehaviorsAndUnlink(IEnumerable<Behavior> behaviors)
    {
        foreach (var behavior in behaviors)
        {
            behavior.OwningVertex.RemoveBehaviorAndUnlink(behavior);
        }
    }

    public override string ToString()
    {
        return DescribeAsUml();
    }
}
