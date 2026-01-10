
# Requirements
See [basic unit test instructions](./lib-test-basic.md) first. 

You don't need to run the specification tests and install *all* of these unless you plan on doing code gen that affects them. Github CI/CD will test all languages in merge requests.

> ℹ️ **Windows** users should install and use `WSL2`, it greatly simplifies the setup and documentation maintenance.

## C/C++
Install the latest `gcc` and `g++`.[^footnot_gcc]

```bash
# Linux/WSL:
sudo apt-get install gcc g++

# MacOS:
brew install gcc
```

## Python
![Supported Versions](https://img.shields.io/badge/Supported_Versions->=_v3\.10-blue)

Install `Python`.[^footnote_python]

```bash
# Download and install uv:
curl -LsSf https://astral.sh/uv/install.sh | sh

# Download and install Python:
uv python install 3.15

# Activate Python
uv python pin 3.15

# Verify installation
python3 --version # Should print e.g. "Python 3.15.0a1"
```

## Java
![Supported Versions](https://img.shields.io/badge/Supported_Versions->=_v5-blue)

Install `java-jdk`.

```bash
# Linux/WSL:
sudo apt-get install openjdk-25-jdk-headless

# MacOS:
brew install openjdk@25
```

## JavaScript (Node.js)
![Supported Versions](https://img.shields.io/badge/Supported_Versions->=_v20\.0\.0-blue)

Install `Node.js`.[^footnote_javascript]

```bash
# Download and install nvm:
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash

# in lieu of restarting the shell
\. "$HOME/.nvm/nvm.sh"

# Download and install Node.js:
nvm install 24

# Verify installation
node -v # Should print e.g. "v24.10.0".
npm -v # Should print e.g. "11.6.1".
```


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
> ⚠️ We run a lot of specification tests. Sometimes they will fail without a helpful error message (like below). If you are concerned, try running only the tests that failed again and they should pass (if CPU resources was the problem).

 - `System.InvalidOperationException : An async read operation has already been started on the stream.`
   - some part of the external process invocation failed => Check to make sure tools (e.g. gcc, dotnet) are installed and have the correct versions.



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

[^footnot_gcc]: It may be enough to install `gcc`, but depending on the system `g++` is missing and may need to be installed alongside (e.g. Ubuntu in `WSL2`). MacOS appears to have both installed by default, but they are usually symlinked to the `clang` compiler, which can be fixed by installing the `gcc` homebrew package.

[^footnote_python]: Python installation instructions from [here](https://github.com/astral-sh/uv). `uv` is a modern python manager with similar benefits to using `nvm` for Node.js. If `uv python pin` does not work, you can try to manually override the existing symlink by running `sudo ln -fs ~/.local/bin/python3.15 $(which python3)`.

[^footnote_javascript]: Node.js installation instructions from [here](https://nodejs.org/en/download#debian-and-ubuntu-based-linux-distributions). Node.js and dependency versions which the project currently uses in its tests can be found [here](./../../src/test-misc/ts-spec2/package.json).