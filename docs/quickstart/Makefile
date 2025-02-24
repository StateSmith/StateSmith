# Assumes that ss.cli and plantuml are in your path
REPO_ROOT := $(shell git rev-parse --show-toplevel)
SITE_DIR := $(REPO_ROOT)/_site


all: \
	$(SITE_DIR)/docs/quickstart/lightbulb.svg \
	$(SITE_DIR)/docs/quickstart/lightbulb.puml \
	$(SITE_DIR)/docs/quickstart/lightbulb.sim.html \


# _site/**/*.svg depends on _site/**/*.puml
$(SITE_DIR)/%.svg: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && plantuml -tsvg $(<F)

# _site/**/*.sim.html depends on _site/**/*.puml
$(SITE_DIR)/%.sim.html: $(SITE_DIR)/%.puml
	mkdir -p $(@D)
	cd $(@D) && ss.cli run --lang=JavaScript --no-ask --no-csx -h -b

# _site/**/*.puml depends on **/*.puml
$(SITE_DIR)/%.puml: $(REPO_ROOT)/%.puml
	mkdir -p $(@D)
	cp $< $@

