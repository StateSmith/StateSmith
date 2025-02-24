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
	$(SITE_DIR)/quickstart/lightbulb.sim.html \


# _site/**/*.svg depends on _site/**/*.puml
$(SITE_DIR)/%.svg: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && plantuml -tsvg $(<F)

# _site/**/*.sim.html depends on _site/**/*.puml
$(SITE_DIR)/%.sim.html: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && ss.cli run --lang=JavaScript --no-ask --no-csx -h -b

# _site/**/*.puml depends on **/*.puml
$(SITE_DIR)/%.puml: $(DOCS_DIR)/%.puml
	mkdir -p $(@D)
	cp $< $@

