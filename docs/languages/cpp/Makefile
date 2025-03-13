# The root directory
REPO_ROOT := $(shell git rev-parse --show-toplevel)

# The common build rules
include $(REPO_ROOT)/docs/build/docs.mk

# The list of files to build, relative to SITE_DIR
all: \
	$(SITE_DIR)/languages/cpp/Lightbulb.svg \
	$(SITE_DIR)/languages/cpp/Lightbulb.puml \
	$(SITE_DIR)/languages/cpp/Lightbulb.cpp \
	$(SITE_DIR)/languages/cpp/Lightbulb.hpp \
	$(SITE_DIR)/languages/cpp/Lightbulb.sim.html
