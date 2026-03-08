using StateSmithTest.Processes;
using Xunit;

// This file is temporary.
// Will remove once we have swift MR merged

namespace StateSmithTest.spec2.fake;

public abstract class FakeSpec2Tests
{
    public FakeSpec2Tests()
    {
        Setup();
    }

    public virtual void Setup()
    {

    }

    [Fact]
    public void AssertNoCrash()
    {
        Assert.Equal(1, 1);
    }
}

public class Spec2TestsSwift : FakeSpec2Tests
{
    public override void Setup()
    {
        SimpleProcess process = new()
        {
            ProgramPath = "swiftccc",
            Args = " --version",
            throwOnStdErr = false   // required for github runner
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }
}

public class Spec2TestsKotlin : FakeSpec2Tests
{
    public override void Setup()
    {
        SimpleProcess process = new()
        {
            ProgramPath = "kotlinc",
            Args = " -version",
            throwOnStdErr = false   // `kotlinc -version` prints to stderr so this is normal
        };
        process.Run(timeoutMs: SimpleProcess.DefaultLongTimeoutMs);
    }
}
