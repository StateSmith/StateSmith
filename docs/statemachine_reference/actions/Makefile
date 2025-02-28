# The root directory
REPO_ROOT := $(shell git rev-parse --show-toplevel)

# The common build rules
include $(REPO_ROOT)/docs/build/docs.mk

# The list of files to build, relative to SITE_DIR
all: \
	$(SITE_DIR)/statemachine_reference/actions/lightbulb.svg \
	$(SITE_DIR)/statemachine_reference/actions/lightbulb.puml \
	$(SITE_DIR)/statemachine_reference/actions/lightbulb.sim.html
