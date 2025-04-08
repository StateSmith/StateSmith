[Tutorial-2](https://github.com/StateSmith/tutorial-2) explains the basics of expansions. This is a reference page with some more tips.

# Special Expansions
There are a few StateSmith helpers that you can use for expansions.

`VarsPath` expands to the state machine variables path.
```c#
string oc => VarsPath + "off_count";
// expands to `this.vars.off_count` for js, c#
// expands to `sm->vars.off_count` for c
```

`AutoNameCopy()` expands to just the expansion name.
```c#
string turtle_topple() => AutoNameCopy();
// expands to "turtle_topple"

string menu_at_top() => "Display_" + AutoNameCopy() + "()";
// expands to "Display_menu_at_top()"
```

`AutoVarName()` equivalent to  `VarsPath + AutoNameCopy()`.
```c#
string off_count => AutoVarName();
// expands to `this.vars.off_count` for js, c#
// expands to `sm->vars.off_count` for c
```


<br>

# [C# interpolated raw string literals](https://thecodeblogger.com/2022/09/17/c-11-what-are-raw-string-literals/)

[C# interpolated raw string literals](https://thecodeblogger.com/2022/09/17/c-11-what-are-raw-string-literals/) are really handy.
```c#
string say_hi() => $"""window.alert("Hi!")""";  // no need to escape double quotes
```

```c#
string menu_at_top() => $"""Display_{AutoNameCopy()}()"""; // calls method `AutoNameCopy()`
// expands to "Display_menu_at_top()"
```


<br>

# For Enumerations
Expansions are helpful for when you want to omit an enumeration type/prefix. Using the below expansion, you can write 
* `set_control_mode(ARMED)` instead of 
* `set_control_mode(BurritoBlasterControlMode_ARMED)` - C, C++
* `set_control_mode(BurritoBlasterControlMode.ARMED)` - C#, js, C++...

```c#
string set_control_mode(string mode_name) => $"this.control_mode = BurritoBlasterControlMode_{mode_name}";
```
