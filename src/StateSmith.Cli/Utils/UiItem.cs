namespace StateSmith.Cli.Utils;

public class UiItem<T> : ICloneable<UiItem<T>> where T : notnull
{
    public UiItem(T id, string display)
    {
        Id = id;
        Display = display;
        if (display == "")
            Display = id.ToString()!;
    }

    public UiItem(T id) { Id = id; Display = id.ToString()!; }

    public T Id { get; set; }
    public string Display { get; set; }
    public UiItem<T> Clone() => new(Id, Display);
}

