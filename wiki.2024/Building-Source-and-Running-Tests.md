You can build and test StateSmith from the command line or with a GUI.

# Installation Requirements
## General Tests
Install `dotnet-sdk-8.0` (needed for tests).

## Specification Tests
NOTE! You probably don't need to run the specification tests unless you are working on code gen. You can skip this step if you simply filter the tests that you want to run. See more below on filtering.

* Install `nodejs` version 21 or later (to test javascript code gen).
    * Actually version 23 causes some TypeScript tests to [fail due to warning](https://github.com/nodejs/node/issues/55417) (not in our code). Use node 21 or 22 for now.
    * Linux NOTE!!! Many distro package managers use an old version of nodejs (v12) which is too old. You'll get errors like `SyntaxError: Unexpected token '{'`. For debian/ubuntu, get latest from [NodeSource](https://nodejs.org/en/download/package-manager#debian-and-ubuntu-based-linux-distributions).
* Install a java sdk.

### [Windows]
* Install `WSL2`
* <u>INSIDE WSL2</u>, install `GCC` (to test C).

### [Linux/Mac]:
* Install `GCC` (to test C).


<br><br>


# Let's build and run!
Before making any code changes, make sure that the tests pass.

> **NOTE!** There is a section at the bottom of this document with troubleshooting tips. If you get stuck check there or contact us (discord/github).

<br>
<br>

# Using Command Line (any platform)
`cd` to the `StateSmith/src` directory.

## Build
Run command `dotnet build` to build. You might need to run it a few times for everything to get resolved. It should be good after that.

```
StateSmith/src$ dotnet build
MSBuild version 17.3.2+561848881 for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  StateSmith -> /home/afk/code/prsnl/StateSmith/src/StateSmith/bin/Debug/net6.0/StateSmith.dll
  StateSmithCli -> /home/afk/code/prsnl/StateSmith/src/StateSmithCli/bin/Debug/net6.0/StateSmithCli.dll
  StateSmithTest -> /home/afk/code/prsnl/StateSmith/src/StateSmithTest/bin/Debug/net6.0/StateSmithTest.dll
  StateSmithCliTest -> /home/afk/code/prsnl/StateSmith/src/StateSmithCliTest/bin/Debug/net6.0/StateSmithCliTest.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.73
```

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

## Run Tests
StateSmith has a fair number of tests and the specification tests are slow, so let's just run the quick ones for now.

Run command `dotnet test --filter StateSmithTest`

```
StateSmith/src$ dotnet test --filter StateSmithTest
  Determining projects to restore...
  <snip...>
Passed!  - Failed:     0, Passed:   307, Skipped:     2, Total:   309, Duration: 3 s
```

If any failed, please refer to the troubleshooting section below.

To run all tests (including the slow specification tests), see the specification tests section below.

Helpful tips:
* Run tests showing passing: `dotnet test -v=normal`
* If you want to only run tests for a specific test project like `StateSmithTest` (or `StateSmith.CliTest`), cd into that directory first before running the command.


<br>
<br>

# Using an IDE (optional)
If on Windows, I recommend the free edition of [Visual Studio](https://visualstudio.microsoft.com/downloads/). It just works. It is `free` for open source collaborators. vscode works as well.

## (Windows) Visual Studio
I generally test on an updated Visual Studio 2022.

### Running Unit Tests
Open the test explorer in Visual Studio.

![image](https://user-images.githubusercontent.com/274012/218225543-4cf390a7-4816-45c5-9428-21f305eb0d9a.png)

Then click on the green arrows to run the selected/all tests.

![image](https://user-images.githubusercontent.com/274012/218225643-b7e7389b-e81c-4385-8740-7af57e136a3a.png)


## (Linux/Mac/Windows) Visual Studio Code
Expect a bit of googling to figure this out. Hit us up on discord if you need help.

These instructions are for the older vscode C# extension where `OmniSharp` is used. I don't know how to use the latest C# dev tools extension (I don't use it because it doesn't support .csx files).

### Running/Debugging Unit Tests
Make sure `Code Lens` is enabled in vscode settings.

![image](https://github.com/StateSmith/StateSmith/assets/274012/b79e9bbc-8aee-4667-a092-ef0e99fd88fc)

Then scroll down a bit and enable these as well:

![image](https://github.com/StateSmith/StateSmith/assets/274012/ba1f0be9-225b-484a-9b67-0a2048bae3f5)

C# `Code Lens` is a bit glitchy. You sometimes have to give it a few seconds or reload the vscode window.

You can then navigate to some test code and run individual tests or debug them:
![image](https://github.com/StateSmith/StateSmith/assets/274012/e0754ed2-189e-420e-aefa-4ade4fdd4d72)

Please let me know if you needed any other steps to get it working for you.


<br>
<br>

# 🔧 Troubleshooting

## inotify instances has been reached (Unix only)
You'll see a number of failures and messages like:
```
System.IO.IOException : The configured user limit (128) on the number of inotify instances has been reached, or the per-process limit on the number of open file descriptors has been reached.
```

The test runner is trying to do too much at once and exceeding the Linux user limit. We can increase that easily though.

Run the below command to check that the limit is too low:
```
StateSmith/src$ sysctl fs.inotify
fs.inotify.max_queued_events = 16384
fs.inotify.max_user_instances = 128
fs.inotify.max_user_watches = 524288
```

Increase this with the following command which I adapted from a StackOverflow post https://askubuntu.com/a/416545
```
StateSmith/src$ echo 1000 | sudo tee /proc/sys/fs/inotify/max_user_instances
```

Check that `sysctl fs.inotify` now reports the correct number and try running the tests again.

## Culture Number Format Setting
If your culture uses something other than a period for a decimal point, some of the unit tests may fail. This issue [#159](https://github.com/StateSmith/StateSmith/issues/159) has been solved for users though. Not sure how to apply that solution to all unit tests efficiently. Let me know if you run into any issues.


## Specification Test Failures
NOTE! We run a lot of specification tests. Sometimes they will fail without a helpful error message (like below). If you are concerned, try running only the tests that failed again and they should pass (if CPU resources was the problem).
`System.InvalidOperationException : An async read operation has already been started on the stream.` - some part of the external process invocation failed. Check to make sure tools like (gcc or required dotnet version) are installed.



<br>
<br>

# Specification Tests
The specification tests take a while to run because they actually generate code, compile it (for compiled languages), run the executable with input events and confirm the output is as expected. 

I suggest that you not run them unless you need to. If the code I'm working on doesn't affect them, I usually only run them before I push or just let the GitHub CI run those tests.

![image](https://github.com/StateSmith/StateSmith/assets/274012/a15a15a1-78e0-46e1-8d0b-ca6dba9621b3)

The test fixtures around the specification tests are a bit complex. This is because they are structured to allow testing future output languages (like python, js, C#...) with minimal effort.

If your specification tests fail randomly, we may need to increase the test timeout for the process. I had to do this once already.
You can also run the tests in sections to reduce CPU load.


## Running Spec Tests
You can run just the specification tests with this filter command if you are using the terminal:
```
dotnet test --filter Spec.
```

You can run ALL tests with just
```
dotnet test
```


<br>
<br>

# Updating Test Examples
If you want to see how your changes will affect example projects run this command from a unix or WSL terminal:

```bash
afk:StateSmith$ ./src/test-misc/examples/code-gen-all.sh 1
```

Then use git to see if any of the example projects generated new code.

----