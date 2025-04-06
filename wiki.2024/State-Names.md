# Auto By Default In v0.8.11+
You can still set to manual if you want.

You probably don't need to read the rest of this page if you are on v0.8.11+. Report any issues [here](https://github.com/StateSmith/StateSmith/issues/138).

---

# State Names Must Be Unique
State names have to be unique before the code generation step.

Small designs typically don't have an issue with accidentally re-using the same state name. However, large nested designs definitely do run into this problem.

StateSmith will print a helpful error message during validation if state names are not unique.

If this happens to you, you have a few options:
* Manually rename the offending state.
* Use [$mod prefix](https://github.com/StateSmith/StateSmith/wiki/$mod-prefix) for advanced/custom control.
* Enable automatic name conflict resolution (see below).

## Automatic Name Conflict Resolution (0.8.7+)
Available starting version 0.8.7, you can enable [automatic name conflict resolution](https://github.com/StateSmith/StateSmith/issues/138). This will likely eventually become the default, but we want to test it in the wild for a bit.

```c#
SmRunner runner = /* your code */
runner.Settings.nameConflictResolution = RunnerSettings.NameConflictResolution.ShortFqnAncestor;
runner.Run();
```

Or

```c#
SmRunner runner =  /* your code */
runner.Settings.nameConflictResolution = RunnerSettings.NameConflictResolution.ShortFqnParent;
runner.Run();
```

What's the difference between `ShortFqnAncestor` and `ShortFqnParent`?
* [`ShortFqnAncestor`](https://github.com/StateSmith/StateSmith/issues/138#issuecomment-1469374108)
* [`ShortFqnParent`](https://github.com/StateSmith/StateSmith/issues/138#issuecomment-1469920758)




