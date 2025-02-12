# Releasing new distributions

Releases are primarily managed through GitHub Actions

1. Tag a new release with a tag in the form of `v*.*.*`
1. Update the release notes manually.
1. `.github/workflows/release-binaries.yml` will build binaries, run tests, and generate a release page with binaries.
1. `.github/workflows/release-homebrew.yml` will update homebrew with the new release.
