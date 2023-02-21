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

    public virtual string CFileName => $"{SmName}.c";
    public virtual string HFileName => $"{SmName}.h";

    /// <summary>
    /// Ctor is short for constructor
    /// </summary>
    public virtual string SmFuncCtor => $"ctor"; // this needs to be changed
}
