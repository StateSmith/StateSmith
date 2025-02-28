# The root directory
REPO_ROOT := $(shell git rev-parse --show-toplevel)

# The common build rules
include $(REPO_ROOT)/docs/build/docs.mk

# The list of files to build, relative to SITE_DIR
all: \
	$(SITE_DIR)/languages/c/lightbulb.svg \
	$(SITE_DIR)/languages/c/lightbulb.puml \
	$(SITE_DIR)/languages/c/lightbulb.c \
	$(SITE_DIR)/languages/c/lightbulb.sim.html
