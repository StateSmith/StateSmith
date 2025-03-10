
# Assumes that statesmith and plantuml are in your path

# In general,
#   1. Copy the diagram file (*.puml) to the site directory
#   2. Generate the SVG file from the diagram file, both in the site directory
#   3. Generate the simulator and code file(s) from the same diagram file

# The docs directory
DOCS_DIR = $(REPO_ROOT)/docs

# The generated _site directory
SITE_DIR := $(REPO_ROOT)/_site



# generate the diagrams in the destination directory
$(SITE_DIR)/%.svg: $(SITE_DIR)/%.puml
	cd $(@D) && plantuml -tsvg $(<F)

# generate the code in the destination directory
$(SITE_DIR)/%.sim.html: $(SITE_DIR)/%.puml
	cd $(@D) && statesmith --lang=JavaScript $<

# generate the code in the destination directory
$(SITE_DIR)/%.c: $(SITE_DIR)/%.puml
	cd $(@D) && statesmith --lang=JavaScript $<

# copy the diagram code to the destination directory
$(SITE_DIR)/%.puml: $(DOCS_DIR)/%.puml
	mkdir -p $(@D)
	cp $< $@

