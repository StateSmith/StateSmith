
We will eventually add a new supported front end other than yEd, but for now, here's how to use it well.
It's actually a pretty great program once you get used to it.

# Settings to save your sanity

## `File > Preferences > Editor`
- Ensure that `Create Node on Background Click` is deselected! This appears to be a default in the latest versions of yEd so you hopefully don't need to worry about it.
- ⚠️ Ensure that `Synchronize Open and Closed Group Label` is selected! Otherwise the label for a group can be different depending on whether it is open or closed. Bad times.
- Adjust the remaining settings as you see fit. I generally leave them alone.

![picture 1](../images/yed-preferences-editor.png)  

## `File > Preferences > General`
- I recommend setting `Tooltips` to `No Tooltips`. Otherwise, it is super distracting.

![picture 2](../images/yed-preferences-general.png)  


# Changing edge style
Open the Pallette from `Windows > Pallette` and open the `Edge Types` tab. Choose the edge style that you like. Right click on it and click `Use as Default`.

![picture 15](../images/docs-yed-edge-types.png)  

# Cautions
## yEd does not detect file changes
yEd will not detect when a .graphml changes from something like changing git branches. yEd loads the diagram into RAM and then writes to disk. It's a one way street. If you do any git operations or file modifications outside of yEd, make sure to close and re-open the file in yEd.

## yEd hidden edges
One particularly annoying yEd bug is that it is possible to accidentally draw an edge from a substate to its parent that disappears. The edge is still there, you just can't see it. If you notice this happen, you can open the `Neighborhood` view with the state selected. It will show the hidden edge and allow you to select and delete it. Auto edge routing will sometimes move the edge so that it becomes visible too. This bug really makes me want a new UI we can control better and fix bugs in.

# Training
I plan to make a short youtube video going over just what you need with yEd, but in the mean time, try opening
[./yed-training-tips.graphml](./yed-training-tips.graphml) with yEd and working through the exercises.

# General Tips

By default, mouse wheel zooms in/out. Right click and drag pans.

How to add or remove an existing node to/from groups see http://yed.yworks.com/support/qa/6053/is-it-possible-to-add-move-an-object-to-existing-group

When pasting, right click and consider using `Paste into selected group`.

## Auto label placement
The auto label placement can be pretty helpful when applied to edges. It also changes the label positioning model to `Free` which allows more customized placement. Wish I could make that the default.

Open dialog from `Layout > Label Placement`. Ensure that `Place Node Labels` is not selected and `Place Edge Labels` is. 

This is a handy thing to have well... handy. I dock it and use these settings

![picture 16](../images/docs-auto-labelling-settings-docked.png)  


# More...

yWorks training [youtube videos](https://www.youtube.com/watch?v=OmSTwKw7dX4&list=PLpIlEtPgrZsUA28PlP2tdRERcMIVn5AUT&index=1&ab_channel=yWorks).

Please share your tips if you find something that works well.

# TODO
- Document best edge router settings. User feedback also appreciated here.
