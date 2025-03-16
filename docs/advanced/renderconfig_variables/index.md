---
title: RenderConfig variables
parent: Advanced usage
layout: default
---

# Advanced: RenderConfig Variables

## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [Using StateSmith in CSX and apps](/StateSmith/advanced/csx/)
* [Advanced: RenderConfig](/StateSmith/advanced/renderconfig/)
* [Actions](/StateSmith/statemachine_reference/actions/)


# Add Some `VariableDeclarations`
There are a few ways to define variables for your state machine.

> Note: some object oriented languages like C# can have the generated state machine extend a user defined base class. The base class is a better place to declare variables for OO languages than what is shown here.

The `VariableDeclarations` section allows us to specify text and code to put in the state machine variables object. If there is no code in this section,
no state machine variable object will be created.

```cs
{% include_relative gen/lightbulb.csx %}
```



# Use The Syntax For Your Language
Note that StateSmith does not parse the `VariableDeclarations` string. It outputs whatever is written there. Use the proper syntax for the output language you are generating for.

If you were using C/C++/C#, you might declare like `int count = 0;` instead.




# Code Gen and Check Output
Run your code generation script and check the generated file `lightbulb.js`.
```
dotnet-script lightbulb.csx
```

You can see that a variables section has been added to your state machine:

```js
// Generated state machine
class lightbulb
{
   ...
    
    // Variables. Can be used for inputs, outputs, user variables...
    vars = {
        count: 0,            // a state machine variable
        switch_state: false, // an input to state machine
        light_state: false,  // an output from state machine
    };    
```





# You Can Put Anything In `VariableDeclarations`
You can put anything valid for your language in the `VariableDeclarations` section. You might choose to do something like below.

```cs
public class LightSmRenderConfig : IRenderConfig
{
    //...

    string IRenderConfig.VariableDeclarations => """
        inputs: {
            switch_state: false,
        },
        outputs: {
            light_state: false,
        },
        internal: {
            count: 0,
        }
        """;
}
```

C allows you to do something similar with anonymous structures.


# How Do We Get Access To The `count` Var?
For javascript, you can access the count variable using `this.vars.count` as shown below.

> **TIP:** It is best to access state machine variables through Expansions (covered next) because the path to the variable is dependent upon the chosen language, algorithm, and potentially future options.


<table>
<tr>
<td>
<pre>
{% include_relative lightbulb.puml %}
</pre>
</td>
</tr>
<tr>
<td>
{% plantuml %}
{% include_relative lightbulb.puml %}
{% endplantuml %}
</td>
</tr>
</table>


<iframe height="300" width="600" src="gen/lightbulb.sim.html"></iframe>



