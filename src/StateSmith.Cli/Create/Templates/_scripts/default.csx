#!/usr/bin/env dotnet-script
// If you have any questions about this file, check out https://github.com/StateSmith/tutorial-2
#r "nuget: StateSmith, {{stateSmithVersion}}"

using StateSmith.Common;
using StateSmith.Input.Expansions;
using StateSmith.Output.UserConfig;
using StateSmith.Runner;

// See https://github.com/StateSmith/tutorial-2/tree/main/lesson-1
SmRunner runner = new(diagramPath: "{{diagramPath}}", new MyRenderConfig(), transpilerId: TranspilerId.{{transpilerId}});
runner.Run();

// See https://github.com/StateSmith/tutorial-2/tree/main/lesson-2/ (basics)
// See https://github.com/StateSmith/tutorial-2/tree/main/lesson-5/ (language specific options)
public class MyRenderConfig : {{renderConfigBase}}
{
    //!!<filter:CppC>
    // NOTE!!! Idiomatic C++ code generation is coming. This will improve.
    // See https://github.com/StateSmith/StateSmith/issues/126
    string IRenderConfigC.CFileExtension => ".cpp"; // the generated StateSmith C code is also valid C++ code
    string IRenderConfigC.HFileExtension => ".h";   // could also be .hh, .hpp or whatever you like

    //!!</filter>



    string IRenderConfig.FileTop => """
        "use strict";    //!!<line-filter:JavaScript>
        // Whatever you put in the IRenderConfig.FileTop section ends up at the top of the generated file(s).
        """;



    //!!<filter:C,CppC>

    string IRenderConfig.AutoExpandedVars => """
        int auto_expanded_user_example_var;
        """;

    string IRenderConfig.VariableDeclarations => """
        int non_expanded_user_example_var;
        """;

    string IRenderConfigC.HFileTop => """
        // user IRenderConfigC.HFileTop: whatever you want to put in here.
        """;

    string IRenderConfigC.CFileTop => """
        // user IRenderConfigC.CFileTop: whatever you want to put in here.
        //#include <stdio.h> // or whatever you need
        """;

    string IRenderConfigC.HFileIncludes => """
        // user IRenderConfigC.HFileIncludes: whatever you want to put in here.
        """;

    string IRenderConfigC.CFileIncludes => """
        // user IRenderConfigC.CFileIncludes: whatever you want to put in here.
        """;

    // Optional: customize how enumerations are declared so that gcc will use the smallest possible int type instead of a full int.
    // string IRenderConfigC.CEnumDeclarer => "typedef enum __attribute__ ((packed)) {enumName}";

    //!!</filter>




    //!!<filter:CSharp>

    // Usings for your state machine so that it can access other code.
    string IRenderConfigCSharp.Usings => """
        using System;  // or whatever you need
        """;

    // Namespace for your generated state machine class.
    // If this ends with a ";" it will use a top level namespace instead of using braces.
    string IRenderConfigCSharp.NameSpace => "{{smName}};";

    // Makes the generated state machine extend a _user_ provided class `{{smName}}Base`.
    // Not required, but can be handy if you don't want to use expansions.
    // string IRenderConfigCSharp.BaseList => "{{smName}}Base";

    // Instead of extending a base class, you could mark the generated state machine class
    // as a partial class. You could do both if you want.
    bool IRenderConfigCSharp.UsePartialClass => true;

    //!!</filter>




    //!!<filter:JavaScript>

    string IRenderConfig.AutoExpandedVars => """
        auto_expanded_user_example_var : 0,
        """;

    string IRenderConfig.VariableDeclarations => """
        non_expanded_user_example_var : 0,
        """;

    // Base class not needed. Sometimes handy though so showing it here.
    // string IRenderConfigJavaScript.ExtendsSuperClass => "{{smName}}Base";

    // Enable if you want generated state machine class like `export class {{smName}}...`
    // bool IRenderConfigJavaScript.UseExportOnClass => true;

    //!!</filter>



    // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-3
    public class MyExpansions : UserExpansionScriptBase
    {
        // See https://github.com/StateSmith/tutorial-2/tree/main/lesson-4 for timing expansions
    }
}
