# Contributing

## Making Thunderstore packages locally

Build the project while manually specifying the `PackThunderstore` target: `dotnet build -target:PackThunderstore` (run on the commandline).  
For trying out the automatic publishing of the Thunderstore package, use `dotnet build -target:PublishThunderstore`. This won't actually publish the package unless if you have the `TCLI_AUTH_TOKEN` environment variable set to the valid value for an API token for the Thunderstore team.

## For Maintainers

### Publishing New Releases

> [!NOTE]  
> This project uses [MinVer](<https://github.com/adamralph/minver>) for versioning via git tags.

New releases can be published by creating a new GitHub release, and setting the tag to a valid SemVer version, prefixed with `v`. GitHub Actions will then build the new version and automatically attach files to the new release, so don't add any files to the release manually. The Thunderstore package and NuGet package also get published.

- Examples: `v1.0.0`, `v1.1.1`.
- To release a prerelease version, use the `dev`-prefix: `v1.1.1-dev`, `v1.1.1-dev.0`. Don't release a prerelease version though, since those aren't supported on Thunderstore and we haven't setup a separate Thunderstore package for a beta version.
