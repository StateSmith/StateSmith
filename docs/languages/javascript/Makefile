# The common build rules
include $(shell git rev-parse --show-toplevel)/docs/build/docs.mk

# The list of files to build in ./gen/ directory
all: \
	gen/lightbulb.sim.html \
	gen/lightbulb.js \
	gen/myapp.html

# Copy myapp.html to the gen directory so that
# the relative paths are the same for people following
# the tutorial in the docs as they are for us.
gen/myapp.html: myapp.html
	@mkdir -p $(@D)
	@cp $< $@