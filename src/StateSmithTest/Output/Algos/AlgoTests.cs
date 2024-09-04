using FluentAssertions;
using StateSmith.Runner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace StateSmithTest.Output.Algos;

public class AlgoTests
{
    [Fact]
    public void Balanced1()
    {
        string cCode = GenerateCForAlgo(AlgorithmId.Balanced1);
        cCode.Should().Contain("current_state_exit_handler = ROOT_exit;");
        cCode.Should().NotContain("switch (sm->state_id)");
    }

    [Fact]
    public void Balanced2()
    {
        string cCode = GenerateCForAlgo(AlgorithmId.Balanced2);
        cCode.Should().NotContain("current_state_exit_handler");
        cCode.Should().Contain("switch (sm->state_id)");
    }

    [Fact]
    public void IntegrationTests()
    {
        var outDir = TestHelper.GetThisDir() + "/out";
        Directory.Delete(outDir, recursive: true);

        foreach (var algoId in GetValues<AlgorithmId>())
        {
            foreach (var transpilerId in GetValues<TranspilerId>())
            {
                if (transpilerId == TranspilerId.NotYetSet)
                    continue;

                var dirName = $"{outDir}/{algoId}_{transpilerId}";
                Directory.CreateDirectory(dirName);
                TestHelper.RunSmRunnerForPlantUmlString(outputDir: dirName, algorithmId: algoId, transpilerId: transpilerId);
            }
        }

        // https://stackoverflow.com/a/972323
        static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

    }


    private static string GenerateCForAlgo(AlgorithmId algo)
    {
        var fakeFs = new CapturingCodeFileWriter();
        var console = new StringBufferConsolePrinter();

        TestHelper.CaptureRunSmRunnerForPlantUmlString(codeFileWriter: fakeFs, consoleCapturer: console, algorithmId: algo);
        var cCode = fakeFs.GetSoleCaptureWithName("RocketSm.c").code;
        return cCode;
    }
}

