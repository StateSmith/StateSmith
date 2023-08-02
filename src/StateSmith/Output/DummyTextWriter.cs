#nullable enable

using System.IO;
using System.Text;

namespace StateSmith.Output;

internal class DummyTextWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

