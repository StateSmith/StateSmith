using Xunit;

namespace StateSmithTest.Input.DrawIo;

public class MultiplePagesTest
{
    [Fact]
    public void VertexTest()
    {
        TestHelper.CaptureSmRunnerFiles(diagramPath: "MultiplePages.drawio");
    }
}
