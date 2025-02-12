# Building StateSmith from Source

## Install prerequisites

Install `dotnet-sdk-8.0` from https://dotnet.microsoft.com/en-us/download


### Build using the Command Line (any platform)
`cd` to the `StateSmith/src` directory.
Run `dotnet build` to build. You might need to run it a few times for everything to get resolved. It should be good after that.

```
home$ cd StateSmith/src
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


## Run the Tests
StateSmith has a fair number of tests and the specification tests are slow, so let's just run the quick ones for now.

Run command `dotnet test --filter StateSmithTest`

```
StateSmith/src$ dotnet test --filter StateSmithTest
  Determining projects to restore...
  <snip...>
Passed!  - Failed:     0, Passed:   307, Skipped:     2, Total:   309, Duration: 3 s
```

If any failed, please refer to the troubleshooting section below.

Helpful tips:
* Run tests showing passing: `dotnet test -v=normal`
* If you want to only run tests for a specific test project like `StateSmithTest` (or `StateSmith.CliTest`), cd into that directory first before running the command.
* You can run the command `dotnet test --list-tests` to see all the available tests.

<br>
<br>

## Run the built ss.cli

Look for the StateSmith.Cli binary you built in the bin directory.

```
StateSmith/src$ ./bin/Debug/net8.0/StateSmith.Cli
bin/Debug/net8.0/StateSmith.Cli
Using settings directory: /Users/mike/Library/Application Support/StateSmith.Cli
StateSmith.Cli 0.10.0+097e1c1f36a4cce898b47aa426e9622999ff62ce

Usage:

  run         Run StateSmith code generation.

  run-lite    Run StateSmith code generation.

  create      Create a new StateSmith project from template.

  setup       Set up vscode for StateSmith & csx files.


To get help for a specific verb, use the command name followed by --help
The `create` verb currently has no options and runs a wizard.

```


## 🔧 Troubleshooting

### inotify instances has been reached (Unix only)
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

### Culture Number Format Setting
If your culture uses something other than a period for a decimal point, some of the unit tests may fail. This issue [#159](https://github.com/StateSmith/StateSmith/issues/159) has been solved for users though. Not sure how to apply that solution to all unit tests efficiently. Let me know if you run into any issues.


### Specification Test Failures
NOTE! We run a lot of specification tests. Sometimes they will fail without a helpful error message (like below). If you are concerned, try running only the tests that failed again and they should pass (if CPU resources was the problem).
`System.InvalidOperationException : An async read operation has already been started on the stream.` - some part of the external process invocation failed. Check to make sure tools like (gcc or required dotnet version) are installed.



<br>
<br>

## Specification Tests
The specification tests take a while to run because they actually generate code, compile it (for compiled languages), run the executable with input events and confirm the output is as expected. 

I suggest that you not run them unless you need to. If the code I'm working on doesn't affect them, I usually only run them before I push or just let the GitHub CI run those tests.

![image](https://github.com/StateSmith/StateSmith/assets/274012/a15a15a1-78e0-46e1-8d0b-ca6dba9621b3)

The test fixtures around the specification tests are a bit complex. This is because they are structured to allow testing future output languages (like python, js, C#...) with minimal effort.

If your specification tests fail randomly, we may need to increase the test timeout for the process. I had to do this once already.
You can also run the tests in sections to reduce CPU load.


### Running Specification Tests
You can run just the specification tests with this filter command if you are using the terminal:
```
dotnet test --filter Spec.
```

You can run ALL tests with just
```
dotnet test
```

