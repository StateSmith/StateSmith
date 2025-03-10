# Integrating with Arduino

StateSmith is designed specifically to work in low memory, low performance situations such as microcontrollers, which makes it great for using with Arduino projects!

## Prerequisites

This tutorial assumes you have completed the following sections before proceeding:
* [C aka C99](/StateSmith/languages/c/)


## Running StateSmith as part of your Ardunio IDE build

Arduino IDE has the ability to define prebuild hooks that execute before your project is compiled. They're global rather than per-project, but we can make them work on a per-project basis fairly easily using the following technique.

> NOTE: If you ever re-install your platform or IDE, you will need to redo these edits in `platform.txt`


#### Make a global hook to run prebuild.sh

You're going to make a `prebuild.sh` script that is going to run statesmith for you. But before you do that, you
need to make sure that arduino will call your `prebuild.sh` script.

1. First, find your platform directory. Turn on verbose logging for `compile` output in Arduino IDE > Settings. Run a build, and 
   among the first few lines you'll see something like the following. This directory is your platform directory.
   ```
   Using core 'esp32' from platform in folder: ~/Library/Arduino15/packages/esp32/hardware/esp32/3.2.0/
   ```
2. Edit the `platform.txt` in that directory and add the following line:
   ```
   recipe.hooks.sketch.prebuild.234.pattern=/usr/bin/env bash -c "if [ -x {build.source.path}/prebuild.sh ] ; then cd {build.source.path} && {build.source.path}/prebuild.sh; fi"
   ```
   This line will look for an executable `prebuild.sh` in your sketch directory and run it if present.
3. Run a build in Arduino with verbose logging enabled, and search for `prebuild.sh` in the output. You should see a log line
   showing the `bash` command that was run from the hook definition. In this case, the hook didn't find `prebuild.sh` so nothing 
   else was done.

Now you have a hook that will run every time Arduino builds a sketch. This is done globally for all skteches, but as long as your sketches do not have a `prebuild.sh` script it won't have any effect.

#### Make `prebuild.sh`

Now that you have the hook, you can implement the script that will run StateSmith and PlantUML. Put the following in `prebuild.sh` in your sketch directory, replacing `FILENAME` with the name of your diagram file.

```sh
#!/bin/sh

# For this to work, you need to put a prebuild hook in your platform.txt
# platform.txt gets replaced whenever you install a new platform version,
# so you need to put this hook in every time you update the platform.
#
# Put this in platform.txt:
#   recipe.hooks.sketch.prebuild.234.pattern=/usr/bin/env bash -c "if [ -x {build.source.path}/prebuild.sh ] ; then cd {build.source.path} && {build.source.path}/prebuild.sh; fi"
#
# The prebuild.NUMBER needs to be unique in the file.
#
# Once the prebuild hook is in place, it will execute a prebuild.sh script if it exists in the sketch directory.
#
# You also must have plantuml and statesmith installed and in your path

# fail the shell on error
set -e

plantuml -tsvg FILENAME.puml
statesmith --lang=CPP FILENAME.puml
mv FILENAME.cpp FILENAME.inc.h
```

Make sure your file is executable
```sh
chmod +x prebuild.sh
```

Now run a build in Arduino IDE and verify that you see StateSmith and plantuml successfully run in the verbose logging output.


> TIP: `prebuild.sh` is re-run every time you build whether you've made any changes or not. You might consider using
> a make Makefile to only run statesmith when necessary. Instructions for doing so are beyond the scope of this document.