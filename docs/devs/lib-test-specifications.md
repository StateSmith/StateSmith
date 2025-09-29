See basic unit test instructions first.

NOTE! You probably don't need to run the specification tests unless you are working on code gen.

# Requirements
You don't need to install *all* of these unless you plan on doing code gen that affects them. Github CI/CD will test all languages in merge requests.

## C/C++
### [Linux/Mac]
Install `gcc`.

### [Windows]
First install `WSL2`.

Then <u>INSIDE WSL2</u>, install `gcc`.

## Python 3
Install Python 3

## Java
Install a java JDK.

## JavaScript
Install `nodejs` version 22. 
> ⚠️ It has to be version 22. Or at least *not* version 23 which causes some TypeScript tests to [fail due to warning](https://github.com/nodejs/node/issues/55417) (not in our code).

### Windows
Download [installer](https://nodejs.org/dist/v22.20.0/node-v22.20.0-x64.msi) or similar from [NodeSource](https://nodejs.org/en/download/package-manager#debian-and-ubuntu-based-linux-distributions).

### Linux/Mac
Directions from [here](https://nodejs.org/en/download#debian-and-ubuntu-based-linux-distributions).
```bash
# Download and install nvm:
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash

# in lieu of restarting the shell
\. "$HOME/.nvm/nvm.sh"

# Download and install Node.js:
nvm install 22

# Verify the Node.js version:
node -v # Should print "v22.20.0".

# Verify npm version:
npm -v # Should print "10.9.3".
```

### Troubleshooting
* Old versions of nodejs (v12) will give errors like `SyntaxError: Unexpected token '{'`.


## Install typescript
Run
```
npm install -g typescript@5.6.3
```
We pin to 5.6.3 to be compatible with Node 22


<br><br>


# Let's build and run!
Before making any code changes, make sure that the tests pass.

`cd` to the `src/StateSmith` directory.

You can run all StateSmith core library tests with
```
src/StateSmith$ dotnet test
```

NOTE! You may need to run twice if lib souce was changed.

You can run just the specification tests with this filter command if you are using the terminal:
```
src/StateSmith$ dotnet test --filter Spec.
```

You can run just the specification tests for python like this:
```
src/StateSmith$ dotnet test --filter Spec2.Python
```

## Run *all* tests for *all* projects
If you want to run lib and ss.cli tests all at once, run `dotnet test` from `src` directory.

<br><br>


# 🔧 Troubleshooting

## Reset System
You may want to reboot your system after installing everything if you have strange issues. Only 1 reboot should be needed otherwise there is another issue.

## Specification Test Failures
NOTE! We run a lot of specification tests. Sometimes they will fail without a helpful error message (like below). If you are concerned, try running only the tests that failed again and they should pass (if CPU resources was the problem).
`System.InvalidOperationException : An async read operation has already been started on the stream.` - some part of the external process invocation failed. Check to make sure tools like (gcc or required dotnet version) are installed.



<!-- 

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

-->

