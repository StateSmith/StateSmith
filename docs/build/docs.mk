
# Assumes that statesmith and plantuml are in your path

# In general,
#   1. Copy the diagram file (*.puml) to the site directory
#   2. Generate the SVG file from the diagram file, both in the site directory
#   3. Generate the simulator and code file(s) from the same diagram file

# The docs directory
DOCS_DIR = $(REPO_ROOT)/docs

# The generated _site directory
SITE_DIR := $(DOCS_DIR)


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

%.svg: %.puml
	cd $(@D) && plantuml -tsvg $(<F)

# copy the diagram code to the destination directory
$(SITE_DIR)/%.puml: $(DOCS_DIR)/%.puml
	mkdir -p $(@D)
	cp $< $@

