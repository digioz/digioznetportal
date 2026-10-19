# 01.04-image-tests-validation: Add image tests and validate the .NET 10 solution

## Objective
Add focused NUnit coverage using generated temporary images, clean the Tests package reference, verify package metadata and graph, and run the complete requested validation.

## Scope
- `10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj`
- New utility image tests following existing NUnit conventions
- `10.0/digioz.Portal.sln` restore/build/test/package graph

## Required Coverage
Landscape and portrait aspect-preserving resize, centered crop dimensions/content, JPEG output and decode round-trip, and relevant failure behavior. Generate inputs at runtime and clean all temporary files.

## Required Validation
Re-verify stable SkiaSharp version, MIT license, net10.0 restore compatibility, and advisories; fresh restore with `NuGetAudit=true` and `NuGetAuditMode=all`; direct/transitive graph and vulnerability audit; no ImageSharp resolution; Debug and Release solution builds; focused and full tests; confirm the `9.0` tree is unchanged.

**Done when**: Focused tests pass; no .NET 10 source or package graph resolves ImageSharp; audits are clean; Debug and Release builds succeed; full tests pass; and results distinguish unrelated pre-existing warnings/skips.
