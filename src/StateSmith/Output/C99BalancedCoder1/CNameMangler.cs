using StateSmith.Output.Algos.Balanced1;
using StateSmith.SmGraph;

namespace StateSmith.Output.C99BalancedCoder1;

// fixme - move to gil name mangler?

public class CNameMangler : NameMangler
{
    public CNameMangler()
    {
        
    }

    public CNameMangler(StateMachine sm) : base(sm) { }

    public virtual string CFileName => $"{Sm.Name}.c";
    public virtual string HFileName => $"{Sm.Name}.h";

    /// <summary>
    /// Ctor is short for constructor
    /// </summary>
    public virtual string SmFuncCtor => $"ctor"; // this needs to be changed
}
