using Xunit;
using Xunit.Abstractions;
using StateSmith.Runner;
using System.Diagnostics;
using System.IO;
using System;
using StateSmith.Output.UserConfig;

namespace StateSmithTest.Output.Java;

public class SimpleJava_Test(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void SimpleJavaSmCompiles()
    {
        var plantUmlText = """"
            @startuml Lightbulb
            [*] -> Off
            Off -> On : Switch
            On -> Off : Switch

            Off: enter / delegate.enterOff();
            On: enter / delegate.enterOn();
            @enduml
            """";

        var className = "Lightbulb";
        var tmpDir = Directory.CreateTempSubdirectory().FullName;
        var javaPath = Path.Combine(tmpDir, $"{className}.java");
        var diagramPath = Path.Combine(tmpDir, Path.GetRandomFileName() + ".plantuml");

        File.WriteAllText(diagramPath, plantUmlText);
        SmRunner smRunner = new(diagramPath: diagramPath, renderConfig: null, transpilerId: StateSmith.Runner.TranspilerId.Java, algorithmId: AlgorithmId.Default);
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>( new XUnitConsolePrinter(output));
        smRunner.Run();

        // Read the generated Java file content
        string generatedCode = File.ReadAllText(javaPath);

        // It should be public <ClassName>(<ClassName>Delegate delegate)
        string expectedConstructorPattern = $@"public\s+{className}\s*\(\s*{className}Delegate\s+delegate\s*\)";

        // Assert that the constructor exists
        bool constructorExists = System.Text.RegularExpressions.Regex.IsMatch(generatedCode, expectedConstructorPattern);
        Xunit.Assert.True(constructorExists, $"Generated Java code for class '{className}' does not contain the expected constructor: 'public {className}({className}Delegate delegate)'.\nActual code:\n{generatedCode}"); // Changed to Xunit.Assert

        // Add a simple Main app with the delegate class for validation
        var mainText = """
            public class MyApp {
                static LightbulbDelegate delegate = new LightbulbDelegate();
                static Lightbulb bulb = new Lightbulb(delegate);

                public static void main(String args[]) {
                    bulb.start();
                    bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
                }
            }

            class LightbulbDelegate {
                public void enterOn() {System.out.println("On");}
                public void enterOff() {System.out.println("Off");}
            }
        """;
        var mainPath = Path.Combine(tmpDir,"MyApp.java");
        File.WriteAllText(mainPath, mainText);

        SuccessfullyCompiles(tmpDir, [javaPath, mainPath]);

        // Only delete on successful completion of test, to aid debugging
        Directory.Delete(tmpDir, true);
    }


    [Fact]
    public void CanDisableDelegate()
    {
        var plantUmlText = """"
            @startuml Lightbulb
            [*] -> Off
            Off -> On : Switch
            On -> Off : Switch
            @enduml
            """";

        var className = "Lightbulb";
        var tmpDir = Directory.CreateTempSubdirectory().FullName;
        var javaPath = Path.Combine(tmpDir, $"{className}.java");
        var diagramPath = Path.Combine(tmpDir, Path.GetRandomFileName() + ".plantuml");


        File.WriteAllText(diagramPath, plantUmlText);
        IRenderConfigJava config = new NoDelegateConfig();
        SmRunner smRunner = new(diagramPath: diagramPath, renderConfig: config, transpilerId: StateSmith.Runner.TranspilerId.Java);
        smRunner.GetExperimentalAccess().DiServiceProvider.AddSingletonT<IConsolePrinter>( new XUnitConsolePrinter(output));
        smRunner.Run();

        // Read the generated Java file content
        string generatedCode = File.ReadAllText(javaPath);

        string unexpectedConstructorPattern = $@"public\s+{className}\s*\(\s*{className}Delegate\s+delegate\s*\)";

        // Assert that the constructor does not exist
        bool constructorExists = System.Text.RegularExpressions.Regex.IsMatch(generatedCode, unexpectedConstructorPattern);
        Xunit.Assert.False(constructorExists, $"Generated Java code for class '{className}' does not contain the expected constructor: 'public {className}({className}Delegate delegate)'.\nActual code:\n{generatedCode}"); // Changed to Xunit.Assert

        // Add a simple Main app to confirm delegate is not needed
        var mainText = """
            public class MyApp {
                static Lightbulb bulb = new Lightbulb();

                public static void main(String args[]) {
                    bulb.start();
                    bulb.dispatchEvent(Lightbulb.EventId.SWITCH);
                }
            }
        """;
        var mainPath = Path.Combine(tmpDir,"MyApp.java");
        File.WriteAllText(mainPath, mainText);

        SuccessfullyCompiles(tmpDir, [javaPath, mainPath]);

        // Only delete on successful completion of test, to aid debugging
        Directory.Delete(tmpDir, true);
    }    

    private class NoDelegateConfig : IRenderConfigJava {
        string IRenderConfig.NoDelegate => "true";
    };

    private static void SuccessfullyCompiles(string tmpDirname, string[] filePaths) {

        // Compile the generated Java code
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "javac",
                Arguments = String.Join(' ', filePaths),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = tmpDirname
            }
        };

        process.Start();
        process.WaitForExit();

        string stdOut = process.StandardOutput.ReadToEnd();
        string stdErr = process.StandardError.ReadToEnd();

        string debugMessage = $"Debug paths: args='{String.Join(' ', filePaths)}', workDir='{tmpDirname}'.\n";
        Xunit.Assert.True(process.ExitCode == 0, $"{debugMessage}javac failed with exit code {process.ExitCode}. StdOut: '{stdOut}'. StdErr: '{stdErr}'.");
    }
}
