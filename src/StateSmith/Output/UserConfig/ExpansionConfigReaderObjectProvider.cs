namespace StateSmith.Output.UserConfig;

/// <summary>
/// This class just exists to add some typing. The config reader actually works
/// with any object, but that won't work with dependency injection so we wrap it.
/// </summary>
public class ExpansionConfigReaderObjectProvider
{
    public object obj;

    public ExpansionConfigReaderObjectProvider(object obj)
    {
        this.obj = obj;
    }
}

