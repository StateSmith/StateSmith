using StateSmith.Input.Expansions;
using StateSmith.output;
using StateSmith.output.C99BalancedCoder1;
using StateSmith.output.UserConfig;
using StateSmith.Runner;
using System;

public class CodeGen
{
    public static void Main()
    {
        var srcDirectory = AppDomain.CurrentDomain.BaseDirectory + "../../../../src/";
        var diagramFile = srcDirectory + "Tutorial1Sm.graphml";

        MyGlueFile myGlueFile = new();
        RunnerSettings settings = new(myGlueFile, diagramFile: diagramFile, outputDirectory: srcDirectory);
        settings.mangler = new MyMangler();
        settings.style = new MyStyler();

        SmRunner runner = new(settings);
        runner.Run();
    }


    /// <summary>
    /// This class gives StateSmith the info it needs to generate working C code.
    /// It adds user code to the generated .c/.h files, declares user variables,
    /// and provides diagram code expansions.
    /// </summary>
    public class MyGlueFile : IRenderConfigC
    {
        // Anything you type in the below string ends up in the generated h file
        string IRenderConfigC.HFileIncludes => StringUtils.DeIndentTrim(@"
                // Some user .h file comment...
                #include <stdint.h>
            ");

        // Anything you type in the below string ends up in the generated c file
        string IRenderConfigC.CFileIncludes => StringUtils.DeIndentTrim(@"
                // Some user .c file comment...
                #include ""light.h""
            ");

        // Anything you type in the below string ends up in the state machine user variables section.
        // If the string is blank, then no user variables section is created.
        string IRenderConfigC.VariableDeclarations => StringUtils.DeIndentTrim(@"
                uint8_t count;  // some user description for count field
            ");

        /// <summary>
        /// This nested class creates expansions because it is inside MyGlueFile.
        /// </summary>
        public class Expansions : UserExpansionScriptBase
        {
            #pragma warning disable IDE1006 // Naming Styles
            string count => AutoVarName();

            // If you want to debug an expansion that uses a C# property like `string count => AutoVarName();`
            // you can write it in the old school way of implementing a C# property like below.
            // This gives the ability to easily add breakpoints.
            // string count {
            //     get {
            //         var text = AutoVarName();
            //         return text;
            //     }
            // } 
            #pragma warning restore IDE1006 // Naming Styles
        }
    }


    /// <summary>
    /// This class mangles names. If you would like to customize the generated code names,
    /// here is where you do it. Simply override the relevant method.
    /// </summary>
    class MyMangler : CNameMangler
    {
        // public override string SmEventEnum => $"{SmName}_event_id";
        // public override string SmEventEnumValue(string evt) => $"{SmName.ToUpper()}_EVENT_ID_{evt.ToUpper()}";
        // public override string SmEventEnumCount => $"{SmName.ToUpper()}_EVENT_ID_COUNT";

        // public override string SmStateEnum => $"{SmName}_state_id";
        // public override string SmStateEnumCount => $"{SmName.ToUpper()}_STATE_ID_COUNT";
        // public override string SmStateEnumValue(NamedVertex namedVertex)
        // {
        //     string stateName = SmStateName(namedVertex);
        //     return $"{SmName.ToUpper()}_STATE_ID_{stateName.ToUpper()}";
        // }

        // public override string SmFuncTypedef => $"{SmStructName}_func";
    }

    class MyStyler : CodeStyleSettings
    {
        // public override bool BracesOnNewLines => false;
        // public override string Indent1 => "  "; 
    }
}
