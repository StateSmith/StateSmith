## Create a new project
Issue the following command.
```
ss.cli create
```

This will bring up a wizard that guides you through quickly creating a new StateSmith project. It remembers your choices for the next time you run the command so you should only need to enter in the project name and then hit enter a few times to create a new project.

Here's a [2 minute video](https://www.youtube.com/watch?v=niiIDh5GHy0) showing it in action with a bit of extra advice.

![](https://github.com/user-attachments/assets/7c00c5f5-baa3-4c25-94e4-53ac87069615)


<br>


## Run Code Generation
The `run` verb has numerous options.

Run code gen recursively in the current directory:
```sh
ss.cli run -hr
```
or
```sh
ss.cli run --here --recursive
```

### Change Detection
By default, `ss.cli` will skip running code generation for a diagram/.csx file if it detects that no changes have been made to the input or output files. If you want to force code generation to run, you can use the `--rebuild` (`-b`) option.

If you want to see what's being checked, you can use the `--verbose` (`-v`) option.

```
ss.cli run -hr --verbose
```
![image](https://github.com/user-attachments/assets/36e8021c-779e-41f4-8a40-9d818461f09b)

<!--
Checking diagram: `MySm.plantuml`
Diagram settings: Lang: NotYetSet, NoSimGen: False
File `MySm.plantuml` hasn't changed since last code gen.
File `MySm.h` hasn't changed since last code gen.
File `MySm.c` hasn't changed since last code gen.
File `MySm.sim.html` hasn't changed since last code gen.
Diagram and its dependencies haven't changed. Skipping.
-->

### Watch
> Available in ss.cli version 0.17.1 or greater.

Use the `--watch` (`-w`) option to have `ss.cli` watch the input files for changes and run code generation when a change is detected.

The watch feature is already quite useful, but I think it could use some refinement. It checks the monitored files once a second (with some other sleeps) to keep CPU usage very low. On my system, it shows as 0% when tracking 10 diagrams.

```
ss.cli run -hrw
```

<!-- 
Watching the following diagram files:
- enemy0/Enemy0Sm.drawio
- enemy1/Enemy1Sm.drawio
- enemy2/Enemy2Sm.drawio
- enemy3/Enemy3Sm.drawio

Watching files for changes...

File `enemy0\Enemy0Sm.drawio` CHANGED since last code gen. Code gen needed.
Running diagram: `enemy0/Enemy0Sm.drawio`

StateSmith lib ver - 0.17.0+45686238724166dee85ac2fd06d4fed4f99951c5
StateSmith Runner - Compiling file: `enemy0/Enemy0Sm.drawio` (no state machine name specified).
StateSmith Runner - State machine `Enemy0Sm` selected.
StateSmith Runner - Writing to file `enemy0/Enemy0Sm.js`
StateSmith Runner - Writing to file `enemy0/Enemy0Sm.sim.html`
StateSmith Runner - Finished normally.

Successful sources: 1
Watching files for changes...
 -->

![image](https://github.com/user-attachments/assets/ada45695-870a-4b59-be18-cecad0d05204)


There's a summary printed at the end so you can easily check for errors.

![image](https://github.com/user-attachments/assets/c20eee77-652d-4945-9200-60953d7cf3aa)


<!-- 
```
Successful sources: 2
Failed sources:
- enemy3/Enemy3Sm.drawio
``` -->

### More `run` Options
```sh
ss.cli run --help
```

```
  -h, --here                Runs code generation in this directory.

  -r, --recursive           Recursive. Can't use with -i.

  -w, --watch               Watch input files for changes.

  -x, --exclude             Glob patterns to exclude

  -i, --include             Glob patterns to include. ex: `**/src/*.csx`. Can't
                            use with -r.

  -b, --rebuild             Ensures code generation is run. Ignores change
                            detection.

<snip...>
```


<br>

## Version Info
```sh
ss.cli --version
```
You should see output similar to the following:

```
Using settings directory: /home/afk/.local/share/StateSmith.Cli
StateSmith.Cli 0.14.0+355742942da782ea2fa41b89efb45b9542e6cc79
```



<br>


## More Usage Info
You can run `ss.cli` by itself to bring up a menu of options. You can also run `ss.cli --help` to see a list of available verbs.


```sh
ss.cli --help
```

You should see output similar to the following:

```
Usage:

  run       Run StateSmith code generation.

  create    Create a new StateSmith project from template.
  
  setup     Set up vscode for StateSmith & csx files.

To get help for a specific verb, use the command name followed by --help
```

<br>

## Manifest files [optional]
Here's an older video walkthough that is missing many new features, but it explains manifest files well.<br>
https://www.youtube.com/watch?v=2y1tLmNpz78


<br>

## Ignore Files
If you have a diagram or .csx file that you don't want `ss.cli` to try and use, you can put the below special string anywhere in that document.
```
statesmith.cli-ignore-this-file
```

You can also manually use the `ss.cli run` exclude option: `-x, --exclude`


<br>

## more info
See [CLI Wiki Root Page](https://github.com/StateSmith/StateSmith/wiki/CLI:-Command-Line-Interface)
