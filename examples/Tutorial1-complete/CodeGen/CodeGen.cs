using StateSmith.Input.Expansions;
using StateSmith.Output;
using StateSmith.Output.Algos.Balanced1;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;
using System;

public class CodeGen
{
    public static void Main()
    {
        string extension = ".drawio.svg";  // For draw.io https://github.com/StateSmith/StateSmith/issues/77
        // extension = ".drawio";          // Another format for draw.io https://github.com/StateSmith/StateSmith/issues/77
        // extension = ".graphml";         // For yEd editor.

        MyGlueFile myGlueFile = new();
        SmRunner runner = new(diagramPath: "../src/Tutorial1Sm" + extension, myGlueFile);
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
        string IRenderConfigC.HFileIncludes => @"
                // Some user .h file comment...
            ";

        // Anything you type in the below string ends up in the generated c file
        string IRenderConfigC.CFileIncludes => @"
                // Some user .c file comment...
                #include ""light.h""
            ";

        // Anything you type in the below string ends up in the state machine user variables section.
        // If the string is blank, then no user variables section is created.
        string IRenderConfig.VariableDeclarations => @"
                uint8_t count;  // some user description for count field
            ";

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
}
