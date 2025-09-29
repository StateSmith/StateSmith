See build instructions first.

🚩 cd to `src/StateSmithTest/` for all of these instructions

# Run Basic Tests
There are a lot of tests in StateSmith. We are going to focus on basic tests for now.

See also the specification tests markdown page in this dir if you are doing code gen related work.

This is pretty easy. Just run `dotnet test --filter StateSmithTest`

If on Windows, it will just work. You should see a ton of output ending with this:

```
Test summary: total: 454, failed: 0, succeeded: 453, skipped: 1, duration: 9.4s
Build succeeded in 10.6s
```

## ⚠️ Unix - `inotify instances has been reached`
You'll see a number of failures and messages like:
```
System.IO.IOException : The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached.
```

The test runner is trying to do too much at once and exceeding the unix user limit. We can increase that easily though.

Run the below command to check that the limit is too low (ex: 128):
```
$ sysctl fs.inotify
fs.inotify.max_queued_events = 16384
fs.inotify.max_user_instances = 128
fs.inotify.max_user_watches = 524288
```

Increase with below commands adapted from an [askubuntu post](https://askubuntu.com/questions/770374/user-limit-of-inotify-watches-reached-on-ubuntu-16-04).
```bash
# temporary increase
$ echo 2000 | sudo tee /proc/sys/fs/inotify/max_user_instances
```

```bash
# permanent change
$ echo fs.inotify.max_user_instances=2000 | sudo tee -a /etc/sysctl.conf
$ sudo sysctl -p
```

Check that `sysctl fs.inotify` now reports 2000.

Now run `dotnet test --filter StateSmithTest` again and you should see a ton of messages ending with success.

## Why filter?
We are keeping things basic to start. We don't want to run specification tests that require various other compilers/runtimes.

## Tips
Too much console noise in output?
Try `dotnet test --filter StateSmithTest --tl:off`

It would be nice to also show test progress, but I'm not sure how to make that work from CLI.

## 🧠 Important - Tests Run In Parallel
Note! By default, xUnit will run all tests in parallel. We want this because we have a lot of tests!

## List Tests
Run command `dotnet test --list-tests` to see all the available tests.

```
StateSmith/src$ dotnet test --list-tests
  Determining projects to restore...
  <snip...>
The following Tests are available:
    Spec.Spec2.JavaScript.Spec2TestsJavaScript.Test1_DoEventHandling
    Spec.Spec2.JavaScript.Spec2TestsJavaScript.Test2_RegularEventHandling
    Spec.Spec2.JavaScript.Spec2TestsJavaScript.Test3_BehaviorOrdering
    Spec.Spec2.JavaScript.Spec2TestsJavaScript.Test4_ParentChildTransitions
    <snip...>
    StateSmithTest.PlantUMLTests.ParsingTests.EntryExitStates
    StateSmithTest.PlantUMLTests.ParsingTests.ChoicePoints
    <snip...>
```

<br>

# Add a New Failing Test
Open file `StringUtilsTest.cs` in a text editor and add the following text **inside** of the `StringUtilsTest` class.

```cs
[Fact]
public void ExampleFailingTest()
{
    int an_integer = 22;
    an_integer.Should().Be(0); // this will fail
}
```

Then run all the tests for the `StringUtilsTest` class with this command:

```
src/StateSmithTest$ dotnet test --filter=StringUtilsTest
```

You should see error messages like:
```shell
<snip...>
[xUnit.net 00:00:00.63]     StateSmithTest.StringUtilsTest.ExampleFailingTest [FAIL]
[xUnit.net 00:00:00.63]       Expected an_integer to be 0, but found 22.
<snip...>
  StateSmithTest test failed with 1 error(s) (1.2s)
    /home/afk/code/StateSmith/src/StateSmithTest/StringUtilsTest.cs(14): error TESTERROR: 
      StateSmithTest.StringUtilsTest.ExampleFailingTest (42ms): Error Message: Expected an_integer to be 0, but found 22.
<snip...>
```

