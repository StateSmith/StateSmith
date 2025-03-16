---
title: Contributing to docs
parent: Contributing
layout: default
---

# Contributing to Documentation

The [docs](/) site is generated automatically on push from the `docs` directory using the [jekyll-gh-pages.yml](/jekyll-gh-pages.yml) Github Action workflow.

> **Common Gotchas!**
> * Watch out for broken URLs! Follow the Style guide below.
> * Test your changes! See the verification section below.


## Style guide

#### URLs
YES

```
# URLs relative to current directory
![lightbulb](lightbulb.svg)
![lightbulb](./lightbulb.svg)
![lightbulb](../lightbulb.svg)
[Quickstart](../quickstart/index.md)
[Quickstart](../quickstart/index.md)
<img src="light.svg" />
<iframe height="300" width="600" src="lightbulb.sim.html"></iframe>

# URLs relative to root begin with StateSmith, not docs
![lightbulb](/StateSmith/media/lightbulb.svg)
[Quickstart](/StateSmith/quickstart/index.md)
<img src="/StateSmith/media/light.svg" />
```

NO
```
![lightbulb](/media/lightbulb.svg) # URLs beginning with / must start with /StateSmith
![lightbulb](/docs/media/lightbulb.svg) # URLs do not start with /docs

```


#### Generated content using Makefiles

Some pages may rely on generated content, such as PlantUML SVGs, StateSmith codegen, etc. 

We use Makefiles to generate this content when we generate the pages themselves.

{: .note}
> Generally speaking, do not check in the generated `gen` directories.

An example is this Makefile from the `quickstart` directory:

```make
# The common build rules
include $(shell git rev-parse --show-toplevel)/docs/build/docs.mk

# The list of files to build in ./gen/ directory
all: \
	gen/lightbulb.sim.html
```
Simply copy the Makefile to your directory and replace the list of files in `all` with the files you need to generate. You can look at `docs.mk` to see the supported build rules or add new ones.

The Makefiles are executed by the Github Actions workflow that generates the Github Pages site.


## Verifying your changes

You need to verify that your changes render properly before you submit. To do that, you need to run the `pages.yml` locally before push. We use [Nektos Act](https://nektosact.com/) to do this.

```
# Install act using the package manager of your choice, eg. `brew install act`.
# You may also need to separately install docker. Check the Act installation instructions.
# Then from the root of your StateSmith repo, run the following.

StateSmith %   act -P ubuntu-latest=catthehacker/ubuntu:act-latest -W .github/workflows/pages.yml

# On subsequent runs, add --pull=false to avoid downloading the 15GB+ image again
StateSmith %   act --pull=false -P ubuntu-latest=catthehacker/ubuntu:act-latest -W .github/workflows/pages.yml
```

This will run your action in a docker instance on your local machine. The action will build the pages in `StateSmith/docs/_site`. (It will fail trying to publish the changes to git, but that's intentional.)

{: .highlight}
If you are on AMD or Apple Silicon, you will need to specify `--container-architecture linux/amd64`

{: .warning}
The size of the docker image and subsequent downloads is over 15GB


Then:
1. Open Docker Desktop and enable `Settings > Resources > Network > Enable host networking`
2. Run the following in your docker container's `Exec` tab: 
   ```
root@docker-desktop:/Users/mike/Documents/GitHub/StateSmith# cd docs
root@docker-desktop:/Users/mike/Documents/GitHub/StateSmith/docs# mv _site StateSmith
root@docker-desktop:/Users/mike/Documents/GitHub/StateSmith/docs# python3 -m http.server
Serving HTTP on 0.0.0.0 port 8000 (http://0.0.0.0:8000/) ...
   ```
3. Open a webrowser and connect to `http://localhost:8000/StateSmith` to verify it looks the way you expect. In particular verify any new links work as expected.

![docker_httpserver]( /StateSmith/media/docker_httpserver.png )


