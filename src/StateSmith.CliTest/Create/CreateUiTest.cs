using FluentAssertions;
using Spectre.Console.Testing;
using StateSmith.Cli.Create;
using Xunit;

namespace StateSmithCliTest.Create;

public class CreateUiTest
{
    private CreateUi cUi;
    private TestConsole fakeConsole;

    public CreateUiTest()
    {
        fakeConsole = new TestConsole();
        cUi = new CreateUi(fakeConsole, new(fakeConsole));
        //fakeConsole.Input.PushTextWithEnter(""); // get past the first prompt
    }

    [Fact]
    public void ScriptFileName()
    {
        fakeConsole.Input.PushTextWithEnter("RocketSm.csx");
        cUi.ScriptFileName();
        cUi._settings.scriptFileName.Should().Be("RocketSm.csx");
    }

    [Fact]
    public void ScriptFileNameDoubleDollarSign()
    {
        cUi._settings.smName = "LazyCatSm";

        fakeConsole.Input.PushTextWithEnter("../$$");
        cUi.ScriptFileName();
        cUi._settings.scriptFileName.Should().Be("../LazyCatSm.csx");
    }

    [Fact]
    public void DiagramFileName()
    {
        fakeConsole.Input.PushTextWithEnter("RocketSm.drawio");
        cUi.DiagramFileName();
        cUi._settings.diagramFileName.Should().Be("RocketSm.drawio");
    }

    [Fact]
    public void DiagramFileNameDoubleDollarSign()
    {
        cUi._settings.FileExtension = ".drawio";
        cUi._settings.smName = "LazyCatSm";

        fakeConsole.Input.PushTextWithEnter("../$$");
        cUi.DiagramFileName();
        cUi._settings.diagramFileName.Should().Be("../LazyCatSm.drawio");
    }
}
