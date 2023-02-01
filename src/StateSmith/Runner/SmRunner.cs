using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using System;
using StateSmith.Compiling;
using System.IO;
using StateSmith.Common;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

[assembly: CLSCompliant(false)]

namespace StateSmith.Runner;

/// <summary>
/// Builds a single state machine and runs code generation.
/// </summary>
public class SmRunner
{
    private const string ExpansionVarsPath = "self->vars.";
    readonly RunnerSettings settings;
    InputSmBuilder inputSmBuilder;
    ExceptionPrinter exceptionPrinter;

    public SmRunner(RunnerSettings settings)
    {
        this.settings = settings;
        var sp = new SsServiceProvider(postConfigAction: (context, services) =>
        {
            services.AddSingleton(settings.drawIoSettings);
            services.AddSingleton(settings.mangler);
        });

        inputSmBuilder = new(sp);
        exceptionPrinter = settings.exceptionPrinter;
    }

    /// <summary>
    /// Publicly exposed so that users can customize transformation behavior
    /// </summary>
    public SmTransformer SmTransformer => inputSmBuilder.transformer;

    public void Run()
    {
        try
        {
            System.Console.WriteLine();
            RunInner();
            OutputStageMessage("Finished normally.");
        }
        catch (System.Exception e)
        {
            if (settings.propagateExceptions)
            {
                throw;
            }

            Environment.ExitCode = -1; // lets calling process know that code gen failed

            exceptionPrinter.PrintException(e);
            DumpErrorDetailsToFile(e);
            OutputStageMessage("Finished with failure.");
        }

        System.Console.WriteLine();
    }

    protected void RunInner()
    {
        RunCompiler();
        RunCodeGen();
    }

    protected void RunCodeGen()
    {
        StateMachine sm = inputSmBuilder.Sm.ThrowIfNull();
        CodeGenContext codeGenContext = new(sm, settings.renderConfig);
        codeGenContext.mangler = settings.mangler;
        codeGenContext.style = settings.style;

        settings.mangler.SetStateMachine(sm);

        // todo_low - move into state machine code generation once dependency injection makes CodeGenContext available
        foreach (var h in sm.historyStates)
        {
            string actualVarName = codeGenContext.mangler.HistoryVarName(h);
            codeGenContext.expander.AddVariableExpansion(h.stateTrackingVarName, ExpansionVarsPath + actualVarName);

            bool useU8 = false;

            if (useU8)
            {
                codeGenContext.sm.variables += $"uint8_t {actualVarName};\n";
                if (h.Behaviors.Count > 255) {
                    throw new VertexValidationException(h, "can't support more than 255 tracked history states right now."); //uint8_t limitation.
                }
            }
            else
            {
                codeGenContext.sm.variables += $"enum {settings.mangler.HistoryVarEnumName(h)} {actualVarName};\n";
            }
        }

        ConfigReader reader = new(codeGenContext.expander, expansionVarsPath: ExpansionVarsPath);
        reader.ReadObject(settings.renderConfig);

        CBuilder cBuilder = new(codeGenContext);
        cBuilder.Generate();

        CHeaderBuilder cHeaderBuilder = new(codeGenContext);
        cHeaderBuilder.Generate();

        string hFileContents = codeGenContext.hFileSb.ToString();
        string cFileContents = codeGenContext.cFileSb.ToString();

        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.HFileName}", hFileContents);
        File.WriteAllText($"{settings.outputDirectory}{settings.mangler.CFileName}", cFileContents);
    }

    private void OutputCompilingDiagramMessage()
    {
        // https://github.com/StateSmith/StateSmith/issues/79
        string filePath = settings.diagramFile;
        if (settings.filePathPrintBase.Trim().Length > 0)
        {
            filePath = Path.GetRelativePath(settings.filePathPrintBase, settings.diagramFile);
        }
        filePath = filePath.Replace('\\', '/');

        OutputStageMessage($"Compiling file: `{filePath}` "
            + ((settings.stateMachineName == null) ? "(no state machine name specified)" : $"with target state machine name: `{settings.stateMachineName}`")
            + "."
        );
    }

    private void FindStateMachine()
    {
        if (settings.stateMachineName != null)
        {
            inputSmBuilder.FindStateMachineByName(settings.stateMachineName);
        }
        else
        {
            inputSmBuilder.FindSingleStateMachine();
        }

        OutputStageMessage($"State machine `{inputSmBuilder.Sm!.Name}` selected.");
    }

    private void RunCompiler()
    {
        OutputCompilingDiagramMessage();
        inputSmBuilder.ConvertDiagramFileToSmVertices(settings.diagramFile);
        FindStateMachine();
        inputSmBuilder.FinishRunningCompiler();
    }

    protected void OutputStageMessage(string message)
    {
        // todo_low add logger functionality
        Console.WriteLine("StateSmith Runner - " + message);
    }

    private void DumpErrorDetailsToFile(Exception e)
    {
        var errorDetailFilePath = settings.diagramFile + ".err.txt";
        //errorDetailFilePath = Path.GetFullPath(errorDetailFilePath); // if you want the full path but with resolved "../../"
        errorDetailFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), errorDetailFilePath);
        exceptionPrinter.DumpExceptionDetails(e, errorDetailFilePath);
        Console.Error.WriteLine("Additional exception detail dumped to file: " + errorDetailFilePath);
    }
}
