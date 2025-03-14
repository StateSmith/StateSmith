# Integrating with Makefiles

You can use the following Make rules to generate statesmith code.

```make
# This rule says that to get a .c, .h, and a .sim.html file, you need 
# to run statesmith on the corresponding .puml file.
%.c %.h %.sim.html&: %.puml
	statesmith --lang=C99 $<

%.cpp %.hpp %.sim.html&: %.puml
	statesmith --lang=cpp $<

%.cs %.sim.html&: %.puml
	statesmith --lang=csharp $<

%.java %.sim.html&: %.puml
	statesmith --lang=Java $<

%.js %.sim.html&: %.puml
	statesmith --lang=JavaScript $<

%.py %.sim.html&: %.puml
	statesmith --lang=Python $<
```

An example Makefile using the above might look like the following:

```make
# Assuming you have myapp.c and lightbulb.puml, you can generate
# the statemachine code and compile the app using the following rule,
# which will look for myapp.c and find it, and look for lightbulb.c 
# and create it using the rules above when it isn't found
myapp: myapp.c lightbulb.c
    g++ -o $@ $<

# ... don't forget to include the above statesmith rules below ...
```

## Generating SVG diagrams

You can use plantuml to generating SVG diagrams. Plantuml should be automatically installed if you installed statesmith the default way.

```make
%.svg: %.puml
	cd $(@D) && plantuml -tsvg $(<F)
```