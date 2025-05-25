using Xunit;
using StateSmith.Runner;
using System.Diagnostics;
using System.IO;
using System;

namespace StateSmithTest.Output.Java;

public class SimpleJava_Test()
{
    [Fact]
    public void SimpleJavaSmCompiles()
    {
        var plantUmlText = """"
            @startuml MySm
            [*] -> Off
            Off -> On : Switch
            On -> Off : Switch
            @enduml
            """";

        var className = "MySm";
        var tmpDir = Directory.CreateTempSubdirectory().FullName;
        var javaPath = Path.Combine(tmpDir, $"{className}.java");
        var diagramPath = Path.Combine(tmpDir, Path.GetRandomFileName() + ".plantuml");

        File.WriteAllText(diagramPath, plantUmlText);
        SmRunner smRunner = new(diagramPath: diagramPath, renderConfig: null, transpilerId: StateSmith.Runner.TranspilerId.Java, algorithmId: AlgorithmId.Default);
        smRunner.Run();

        // Read the generated Java file content
        string generatedCode = File.ReadAllText(javaPath);

        // Construct the expected constructor signature
        // It should be public <ClassName>(<ClassName>Delegate delegate)
        // For example: public MySm(MySmDelegate delegate)
        // A simple string contains might be too brittle. Let's try with a regex that's a bit flexible.
        string expectedConstructorPattern = $@"public\s+{className}\s*\(\s*{className}Delegate\s+delegate\s*\)";

        // Assert that the constructor exists
        bool constructorExists = System.Text.RegularExpressions.Regex.IsMatch(generatedCode, expectedConstructorPattern);
        Xunit.Assert.True(constructorExists, $"Generated Java code for class '{className}' does not contain the expected constructor: 'public {className}({className}Delegate delegate)'.\nActual code:\n{generatedCode}"); // Changed to Xunit.Assert

        // Generate the empty delegate class
        var delegateText = $"public class {className}Delegate {{}}";
        var delegatePath = Path.Combine(tmpDir,$"{className}Delegate.java");
        File.WriteAllText(delegatePath, delegateText);

        SuccessfullyCompiles(tmpDir, [javaPath, delegatePath]);

        // Only delete on successful completion of test, to aid debugging
        Directory.Delete(tmpDir, true);
    }

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
