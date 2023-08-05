#nullable enable

namespace StateSmith.Runner;

//internal class Item : Item<string>
//{
//    public Item(string id, string display) : base(id, display)
//    {
//    }
//}

internal class Item<T> : ICloneable<Item<T>> where T : notnull
{
    public Item(T id, string display)
    {
        Id = id;
        Display = display;
        if (display == "")
            Display = id.ToString()!;
    }

    public Item(T id) { Id = id; Display = id.ToString()!; }

    public T Id { get; set; }
    public string Display { get; set; }
    public Item<T> Clone() => new(Id, Display);
}

