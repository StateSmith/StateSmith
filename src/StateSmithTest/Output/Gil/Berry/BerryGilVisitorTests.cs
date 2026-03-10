#nullable enable

using System.Text;
using FluentAssertions;
using StateSmith.Output;
using StateSmith.Output.Gil;
using StateSmith.Output.Gil.Berry;
using StateSmith.Output.UserConfig;
using Xunit;

namespace StateSmithTest.Output.Gil.Berry;

public class BerryGilVisitorTests
{
    [Fact]
    public void Process_IncludesImportsAndClassCode()
    {
        string gilCode = """
            public class ____GilData_FileTop { }
            public class SimpleSm
            {
                public enum EventId
                {
                    DO = 0,
                    EV1 = 1,
                }

                public void Start()
                {
                }
            }
            """;

        var berryConfig = new RenderConfigBerryVars
        {
            Imports = "import string",
            ClassCode = "def helper(): pass"
        };

        string result = Transpile(gilCode, berryConfig: berryConfig);

        result.Should().Contain("import string");
        result.Should().Contain("class SimpleSm");
        result.Should().Contain("def helper()");
        result.Should().Contain("return SimpleSmModule");
    }

    [Fact]
    public void Process_RendersSwitchAsIfChain()
    {
        string gilCode = """
            public class ____GilData_FileTop { }
            public class SimpleSm
            {
                public enum EventId { DO = 0, EV1 = 1 }
                public StateId stateId;

                public enum StateId { ROOT = 0, S1 = 1 }

                public void DispatchEvent(EventId eventId)
                {
                    switch (stateId)
                    {
                        case StateId.ROOT:
                            switch (eventId)
                            {
                                case EventId.DO:
                                    break;
                            }
                            break;
                    }
                }
            }
            """;

        string result = Transpile(gilCode);

        result.Should().Contain("def DispatchEvent(eventId)");
        result.Should().Contain("if self.stateId == SimpleSm.StateId.ROOT");
    }

    private static string Transpile(string gilCode, RenderConfigBerryVars? berryConfig = null, RenderConfigBaseVars? baseConfig = null)
    {
        berryConfig ??= new RenderConfigBerryVars();
        baseConfig ??= new RenderConfigBaseVars();
        var sb = new StringBuilder();
        var visitor = new BerryGilVisitor(gilCode, sb, berryConfig, baseConfig, new RoslynCompiler(), new CodeStyleSettings(), moduleName: "SimpleSm");
        visitor.Process();
        return sb.ToString();
    }
}
