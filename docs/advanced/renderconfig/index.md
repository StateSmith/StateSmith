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
{% include_relative lightbulb.js %}
```