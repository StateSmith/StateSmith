using System.Collections.Generic;

namespace StateSmithTest;

public class VertexTestHelper
{
    public static List<string> TriggerList(params string[] triggers)
    {
        return new List<string>(triggers);
    }
}
