# Creating State Machines with PlantUML 

PlantUML is the preferred state machine language on the StateSmith documentation website, so you'll find many examples as you browse around the documentation.

<table>
<tr>
<td>
<img src="lightbulb.svg">
</td>
<td>
<pre>
{% include_relative lightbulb.puml %}
</pre>
</td>
</tr>
</table>

## (Optional) Install PlantUML

If you installed `StateSmith` the recommended way, PlantUML was included as a dependency on Mac and Linux. For Windows users, follow the installation instructions on https://plantuml.com/ if you would like to generate images from your PlantUML files (recommended).

TODO verify dependency for Mac and Linux

## Generate images from `.puml` files

Generating an image is easy. You can easily choose between image formats such as SVG or PNG:

```
% plantuml -tsvg lightbulb.puml
% plantuml -tpng lightbulb.puml
```

## Generating source code

Using StateSmith to generate source code in the language of your choice is easy.

```
% statesmith lightbulb.puml
```

This will generate JavaScript by default (since most everyone can run JS conveniently in a browser). Visit the `Languages` section to see how to generate other supported languages.

