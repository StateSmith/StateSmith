# Contributing to Documentation

The [docs](/) site is generated automatically on push from the `docs` directory using the [jekyll-gh-pages.yml](/jekyll-gh-pages.yml) Github Action workflow.

![WARNING]
> Common Gotchas!
> * Watch out for broken URLs! Follow the Style guide below.
> * Test your changes! See the verification section below.


## Style guide

#### URLs
TODO how to do URLS properly

YES

```
# URLs relative to current directory
![lightbulb](lightbulb.svg)
![lightbulb](./lightbulb.svg)
![lightbulb](../lightbulb.svg)
[Quickstart](../quickstart/index.md)
[Quickstart](../quickstart/index.md)

# URLs relative to root begin with StateSmith
![lightbulb](/StateSmith/media/lightbulb.svg)
[Quickstart](/StateSmith/quickstart/index.md)
```

NO
```
![lightbulb](/media/lightbulb.svg) # URLs beginning with / must start with /StateSmith

```

## Verifying your changes

You need to verify that your changes render properly before you submit. To do that, you need to run the `jekyll-gh-pages.yml` locally before push. We use [Nektos Act](https://nektosact.com/) to do this.

```
# Install act using the package manager of your choice, eg. `brew install act`.
# You may also need to separately install docker. Check the Act installation instructions.
# Then from the root of your StateSmith repo, run the following.

StateSmith %   act -P ubuntu-latest=catthehacker/ubuntu:act-latest -W .github/workflows/jekyll-gh-pages.yml

# On subsequent runs, add --pull=false to avoid downloading the 15GB+
# image again
StateSmith %   act --pull=false -P ubuntu-latest=catthehacker/ubuntu:act-latest -W .github/workflows/jekyll-gh-pages.yml

```

This will run your action in a docker instance on your local machine. The action will build the pages in `StateSmith/_site`. (It will fail trying to publish the changes to git, but that's okay.)

Then:
1. Open Docker Desktop and copy the `_site` folder from your docker instance to your host.
2. Make a folder inside `_site` called `StateSmith`, and move everything from `_site` into `_site/StateSmith`. This reproduces the url structure on the web, which starts all your URLs with `/StateSmith`.
3. Run `python3 -m http.server` from your `_site` directory.
3. Then open the URL logged by python with your browser to verify it looks the way you expect. In particular verify any new links work as expected.

![docker_site]( /StateSmith/media/docker_site.png )

Note: If you are on AMD or Apple Silicon, you will need to specify `--container-architecture linux/amd64`

Note: the size of the docker image and subsequent downloads is over 15GB

