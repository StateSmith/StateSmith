#nullable enable

namespace StateSmith.Output.Gil;

internal class AtomicInt
{
    private int _value;
    private readonly object _lockObject = new object();

    public AtomicInt(int value = 0)
    {
        _value = value;
    }

    public int IncrementAndGet()
    {
        int resultValue;
        
        lock (_lockObject)
        {
            _value++;
            resultValue = _value;
        }

        return resultValue;
    }
}
