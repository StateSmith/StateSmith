---
title: RenderConfig
parent: Advanced usage
layout: default
---

# Advanced: RenderConfig

You can use a `RenderConfig` to customize StateSmith's code generation to do things like add additional elements to your functions and classes.

## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [Using StateSmith in CSX and apps](/StateSmith/advanced/csx/)


## Adding a RenderConfig

Add an [IRenderConfig]( {{ site.github.repository_url }}/src/StateSmith/Output/UserConfig/IRenderConfig.cs ) to the CSX you created in the previous section. 

```c#
{% include_relative lightbulb.csx %}
```

Then run your script
```
% dotnet-script lightbulb.csx
```

If you inspect the output `lightbulb.js` you should see `turtles` at the top:

```js
// Copyright: turtles, turtles, turtles...
// You can include other files/modules specific to your programming language here
let x = 55; // You can even output raw code...

// Generated state machine
class lightbulb
{
    ...
}
```

Read on to the other sections to learn more about what you can do with RenderConfig

> You can actually define the render config options directly in the diagram if you want. See [this wiki page](https://github.com/StateSmith/StateSmith/wiki/Diagram-Based-Render-Config) for details.
>
> In the future, we won't need to use .csx files at all.




