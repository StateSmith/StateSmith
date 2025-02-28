# Advanced: RenderConfig and Variables

## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [Using StateSmith in CSX and apps](/StateSmith/advanced/csx)


## Adding a RenderConfig

Add a RenderConfig to the CSX you created in the previous section.

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