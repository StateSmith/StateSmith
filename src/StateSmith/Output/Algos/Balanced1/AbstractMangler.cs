#nullable enable


namespace StateSmith.Output.Algos.Balanced1;

public abstract class AbstractMangler : NameMangler
{
    /// <summary><inheritdoc/></summary>
    public override string SmStartFuncName => MangleFuncName(base.SmStartFuncName);

    /// <summary><inheritdoc/></summary>
    public override string SmFuncCtor => MangleFuncName(base.SmFuncCtor);

    /// <summary><inheritdoc/></summary>
    public override string SmDispatchEventFuncName => MangleFuncName(base.SmDispatchEventFuncName);

    /// <summary><inheritdoc/></summary>
    public override string SmEventIdToStringFuncName => MangleFuncName(base.SmEventIdToStringFuncName);

    /// <summary><inheritdoc/></summary>
    public override string SmStateIdToStringFuncName => MangleFuncName(base.SmStateIdToStringFuncName);

    /// <summary><inheritdoc/></summary>
    public override string SmExitUpToFuncName => MangleFuncName(base.SmExitUpToFuncName);

    public override string SmStateIdVarName => MangleVarName(base.SmStateIdVarName);
    public override string SmAncestorEventHandlerVarName => MangleVarName(base.SmAncestorEventHandlerVarName);
    public override string SmCurrentEventHandlersVarName => MangleVarName(base.SmCurrentEventHandlersVarName);
    public override string SmCurrentStateExitHandlerVarName => MangleVarName(base.SmCurrentStateExitHandlerVarName);

    public abstract string MangleFuncName(string originalName);
}
