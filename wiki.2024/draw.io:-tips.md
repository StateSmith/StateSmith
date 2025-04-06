# 📢 Common Beginner Issue 📢
Most beginners will run into the issue of draw.io showing nodes as grouped when they actually aren't.

See https://github.com/StateSmith/StateSmith/wiki/Troubleshooting#non-related-nodes-overlap




<br>



# "Proper" Dark Mode Colors
By default, draw.io dark mode uses a "cheap css filter" that many people don't like. You can [easily disable that here](https://github.com/jgraph/drawio/discussions/3701).


<br>


# Make Some Space
It can be helpful to collapse the sidebars (especially if working on a laptop).

![make-space](https://github.com/user-attachments/assets/c2b032b6-3f06-4022-b409-c5b605c0fe02)


<br>


# Pan
Hold the right mouse button or scroll wheel button while moving the mouse to pan around.

![pan](https://user-images.githubusercontent.com/274012/228716949-10e418a5-f66c-4913-bd6b-eb8a3e375579.gif)


<br>


# Connecting Shapes
Hover over a shape to see its connection points. Then use those connection points to connect to another shape.

![add-transitions](https://user-images.githubusercontent.com/274012/229119836-9f2afc9d-4237-4cc0-a39b-e36bae7d9337.gif)


<br>


# Editing Labels
* Shape label: Double click it or use F2.
* Transition label: select it and start typing or use F2.

Use `CTRL+ENTER` to finish editing, or click somewhere else.

![editing-labels](https://user-images.githubusercontent.com/274012/229120032-4a523168-2f7c-4d7a-8df5-cd30a674d094.gif)


<br>


# Reposition Transition Labels
Select a transition, then use the diamond control.

![reposition-labels](https://user-images.githubusercontent.com/274012/229120181-6389ec6e-fe07-401d-b5b5-b6c87f7e0ba6.gif)


<br>


# Edge Waypoints
![edge-waypoints](https://user-images.githubusercontent.com/274012/229260155-c9581cca-9a73-473a-9cc8-e8343641d719.gif)



<br>



# Styling
Feel free to style the shapes however you want.

![styling-2](https://user-images.githubusercontent.com/274012/229120306-bf62e282-1c88-4d4d-9327-b098e11068fc.gif)

A state's nested text node has [some restrictions](https://github.com/StateSmith/StateSmith/wiki/draw.io:-styling-restrictions) so that StateSmith can parse the design properly.



<br>



# Clone And Connect
Note how the plugin automatically renames the state to help save you a bit of typing.

![clone-and-connect](https://user-images.githubusercontent.com/274012/228718272-4fde1ff3-0374-4f4f-9847-5bd7b5879439.gif)


<br>


# Clone With Move
Hold `CTRL` while moving a shape to clone it.

![move-and-clone](https://user-images.githubusercontent.com/274012/229667250-5988af03-4af6-49cb-bf39-803d85c66622.gif)



<br>



# Delete A Shape With Connected Edges
`CTRL + delete` to delete a vertex and connected edges (edges are normally left hanging).


<br>


# Group States [without plugin]
This is easier [with the plugin](https://github.com/StateSmith/StateSmith-drawio-plugin/wiki/How-to-Use#group-states).

DO NOT use normal draw.io grouping to group states into a parent state.

![bad-group](https://github.com/user-attachments/assets/3f5f5828-13e2-4901-a005-07139e4738bd)

Instead, get a new state to be the parent (can copy paste an existing), and then move existing into that parent state.

![good-group](https://github.com/user-attachments/assets/bb129640-eb2f-4d93-8d79-1efd67d0e157)


<br>



# Enter/Exit A Group [without plugin]
This is easier with the [plugin](https://github.com/StateSmith/StateSmith-drawio-plugin/wiki/How-to-Use#enterexit-a-group).

Hotkeys:
* enter a group: `CTRL+SHIFT+END`
* exit a group: `CTRL+SHIFT+HOME`
* exit to root: `SHIFT+HOME`

Or use `Arrange > Layout` menu.

![image](https://github.com/user-attachments/assets/ff9f1bff-7336-4d3f-870d-223e04b811ed)


<br>



# Visually Collapse A State
Press the small `-` button in the top left corner to visually collapse a state/group. Your design functions the exact same way regardless of whether a state is visually expanded or collapsed. When state machines start to get large and some level of abstraction is needed, simply collapse the state and leave it collapsed.

> Tip: entry and exit points are very useful with collapsed states.

See `Copy Paste Size` below for a simple way of keeping the shape the same size.

![collaspe-state](https://github.com/StateSmith/StateSmith-drawio-plugin/assets/274012/8009507b-b165-4b61-8a47-2f7433014352)



<br>



# Copy Paste Size
This is handy in a number of situations, but particularly when collapsing a state and you want its size to remain the same.

![paste-size](https://github.com/StateSmith/StateSmith-drawio-plugin/assets/274012/590a41cd-ebc6-4aa3-a4ef-6854c5694f09)


<br>



# Scratchpad
This is super useful!

![scratch-pad](https://github.com/user-attachments/assets/8a4e9a28-4af8-4bf3-9ccd-0ebfa3a2f387)

> The scratchpad is your personal shape library where you can add the styled shapes or groups of shapes that you use the most often. You can drag a shape/group of shapes from the drawing canvas onto your scratchpad, and then drag new copies from the scratchpad back onto the drawing canvas whenever you need them.

https://drawio.freshdesk.com/support/solutions/articles/16000042367-use-the-scratchpad-in-diagrams-net


<br>


# Zoom Extents
Hit `enter` or zoom toolbar.

![image](https://github.com/user-attachments/assets/e24f02fe-84b9-48a3-9fd2-aa8ead34b250)


<br>




# Styling Fonts
Style the fonts however you want, but [don't style their outer shape though](https://github.com/StateSmith/StateSmith/wiki/draw.io:-styling-restrictions).

![style-fonts](https://user-images.githubusercontent.com/274012/229119896-f7e46f5f-f191-4852-8233-3bef2b364b06.gif)


<br>



# Add Shape Connection Points
If you need more connection points on a shape, you can easily add them.
[4:36 minute video](https://www.youtube.com/watch?v=GCd38a2NsQQ&ab_channel=StateSmith).

[![image](https://user-images.githubusercontent.com/274012/233871376-ccff4bd7-d02e-4e3a-9891-9299fa15db11.png)](https://www.youtube.com/watch?v=GCd38a2NsQQ&ab_channel=StateSmith)


<br>



# Floating vs Fixed Connections
https://www.youtube.com/watch?v=xM04I-WVXlE&ab_channel=draw.io

[![image](https://user-images.githubusercontent.com/274012/233869408-d6677ba3-ca69-41b8-9f94-eb74a8cf258b.png)](https://www.youtube.com/watch?v=xM04I-WVXlE&ab_channel=draw.io)


<br>



# Edit Vertex Geometry
`CTRL + SHIFT + M` To bring up dialog. Useful to resize all selected states at once.

[![image](https://user-images.githubusercontent.com/274012/233869446-8a20a4b1-4f40-4499-ae53-098419c63e9b.png)
](https://www.youtube.com/watch?v=AdiGnkpzZsk&ab_channel=draw.io)


<br>


# Sketch Style And Rounded Corners
The sketch style looks pretty fun. Might be nice for an online tutorial.

![sketch-style](https://user-images.githubusercontent.com/274012/229131118-0dab7dcd-762c-4ba2-9dc9-3f4ed5fda043.gif)


<br>



# Change Between Pages
* `CTRL+SHIFT+PG_UP`
* `CTRL+SHIFT+PG_DOWN`


<br>



# Shortcuts Cheat Sheet
https://drawio-app.com/blog/draw-io-diagramming-in-confluence-cheat-sheets-for-beginners/#


<br>



# Adding a Boatload of Connected States
Hold ALT when clicking on the sidebar palette to put down a new vertex and automatically connect it to the currently selected vertex. You can also hold SHIFT and or CTRL to change the direction of the arrow.

* ALT + click: add and join downwards.
* ALT + SHIFT + click: add and join to the right.
* ALT + CTRL + click: add and join upwards.
* ALT + SHIFT + CTRL: add and join to the left.


<br>



# draw.io tutorials
https://drawio-app.com/tutorials/