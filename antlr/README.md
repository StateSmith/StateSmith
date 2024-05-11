StateSmith uses ANTLR4 grammar (.g4) files. There are lots of good tutorials and ebooks online.

To be perfectly honest, the StateSmith grammars are not perfect. Contributions welcome :)


<br>

# Requirements
Please use WSL2 or linux (mac?) to run the scripts in this directory.

Please let me know if any of the files have bad chmod permissions (sometimes happens when I use wsl).

## (Windows only step) WSL2 installation that allows GUI use
https://ubuntu.com/tutorials/install-ubuntu-on-wsl2-on-windows-11-with-gui-support#1-overview

## install java jdk for antlr4
`sudo apt install default-jdk`

If you want to uninstall java later, see https://stackoverflow.com/questions/23959615/how-to-remove-default-jre-java-installation-from-ubuntu

## Download antlr
Run `./install.sh` script.


<br>

# Editing the grammar files
Highly recommend using vscode and this extension https://marketplace.visualstudio.com/items?itemName=mike-lischke.vscode-antlr4

![image](https://github.com/StateSmith/StateSmith/assets/274012/052549ca-7c50-4a0d-afdd-f138686b8441)

It gives highlighting, navigation and live error checking!

One very useful feature is to enable the railroad diagram.

![image](https://github.com/StateSmith/StateSmith/assets/274012/b8755aa7-e62c-4a66-ae80-19ce53f6c455)

![image](https://github.com/StateSmith/StateSmith/assets/274012/05be7944-381c-44fb-8b5a-55b07b0cd78d)


<br>

# Compile the grammar(s)
Run `. setup.sh` to setup some variables to allow us to run antlr easily. Note the dot infront of the script file so that we [source it](https://stackoverflow.com/questions/9772036/pass-all-variables-from-one-shell-script-to-another).
```
. setup.sh
```

Now compile the grammars you are interested in using one of the scripts below. Note that we are sourcing these scripts as well.

```
. compile-plantuml.sh
. compile-ss.sh
. compile-all.sh
```



<br>

# Manually test
Put the text you want to parse in `test_input.txt`.

Edit the `test.sh` file to select the command you want. Or write your own. It's just a collection of samples.

Run the test script sourcing it as well.
```
. test.sh
```

If you ran the GUI, it can look a bit overwhelming at first for even tiny input files!

![image](https://github.com/StateSmith/StateSmith/assets/274012/0f537f0c-1179-462b-a745-4649e07997a5)

Use the tree explorer in the left tab to drill down and focus on areas one at a time.

![image](https://github.com/StateSmith/StateSmith/assets/274012/3650b568-b4b8-47f6-b55c-3f5e2bb20a74)

Read `test.sh` for more tips.




<br>

# Tips
## Add new keywords to identifiers as well
The `ignore` parse rule for PlantUML matches against the string literal `scale`. We also need to add `scale` to the list of possible identifiers. It would probably be better to use a common lexer rule instead of string literals..

```g4
ignore:
    //... snip
    'scale' HWS rest_of_line
    //... snip
    ;

identifier
    : IDENTIFIER
    | 'state'
    | 'State'
    | 'note'
    | 'end'
    | 'as'
    | 'scale'
    | 'skinparam'
    ;
```

