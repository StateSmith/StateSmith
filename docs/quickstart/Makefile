# Assumes that ss.cli and plantuml are in your path

# The root directory
REPO_ROOT := $(shell git rev-parse --show-toplevel)

# The docs directory
DOCS_DIR = $(REPO_ROOT)/docs

# The generated _site directory
SITE_DIR := $(REPO_ROOT)/_site


all: \
	$(SITE_DIR)/quickstart/lightbulb.svg \
	$(SITE_DIR)/quickstart/lightbulb.puml \
	$(SITE_DIR)/quickstart/lightbulb.sim.html 	

# In general,
#   1. Make copies the diagram file (*.puml) to the site directory
#   2. Make generates the SVG file from the diagram file, both in the site directory
#   3. Make generates the simulator and code file(s) from the same diagram file


$(SITE_DIR)/%.svg: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && plantuml -tsvg $(<F)

$(SITE_DIR)/%.sim.html: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && ss.cli run --lang=JavaScript --no-ask --no-csx -h -b

$(SITE_DIR)/%.puml: $(DOCS_DIR)/%.puml
	mkdir -p $(@D)
	cp $< $@

