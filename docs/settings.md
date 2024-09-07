# StateSmith Settings
There are two main ways of specify settings for StateSmith.
- In the diagram file itself [using TOML](https://github.com/StateSmith/StateSmith/issues/335)
- In a .csx file (if your project has one)

Both are valid and supported, but we will documenting the settings here in TOML format.

A few settings can also be specified on the command line when using the CLI.

## Quick TOML Note
The [full spec](https://toml.io/en/v1.0.0) is a quick read, but I'll just make a quick note as you'll encounter it often.

The below two TOML configs are equivalent. Use which ever style works best for you.

### [TOML Dotted Keys](https://toml.io/en/v1.0.0#keys)
```toml
RenderConfig.FileTop = "stuff..."
RenderConfig.AutoExpandedVars  = "stuff..."
```

### [TOML Table](https://toml.io/en/v1.0.0#table)

```toml
[RenderConfig]
FileTop = "stuff..."
AutoExpandedVars  = "stuff..."
```

<br>

# Table of Contents
<!-- TOC start (generated with https://derlin.github.io/bitdowntoc/) -->

- [RenderConfig](#renderconfig)
   * [RenderConfig.FileTop](#renderconfigfiletop)
   * [RenderConfig.AutoExpandedVars](#renderconfigautoexpandedvars)
      + [C99/C++ Toml Example](#c99c-toml-example)
      + [CSharp Toml Example](#csharp-toml-example)
      + [JavaSript Toml Example](#javasript-toml-example)
   * [RenderConfig.VariableDeclarations](#renderconfigvariabledeclarations)
   * [RenderConfig.DefaultVarExpTemplate](#renderconfigdefaultvarexptemplate)
   * [RenderConfig.DefaultFuncExpTemplate](#renderconfigdefaultfuncexptemplate)
   * [RenderConfig.DefaultAnyExpTemplate](#renderconfigdefaultanyexptemplate)
   * [RenderConfig.TriggerMap](#renderconfigtriggermap)
- [RenderConfig.C](#renderconfigc)
   * [RenderConfig.C.HFileTop](#renderconfigchfiletop)
   * [RenderConfig.C.IncludeGuardLabel](#renderconfigcincludeguardlabel)
   * [RenderConfig.C.HFileTopPostIncludeGuard](#renderconfigchfiletoppostincludeguard)
   * [RenderConfig.C.HFileIncludes](#renderconfigchfileincludes)
   * [RenderConfig.C.HFileBottomPreIncludeGuard](#renderconfigchfilebottompreincludeguard)
   * [RenderConfig.C.HFileBottom](#renderconfigchfilebottom)
   * [RenderConfig.C.CFileTop](#renderconfigccfiletop)
   * [RenderConfig.C.CFileIncludes](#renderconfigccfileincludes)
   * [RenderConfig.C.CFileBottom](#renderconfigccfilebottom)
   * [RenderConfig.C.CFileExtension](#renderconfigccfileextension)
   * [RenderConfig.C.HFileExtension](#renderconfigchfileextension)
   * [RenderConfig.C.CEnumDeclarer](#renderconfigccenumdeclarer)
   * [RenderConfig.C.UseStdBool](#renderconfigcusestdbool)
- [RenderConfig.CSharp](#renderconfigcsharp)
   * [RenderConfig.CSharp.NameSpace](#renderconfigcsharpnamespace)
   * [RenderConfig.CSharp.Usings](#renderconfigcsharpusings)
   * [RenderConfig.CSharp.ClassCode](#renderconfigcsharpclasscode)
   * [RenderConfig.CSharp.BaseList](#renderconfigcsharpbaselist)
   * [RenderConfig.CSharp.UseNullable](#renderconfigcsharpusenullable)
   * [RenderConfig.CSharp.UsePartialClass](#renderconfigcsharpusepartialclass)
- [RenderConfig.JavaScript](#renderconfigjavascript)
   * [RenderConfig.JavaScript.ClassCode](#renderconfigjavascriptclasscode)
   * [RenderConfig.JavaScript.ExtendsSuperClass](#renderconfigjavascriptextendssuperclass)
   * [RenderConfig.JavaScript.UseExportOnClass](#renderconfigjavascriptuseexportonclass)
   * [RenderConfig.JavaScript.PrivatePrefix](#renderconfigjavascriptprivateprefix)
- [RenderConfig.Java](#renderconfigjava)
   * [RenderConfig.Java.Package](#renderconfigjavapackage)
    * [RenderConfig.Java.Imports](#renderconfigjavaimports)
    * [RenderConfig.Java.Extends](#renderconfigjavaextends)
    * [RenderConfig.Java.Implements](#renderconfigjavaimplements)
- [SmRunnerSettings](#smrunnersettings)
   * [SmRunnerSettings.outputDirectory](#smrunnersettingsoutputdirectory)
   * [SmRunnerSettings.outputCodeGenTimestamp](#smrunnersettingsoutputcodegentimestamp)
   * [SmRunnerSettings.outputStateSmithVersionInfo](#smrunnersettingsoutputstatesmithversioninfo)
   * [SmRunnerSettings.propagateExceptions](#smrunnersettingspropagateexceptions)
   * [SmRunnerSettings.dumpErrorsToFile](#smrunnersettingsdumperrorstofile)
- [SmRunnerSettings.smDesignDescriber](#smrunnersettingssmdesigndescriber)
- [SmRunnerSettings.algoBalanced1](#smrunnersettingsalgobalanced1)
- [SmRunnerSettings.simulation](#smrunnersettingssimulation)
- [...more](#more)

<br>


# RenderConfig
These settings map to the [IRenderConfig](../src/StateSmith/Output/UserConfig/IRenderConfig.cs) interface you could use in a .csx file.
```toml
[RenderConfig]
FileTop = ""
AutoExpandedVars = ""
VariableDeclarations = ""
DefaultVarExpTemplate = ""
DefaultFuncExpTemplate = ""
DefaultAnyExpTemplate = ""
TriggerMap = ""
```

## RenderConfig.FileTop
Type: `string`

Copied to top of every generated code file. Can be code or comments. Can span multiple lines.

Toml example:
```toml
[RenderConfig]
FileTop = """
    // @Author: ...
    // @Copyright: ...
    """
```

## RenderConfig.AutoExpandedVars
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/91

This section allows you to conveniently do two things at once: 

1) define variables that are part of the state machine struct/class, and
2) automatically create expansions for those variables so you can use them naturally in your diagram actions code like `enter / count++` instead of something like `enter / sm->vars.count++`.

NOTE! This section is parsed depending the selected language.

### C99/C++ Toml Example
```toml
[RenderConfig]
AutoExpandedVars = """
    // Some description for count
    uint8_t count;

    struct SomeInterface * interface; //!< Some comment
    """
```

This will generate the following structure for your state machine. Note how the contents are copied exactly.
```c
// State machine variables. Can be used for inputs, outputs, user variables...
typedef struct MySm_Vars
{
    // Some description for count
    uint8_t count;
    
    struct SomeInterface * interface; //!< Some comment
} MySm_Vars;

// Generated state machine
struct public MySm
{
    // ...
    
    // Variables. Can be used for inputs, outputs, user variables...
    MySm_Vars vars;
};
```

### CSharp Toml Example
```toml
[RenderConfig]
AutoExpandedVars = """
    // Some description for count
    byte count = 0;

    /// <summary>Some comment</summary>
    ISomeInterface interface;
    """
```

This will generate the following structure for your state machine. Note how the contents are copied exactly.
```csharp
public class MySm
{
    // State machine variables. Can be used for inputs, outputs, user variables...
    public struct Vars
    {
        // Some description for count
        byte count = 0;
        
        /// <summary>Some comment</summary>
        ISomeInterface interface;
    }
}
```

### JavaSript Toml Example
**NOTE the syntax difference below** from C99/C++ and CSharp. In JavaScript, the variables are defined directly in the state machine object, not in a struct/class. This might change in the future.

```toml
[RenderConfig]
AutoExpandedVars = """
    // Some description for count
    count: 0,

    // Some comment
    interface: null,
    """
```

This will generate the following structure for your state machine. Note how the contents are copied exactly.
```javascript
class MySm
{
    //...

    // Variables. Can be used for inputs, outputs, user variables...
    vars = {
        // Some description for count
        count: 0,
        
        // Some comment
        interface: null,
    };
}
```

## RenderConfig.VariableDeclarations
Type: `string`

This is the exact same as `AutoExpandedVars` but it is not automatically expanded. This can be sometimes be useful if you want to manually create your own expansions.

```toml
[RenderConfig]
VariableDeclarations = """
    // Your variables here... 
    """
```

## RenderConfig.DefaultVarExpTemplate
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/284

Default variable expansions template. This expansion template is used when a variable appears in some diagram action code but no matching expansion is found. The default template is used to create the expansion. The default template can be any code that you want.

You can use special values inside the template to create the expansion. The special values are:
- `{AutoNameCopy()}` - The name of the variable that is being expanded.
- `{VarsPath}` - The path to the variables in the state machine. This is useful if you want to access the variables in a different way than the default.
- `{AutoVarName()}` - Equivalent to `{VarsPath} + {AutoNameCopy()}`.

```toml
[RenderConfig]
DefaultVarExpTemplate = "{VarsPath}my_interface.{AutoNameCopy()}"
```

Then if you have action code like `enter / count++` and there isn't an existing expansion for `count`, your action code will be expanded to something like `enter / this.vars.my_interface.count++`.

## RenderConfig.DefaultFuncExpTemplate
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/284

Default function expansions template. This expansion template is used when a function call appears in some diagram action code but no matching expansion is found. The default template is used to create the expansion. The default template can be any code that you want.

You can use special values inside the template to create the expansion. The special values are:
- `{AutoNameCopy()}` - The name of the variable that is being expanded.
- `{VarsPath}` - The path to the variables in the state machine. This is useful if you want to access the variables in a different way than the default.

```toml
[RenderConfig]
DefaultFuncExpTemplate = "{VarsPath}my_interface.{AutoNameCopy()}"
```

Then if you have action code like `exit / some_function()` and there isn't an existing expansion for `some_function`, your action code will be expanded to something like `exit / this.vars.my_interface.some_function()`.

## RenderConfig.DefaultAnyExpTemplate
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/284

Default any expansions template. This expansion template is used when a variable or function call appears in some diagram action code but no matching expansion is found. The default template is used to create the expansion. The default template can be any code that you want.

See `DefaultVarExpTemplate` and `DefaultFuncExpTemplate` for more information.

### JavaScript Inheritance Example
If your state machine extends a base class with functionality that you want to access naturally in your diagram action code, you can use the below template.
```toml
[RenderConfig]
DefaultAnyExpTemplate = "this.{AutoNameCopy()}"
```
Then instead of writing `this.timer.set(1)`, you can write `timer.set(1)`.

### JavaScript Interface/Composition Example
If your state machine has a variable with a reference to an interface class that provides functionality, you can use the below template to access the interface functions naturally in your diagram action code.
```toml
[RenderConfig]
DefaultAnyExpTemplate = "{VarsPath}myInterface.{AutoNameCopy()}"
```
Then instead of writing `this.vars.myInterface.doStuff(1)`, you can write `doStuff(1)`.

## RenderConfig.TriggerMap
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/161

Allows you to conveniently create a shorthand for multiple triggers/events.

For example, if you have a trigger called `UP_PRESS` and `UP_HELD`, you can create a shorthand `UPx` that will expand to `UP_PRESS` and `UP_HELD`.

Then in your diagram you add a state behavior like `UPx / count++` and it will expand to `UP_PRESS / count++` and `UP_HELD / count++`.

```toml
[RenderConfig]
TriggerMap = """
    UPx => UP_PRESS, UP_HELD
    DOWNx => DOWN_PRESS, DOWN_HELD
    """
```

<br>
<br>

# RenderConfig.C
Maps to [IRenderConfigC](../src/StateSmith/Output/UserConfig/IRenderConfigC.cs) interface.

```toml
HFileTop = ""
HFileIncludes = ""
CFileTop = ""
CFileIncludes = ""
CFileExtension = ""
HFileExtension = ""
CEnumDeclarer = ""
```

## RenderConfig.C.HFileTop
Type: `string`

Copied to top of generated header file. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
HFileTop = """
    // Use this state machine to ...
    """
```

## RenderConfig.C.IncludeGuardLabel
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/112
<br>Added in lib version: `0.12.1`

If blank (the default), `#pragma once` is used as the include guard. If you want to use standard `#ifdef` include guards, you can specify a label here.

```toml
[RenderConfig.C]
IncludeGuardLabel = "MY_HEADER_H"
```

Generates:
```c
#ifndef MY_HEADER_H
#define MY_HEADER_H
//...
#endif // MY_HEADER_H
```


### Supports `{FILENAME}` and `{fileName}` replacements

```toml
[RenderConfig.C]
IncludeGuardLabel = "{FILENAME}_H"
```

Assuming generated file name is `RocketSm.h`, generates:
```c
#ifndef ROCKETSM_H
#define ROCKETSM_H
//...
#endif // ROCKETSM_H
```


## RenderConfig.C.HFileTopPostIncludeGuard
Type: `string`
<br>Added in lib version: `0.12.2`

Copied to top of generated header file after include guard. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
HFileTopPostIncludeGuard = """
    // Use this state machine to ...
    """
```


## RenderConfig.C.HFileIncludes
Type: `string`

Include statements for the generated header file. Can span multiple lines.

```toml
[RenderConfig.C]
HFileIncludes = """
    #include <stdint.h> // For uint8_t
    #include "some_header.h"
    """
```

## RenderConfig.C.HFileBottomPreIncludeGuard
Type: `string`
<br>Added in lib version: `0.12.2`

Copied to bottom of generated header file just before include guard area. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
HFileBottomPreIncludeGuard = """
    // Use this state machine to ...
    """
```

## RenderConfig.C.HFileBottom
Type: `string`
<br>Added in lib version: `0.12.2`

Copied to bottom of generated header file after include guard area. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
HFileBottom = """
    // Use this state machine to ...
    """
```


## RenderConfig.C.CFileTop
Type: `string`

Copied to top of generated source file. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
CFileTop = """
    // Use this state machine to ...
    """
```

## RenderConfig.C.CFileIncludes
Type: `string`

Include statements for the generated source file. Can span multiple lines.

```toml
[RenderConfig.C]
CFileIncludes = """
    #include "some_header.c"
    """
```

## RenderConfig.C.CFileBottom
Type: `string`
<br>Added in lib version: `0.12.2`

Copied to bottom of generated source file. Can be code or comments. Can span multiple lines.

```toml
[RenderConfig.C]
CFileBottom = """
    // Use this state machine to ...
    """
```


## RenderConfig.C.CFileExtension
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/185

File extension for generated source files. Default is `.c`.

Useful if you want to generate `.cpp` files instead of `.c`.

Also very useful if you want to generate a `.inc` file that you can include into a handwritten source file. See https://github.com/StateSmith/StateSmith-examples/tree/main/c-include-sm-basic-2-plantuml for an example.

```toml
[RenderConfig.C]
CFileExtension = ".cpp"
```

## RenderConfig.C.HFileExtension
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/185

File extension for generated header files. Default is `.h`.

Useful if you want to generate `.hpp` files instead of `.h`.

```toml
[RenderConfig.C]
HFileExtension = ".hpp"
```

## RenderConfig.C.CEnumDeclarer
Type: `string`
Info: https://github.com/StateSmith/StateSmith/issues/185

Allows you to customize how enums are declared in the generated code. This is useful if you want to add attributes to the enum to control its size or alignment.

You can use `{enumName}` in this string and it will be replaced with the name of the enum.

```toml
[RenderConfig.C]
CEnumDeclarer = "typedef enum __attribute__((packed)) {enumName}" # for gcc
```

## RenderConfig.C.UseStdBool
Type: `string`
Info: https://github.com/StateSmith/StateSmith/pull/376
<br>Added in lib version: `0.12.1`

Allows you to disable the use of `bool` from `<stdbool.h>` and instead use integer.

Useful if your toolchain doesn't have `<stdbool.h>`.

```toml
[RenderConfig.C]
UseStdBool = false
```



<br>
<br>

# RenderConfig.CSharp
Maps to the [IRenderConfigCSharp](../src/StateSmith/Output/UserConfig/IRenderConfigCSharp.cs) interface.

```toml
NameSpace = ""
Usings = ""
ClassCode = ""
BaseList = ""
UseNullable = false
UsePartialClass = false
```

## RenderConfig.CSharp.NameSpace
Type: `string`

Namespace for the generated class. If empty, no namespace is generated. If it ends with a `;`, then no braces are added around the namespace.

```toml
[RenderConfig.CSharp]
NameSpace = "MyNamespace"
```

## RenderConfig.CSharp.Usings
Type: `string`

Using statements for the generated class. Can span multiple lines. Use to give your state machine access to other classes.

```toml
[RenderConfig.CSharp]
Usings = """
    using System;
    using System.Collections.Generic;
    """
```

## RenderConfig.CSharp.ClassCode
Type: `string`

Use to add custom code to generated state machine class, although partial classes are usually a better choice, or inheritance.

```toml
[RenderConfig.CSharp]
ClassCode = """
    public void MyCustomFunction()
    {
        // Do something custom...
    }
    """
```

## RenderConfig.CSharp.BaseList
Type: `string`

Base class or interfaces for the generated class. Can be multiple interfaces separated by a comma.

```toml
[RenderConfig.CSharp]
BaseList = "MyUserBaseClass, IMyOtherUserInterface"
```

## RenderConfig.CSharp.UseNullable
Type: `bool`

Controls whether generated code uses nullable types. Default is `true`.

```toml
[RenderConfig.CSharp]
UseNullable = false
```

## RenderConfig.CSharp.UsePartialClass
Type: `bool`

Controls whether generated code uses partial classes. Default is `true`.

```toml
[RenderConfig.CSharp]
UsePartialClass = false
```





<br>
<br>

# RenderConfig.JavaScript
Maps to the [IRenderConfigJavaScript](../src/StateSmith/Output/UserConfig/IRenderConfigJavaScript.cs) interface.

```toml
ClassCode = ""
ExtendsSuperClass = "MyUserBaseClass"
UseExportOnClass = true
PrivatePrefix = "_"
```

## RenderConfig.JavaScript.ClassCode
Type: `string`

Use to add custom code to generated state machine class. Inheritance or composition is usually a better choice.

```toml
[RenderConfig.JavaScript]
ClassCode = """
    // Add custom code here...
    """
```

## RenderConfig.JavaScript.ExtendsSuperClass
Type: `string`

Use to have generated state machine class extend a user defined base class.

```toml
[RenderConfig.JavaScript]
ExtendsSuperClass = "MyUserBaseClass"
```

## RenderConfig.JavaScript.UseExportOnClass
Type: `bool`

Useful for Node.js.

Controls whether generated code uses `export` on the class. Default is `false`.

```toml
[RenderConfig.JavaScript]
UseExportOnClass = true
```

## RenderConfig.JavaScript.PrivatePrefix
Type: `string`

Prefix for private variables. Default is `#` for private class fields. Set to `_` if you are using an older version of JavaScript.

https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Classes/Private_properties

```toml
[RenderConfig.JavaScript]
PrivatePrefix = "_"
```


<br>
<br>

# RenderConfig.Java
Maps to the [IRenderConfigJava](../src/StateSmith/Output/UserConfig/IRenderConfigJava.cs) interface.

```toml
Package = "my.package.for.statemachine"
Imports = """
    import java.util.*; // or whatever you need
    """
Extends = "MyUserBaseClass"
Implements = "SomeUserInterface"
ClassCode = """
    // Add custom code here to inject into the generated class.
    // Inheritance or composition might be a better choice.
    """
```

## RenderConfig.Java.Package
Type: `string`

Use to specify the package for the generated class. If empty, no package is generated.

```toml
[RenderConfig.Java]
Package = "my.package.for.statemachine"
```

## RenderConfig.Java.Imports
Type: `string`

Use to add import statements to the generated class. Can span multiple lines.

```toml
[RenderConfig.Java]
Imports = """
    import java.util.*; // or whatever you need
    """
```

## RenderConfig.Java.Extends
Type: `string`

Use to have generated state machine class extend a user defined base class.

```toml
[RenderConfig.Java]
Extends = "MyUserBaseClass"
```

## RenderConfig.Java.Implements
Type: `string`

Use to have generated state machine class implement a user defined interface.

```toml
[RenderConfig.Java]
Implements = "SomeUserInterface"
```

## RenderConfig.Java.ClassCode
Type: `string`

Use to add custom code to generated state machine class. Inheritance or composition is often a better choice.

```toml
[RenderConfig.JavaScript]
ClassCode = """
    // Add custom code here...
    """
```

<br>
<br>






-----------------






<br>
<br>

# SmRunnerSettings
Maps to the [RunnerSettings](../src/StateSmith/Runner/RunnerSettings.cs) class.

```toml
[SmRunnerSettings]
transpilerId = "CSharp"
algorithmId = "Balanced2"
outputDirectory = ""
outputCodeGenTimestamp = false
outputStateSmithVersionInfo = true
propagateExceptions = false
dumpErrorsToFile = false
```

If you are using a .csx file, you can choose to specify these settings in your diagram file using TOML and/or in the .csx file.

```cs
// .csx file
SmRunner smRunner = new(diagramPath: "MyDiagram.plantuml", new MyRenderConfig());
smRunner.Settings.outputDirectory = "./gen";
smRunner.Run();
```

## SmRunnerSettings.transpilerId
Type: `string`

The transpiler to use. Default is `C99`.

```toml
[SmRunnerSettings]
transpilerId = "CSharp"
```

Options:
- `Default` --> `C99`
- `C99`
- `CSharp`
- `JavaScript`

> csx note: you can also specify the `transpilerId` via the `SmRunner` constructor.

## SmRunnerSettings.algorithmId
Type: `string`
Info: https://github.com/StateSmith/StateSmith/wiki/Algorithms

The algorithm to use. The default value will likely change in the future. If you want to lock in the current algorithm, specify it.

```toml
[SmRunnerSettings]
algorithmId = "Balanced1"
```

Options:
- `Default` --> `Balanced2`
- `Balanced1` (old default)
- `Balanced2` (new default) - added in lib version `0.13.0`

> csx note: you can also specify the `algorithmId` via the `SmRunner` constructor.


## SmRunnerSettings.outputDirectory
Type: `string`

Output directory for generated code files. If empty, the current working directory is used. You can specify a relative or absolute path.

```toml
[SmRunnerSettings]
outputDirectory = "./gen"
```

## SmRunnerSettings.outputCodeGenTimestamp
Type: `bool`

Controls whether a timestamp is added to the generated code files. Default is `false` to avoid git diff noise.

```toml
[SmRunnerSettings]
outputCodeGenTimestamp = true
```

## SmRunnerSettings.outputStateSmithVersionInfo
Type: `bool`

Controls whether StateSmith version info is added to the generated code files. Default is `true`.

```toml
[SmRunnerSettings]
outputStateSmithVersionInfo = false
```

## SmRunnerSettings.propagateExceptions
Type: `bool`
Info: https://github.com/StateSmith/StateSmith/issues/348

> NOTE! This setting (if specified in your diagram) will not take effect if your diagram fails to parse. In this case, use a CLI option.

Controls whether exceptions are propagated to the caller. Useful for getting stack trace information on failures. Default is `false`.

```toml
[SmRunnerSettings]
propagateExceptions = true
```

## SmRunnerSettings.dumpErrorsToFile
Type: `bool`

Controls whether errors are dumped to a file. Useful for debugging. Default is `false`.

```toml
[SmRunnerSettings]
dumpErrorsToFile = true
```


<br>
<br>

# SmRunnerSettings.smDesignDescriber
Info: https://github.com/StateSmith/StateSmith/issues/200

Outputs a markdown file that describes the design of the state machine.

This feature is useful for:
- git/svn diffs
- inspecting hierarchical designs
- understanding StateSmith transformations


```toml
[SmRunnerSettings.smDesignDescriber]
enabled = true
outputDirectory = ""
outputAncestorHandlers = true

[SmRunnerSettings.smDesignDescriber.outputSections]
beforeTransformations = false
afterTransformations  = true
```

# SmRunnerSettings.algoBalanced1
Info: https://github.com/StateSmith/StateSmith/issues/181

You can tell StateSmith that you don't want it to generate an event ID or state ID to string function with the below settings.

```toml
[SmRunnerSettings.algoBalanced1]
outputEventIdToStringFunction = false
outputStateIdToStringFunction = false
```

# SmRunnerSettings.simulation
Info: to be documented

When StateSmith is from `StateSmith.Cli`, it generates a simulation file by default. You can pass CLI argument `--no-sim-gen` to disable this feature or use the below settings.

If you are using a .csx file, you need to explicitly enable this feature.

```toml
[SmRunnerSettings.simulation]
enableGeneration = false
outputDirectory = ".."
outputFileNamePostfix = ".sim.html" # NOT YET SUPPORTED!
```

Note: `outputFileNamePostfix` is [not yet supported](https://github.com/StateSmith/StateSmith/issues/360).

# ...more
There are more SmRunnerSettings available that are less commonly used and not yet documented here.

See [RunnerSettings.cs](https://github.com/StateSmith/StateSmith/blob/main/src/StateSmith/Runner/RunnerSettings.cs) in github project.
