# 04-image-tests-validation: Add image tests and validate the .NET 10 solution

Add focused NUnit coverage using generated temporary images for landscape and portrait resize, centered crop dimensions/content, JPEG output and decode round-trip, and relevant failure behavior. Clean the Tests package reference, verify live package metadata and the complete dependency graph, and execute all requested restore, audit, build, and test gates.

**Done when**: Focused tests pass; no .NET 10 source or package graph resolves ImageSharp; stable SkiaSharp version, MIT license, net10.0 compatibility, and advisory status are verified; audited restore and vulnerability checks are clean; Debug and Release solution builds succeed; full tests pass; and the `9.0` tree remains unchanged.

## Research Findings

- The Tests project directly references ImageSharp 4.1.3 but has no image source usage. Focused generated-image tests require a direct SkiaSharp 4.153.1 reference, so this is a replacement rather than simple removal.
- Existing utility tests use NUnit `[TestFixture]`, `Unit` and `Utilities` categories, FluentAssertions, and `Method_Scenario_ExpectedResult` naming under `Unit/Utilities`.
- Tests will create an isolated directory under the OS temporary path during setup and delete it recursively during teardown; no binary fixtures or repository outputs will be added.
- Generated images will use solid/striped `SKBitmap` inputs encoded in memory. Assertions will cover landscape 100x50 and portrait 50x100 downscales, centered 100x100 crop content, JPEG signature plus decode round-trip, source-format-preserving PNG resize, and false/no-output behavior for invalid input.
- The solution contains eight SDK-style net10.0 projects. CLI `dotnet restore/build/test` is appropriate for audited and configuration-specific validation; IDE build remains available for final diagnostic confirmation.
