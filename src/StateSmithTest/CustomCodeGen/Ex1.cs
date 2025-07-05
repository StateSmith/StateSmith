using StateSmith.Output;
using StateSmith.Runner;
using System.IO;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace StateSmithTest.CustomCodeGen;

public class Ex1
{
    // Support custom user code generation strategies: https://github.com/StateSmith/StateSmith/issues/89
    [Fact]
    public void ExampleCustomCodeGen()
    {
        // settings for your custom code generator
        MyCodeGenSettings myCodeGenSettings = new(outputFileName: "my_output.txt");

        var spBuilder = DefaultServiceProviderBuilder.CreateDefault((services) =>
        {
        // register your custom code generator (and any custom dependencies) for Dependency Injection
            services.AddSingleton<ICodeGenRunner, MyCodeGenRunner>();
            services.AddSingleton(myCodeGenSettings);
        });

        SmRunner runner = new(diagramPath: "Ex1.drawio.svg", serviceProvider: spBuilder.Build());

        // adjust settings because we are unit testing. Normally wouldn't do below.
        runner.Settings.propagateExceptions = true;
        runner.Settings.outputDirectory = Path.GetTempPath();

        // run StateSmith with your custom code generator!!!
        runner.Run();

        // Test that we saw the expected output from your custom code generator.
        string expectedFilePath = Path.GetTempPath() + "/" + "my_output.txt";
        File.ReadAllText(expectedFilePath).ShouldBeShowDiff("""
            State machine name: Ex1
            Child count: 3

            """);        
    }

    /// <summary>
    /// This class is needed so that MyCodeGenRunner can get your custom settings from dependency injection.
    /// You don't need this if you don't want.
    /// </summary>
    class MyCodeGenSettings
    {
        public string outputFileName;

        public MyCodeGenSettings(string outputFileName)
        {
            this.outputFileName = outputFileName;
        }
    }

    /// <summary>
    /// See <see cref="CodeGenRunner"/> for the StateSmith C99 code gen runner.
    /// 
    /// This example code generator simply writes a bit of info about a state machine to a text file.
    /// </summary>
    class MyCodeGenRunner : ICodeGenRunner
    {
        readonly MyCodeGenSettings mySettings;
        readonly StateMachineProvider stateMachineProvider;
        readonly RunnerSettings runnerSettings;

        // Called by dependency injection. Handy because you can easily get whatever you need for your code gen by simply
        // adding it to this constructor.
        public MyCodeGenRunner(StateMachineProvider stateMachineProvider, MyCodeGenSettings settings, RunnerSettings runnerSettings)
        {
            this.stateMachineProvider = stateMachineProvider;
            this.mySettings = settings;
            this.runnerSettings = runnerSettings;
        }

        /// <summary>
        /// See <see cref="CodeGenRunner.Run()"/> for a real example of things you would actually want to do.
        /// </summary>
        public void Run()
        {
            var sm = stateMachineProvider.GetStateMachine();

            var outputString = $"State machine name: {sm.Name}\n"
                + $"Child count: {sm.Children.Count}\n";

            string path = runnerSettings.outputDirectory + mySettings.outputFileName;
            File.WriteAllText(path, outputString);
        }
    }
}
