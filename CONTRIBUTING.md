# Contributing

## Making Thunderstore packages locally

Build the project while manually specifying the `PackThunderstore` target: `dotnet build -target:PackThunderstore` (run on the commandline).  
For trying out the automatic publishing of the Thunderstore package, use `dotnet build -target:PublishThunderstore`. This won't actually publish the package unless if you have the `TCLI_AUTH_TOKEN` environment variable set to the valid value for an API token for the Thunderstore team.

## For Maintainers

### Publishing New Releases

> [!NOTE]  
> This project uses [MinVer](<https://github.com/adamralph/minver>) for versioning via git tags.

New releases can be published by pushing a new git tag prefixed with `v`. This publishes Thunderstore and NuGet packages with that version. Examples:

- Valid tags: `v1.0.0`, `v1.1.1`
- NOT valid: `v1`, `v1.0`, `1.0.0`

To release a prerelease version, use the `dev`-postfix:

- Valid tags: `v1.0.0-dev`, `v1.1.1-dev`
- NOT valid: `v1-dev`, `v1.0-dev`, `v1.0.0-dev.0`, `1.0.0-dev`

Don't release a prerelease version though, since those aren't supported on Thunderstore and we haven't setup a separate Thunderstore package for a beta version.

### Ensuring Validity of Packages Before Release

A build workflow is run on every commit on the `main` branch, including on every PR (needs to be approved first). The workflow tries to mirror the publish workflow as closely as possible, and uploads the package artifacts for the workflow. For downloading these artifacts, see: <https://docs.github.com/en/actions/managing-workflow-runs-and-deployments/managing-workflow-runs/downloading-workflow-artifacts>
