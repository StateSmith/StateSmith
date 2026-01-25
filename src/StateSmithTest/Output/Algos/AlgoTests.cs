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
    public void IntegrationTests_SingleEvent_C99_CPP_CSharp_Javascript_Java_Python_TypeScript()
    {
        const string basicSingleEventPlantUml = """
            @startuml RocketSm
            [*] --> group
            state group {
                [*] --> g1
                g1 --> g2
                g2 --> g1: [x > 50]
            }
            @enduml
            """;

        RunIntegrationTestMatrix(basicSingleEventPlantUml, "out");
    }

    [Fact]
    public void IntegrationTests_MultipleEvent_C99_CPP_CSharp_Javascript_Java_Python_TypeScript()
    {
        RunIntegrationTestMatrix(allLangsMultipleEvents, "out2");
    }

    private static void RunIntegrationTestMatrix(string MinimalPlantUmlFsm, string outDirName)
    {
        var outDir = TestHelper.GetThisDir() + "/" + outDirName;
        if (Directory.Exists(outDir))
        {
            Directory.Delete(outDir, recursive: true);
        }

        foreach (var algoId in GetValues<AlgorithmId>())
        {
            foreach (var transpilerId in GetValues<TranspilerId>())
            {
                if (transpilerId == TranspilerId.NotYetSet)
                    continue;

                // Java only supports Balanced2 right now. See https://github.com/StateSmith/StateSmith/issues/395
                if (transpilerId == TranspilerId.Java && algoId != AlgorithmId.Balanced2)
                    continue;

                // Python only supports Balanced2 right now. See https://github.com/StateSmith/StateSmith/issues/398
                if (transpilerId == TranspilerId.Python && algoId != AlgorithmId.Balanced2)
                    continue;

                // Swift only supports Balanced2 right now. 
                if (transpilerId == TranspilerId.Swift && algoId != AlgorithmId.Balanced2)
                    continue;

                // TypeScript only supports Balanced2 right now. See https://github.com/StateSmith/StateSmith/issues/407
                if (transpilerId == TranspilerId.TypeScript && algoId != AlgorithmId.Balanced2)
                    continue;

                // Cpp only supports Balanced2 right now. See https://github.com/StateSmith/StateSmith/issues/411
                if (transpilerId == TranspilerId.Cpp && algoId != AlgorithmId.Balanced2)
                    continue;

                var dirName = $"{outDir}/{algoId}_{transpilerId}";
                Directory.CreateDirectory(dirName);
                TestHelper.RunSmRunnerForPlantUmlString(plantUmlText: MinimalPlantUmlFsm, outputDir: dirName, algorithmId: algoId, transpilerId: transpilerId);
            }
        }
    }

    private static string GenerateCForAlgo(AlgorithmId algo)
    {
        var fakeFs = new CapturingCodeFileWriter();
        var console = new StringBuilderConsolePrinter();

        TestHelper.CaptureRunSmRunnerForPlantUmlString(codeFileWriter: fakeFs, consoleCapturer: console, algorithmId: algo);
        var cCode = fakeFs.GetSoleCaptureWithName("RocketSm.c").code;
        return cCode;
    }

    // https://stackoverflow.com/a/972323
    static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    const string allLangsMultipleEvents = """"
        @startuml RocketSm
        [*] --> group
        state s1
            
        state group {
            state g1
            state g2
            [*] --> g1
        }

        g1 --> g2: EV1 [a > 20]
        g2 --> g1: EV2
        group --> s1: EV1
        
        /'! $CONFIG: toml

            [RenderConfig.C]
            CFileTop = """
                // RenderConfig.C.CFileTop
                """

            [RenderConfig.CSharp]
            ClassCode = """
                public void UserCodeMethod() {
                    Console.WriteLine('userCodeMethod');
                }
                """

            [RenderConfig.Java]
            ClassCode = """
                public void userCodeMethod() {
                    System.out.println('userCodeMethod');
                }
                """

            [RenderConfig.Python]
            ClassCode = """
                def userCodeMethod(self):
                    print('userCodeMethod')
                """

            [RenderConfig.JavaScript]
            ClassCode = """
                function userCodeMethod() {
                    console.log('userCodeMethod');
                }
                """

            [RenderConfig.Swift]
            ClassCode = """
                func userCodeMethod():
                    print("userCodeMethod")
                """

            [RenderConfig.TypeScript]
            ClassCode = """
                public userCodeMethod() {
                    console.log('userCodeMethod');
                }
                """
            Implements = "IRocketSm"
            Extends = "RocketSmBase"

        '/
        @enduml
        """";

}

