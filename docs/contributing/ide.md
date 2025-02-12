# Using an IDE



## Visual Studio (Windows-only)

On Windows I recommend the free edition of [Visual Studio](https://visualstudio.microsoft.com/downloads/). It just works. It is free for open source collaborators.

#### Building

TODO


#### Running Tests
Open the test explorer in Visual Studio.

![image](https://user-images.githubusercontent.com/274012/218225543-4cf390a7-4816-45c5-9428-21f305eb0d9a.png)

Then click on the green arrows to run the selected/all tests.

![image](https://user-images.githubusercontent.com/274012/218225643-b7e7389b-e81c-4385-8740-7af57e136a3a.png)


#### Running ss.cli

TODO




## VS Code (any platform)
Expect a bit of googling to figure this out. Hit us up on discord if you need help.

These instructions are for the older vscode C# extension where `OmniSharp` is used. I don't know how to use the latest C# dev tools extension (I don't use it because it doesn't support .csx files).

#### Building

From a file in the StateSmith repo, select `Run Build Tasks`
<img width="609" alt="Run Build Tasks" src="https://github.com/user-attachments/assets/ad4a9bbe-2499-41b8-83a9-03dc28c7d3cf" />


#### Running Tests
Make sure `Code Lens` is enabled in vscode settings.

![image](https://github.com/StateSmith/StateSmith/assets/274012/b79e9bbc-8aee-4667-a092-ef0e99fd88fc)

Then scroll down a bit and enable these as well:

![image](https://github.com/StateSmith/StateSmith/assets/274012/ba1f0be9-225b-484a-9b67-0a2048bae3f5)

C# `Code Lens` is a bit glitchy. You sometimes have to give it a few seconds or reload the vscode window.

You can then navigate to some test code and run individual tests or debug them:
![image](https://github.com/StateSmith/StateSmith/assets/274012/e0754ed2-189e-420e-aefa-4ade4fdd4d72)

Please let me know if you needed any other steps to get it working for you.

#### Running ss.cli

TODO
