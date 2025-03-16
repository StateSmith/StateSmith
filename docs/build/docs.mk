
# Assumes that statesmith and plantuml are in your path

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

%.sim.html: %.puml
	statesmith --lang=JavaScript $<

%.py %.sim.html&: %.puml
	statesmith --lang=Python $<

%.svg: %.puml
	cd $(@D) && plantuml -tsvg $(<F)

# copy the diagram code to the destination directory
gen/%.puml: %.puml
	mkdir -p $(@D)
	cp $< $@