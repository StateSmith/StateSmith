# TLDR
* if you just want it to work, use `.drawio` xml files
* if you want snazzy svg files and don't mind learning the quirks of different draw.io UI tools, use `.drawio.svg`.
    * Or avoid using the unofficial `vscode draw.io` extension which is rarely updated and has a number of bugs/quirks.

<br>


# Which to choose?

StateSmith recognizes the following file extensions as draw.io files:
* `.drawio` (regular xml) - generally works better. **Recommended.**
* `.drawio.svg` (encoded xml) - works well with official draw.io application
    * unofficial vscode draw.io extension has numerous quirks
* `.dio` (regular xml)


<br>

## `.drawio`/`.dio` (regular XML)
This is a regular XML file. It generally just works well with all draw.io tools.

Pros:
* draw.io offline app opens this type automatically (just double click).
* vscode draw.io extension recognizes this type automatically.
* Can see changes in design XML with git.

Cons:
* Not an image that can be viewed without draw.io. Can be exported as SVG or PNG though.
* Can be less useful for pull requests (depending on platform).

<br>

## `.drawio.svg` (encoded XML)
This is a Scalable Vector Graphics (SVG) image file. It is still editable by draw.io because the draw.io XML is encoded inside it.

Can work well as long as you are aware of subtle UI quirks/bugs. Mainly in the vscode draw.io extension which is rarely updated.

Pros:
* Design file is a valid SVG image that can be used in markdown and other files!
* Can be really great for pull requests as you can see image differences.
* vscode drawio extension recognizes this type automatically.

Cons:
* vscode extension [has bugs to watch out for](https://github.com/StateSmith/StateSmith-drawio-plugin/issues/25) with svg files.
* When we add multiple page support, vscode extension [has another quirk](https://github.com/StateSmith/StateSmith/issues/380).
* The design is encoded inside the SVG file. Might want to treat as a binary file for git.
* A bit harder to [see XML](https://drawio-app.com/blog/extracting-the-xml-from-mxfiles/).


<br>

## Converting between types
The file choice isn't too important as you can easily switch between them. Pick one and try it.

