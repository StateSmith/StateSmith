#nullable enable

namespace StateSmith.Output.Algos.Balanced1;

public class PascalFuncNameMangler : NameMangler
{
    /// <summary><inheritdoc/></summary>
    public override string SmStartFuncName => "Start";

    /// <summary><inheritdoc/></summary>
    public override string SmFuncCtor => "Ctor";

    /// <summary><inheritdoc/></summary>
    public override string SmDispatchEventFuncName => "DispatchEvent";

    /// <summary><inheritdoc/></summary>
    public override string SmEventIdToStringFuncName => "EventIdToString";

    /// <summary><inheritdoc/></summary>
    public override string SmStateIdToStringFuncName => "StateIdToString";

    /// <summary><inheritdoc/></summary>
    public override string SmExitUpToFuncName => "ExitUpToStateHandler";
}
