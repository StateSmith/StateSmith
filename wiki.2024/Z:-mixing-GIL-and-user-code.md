This is an advanced topic and mostly a collection of thoughts to document further. You don't need to read this.

Essentially, it is pretty tricky to output user code that can be in any language to our transpiler and mix it with GIL code that needs transpiling.

This functionality is also very likely to change. Don't rely on it just yet.

You can mix expanded user code and GIL code together.

```
enter [my_var == 0 && $gil(this.some_history_var == 22)]
```

The TracingModder class needs this functionality because it outputs code that needs to be expanded, but wraps GIL code that needs to be transpiled.

The above expression gets broken into steps:
```html
<finalCode>my_var == 0 && </code><gilCode>this.some_history_var == 22</code><finalCode></code>
```

This then gets converted into GIL ready for transpiling:
```c#
if (____GilNoEmit_GilBoolWrapper(____GilNoEmit_EchoStringBool("my_var == 0 && ", this.some_history_var == 22)))
```

Then the special GIL functions get resolved and it turns into something like:
```c#
// C# GIL
if (/*my_var == 0 && */ this.some_history_var == 22)
```

Then the comments (which actually have special tags) are uncommented in the post processor.
```c
// C final target code
if (my_var == 0 && sm->some_history_var == 22)
```