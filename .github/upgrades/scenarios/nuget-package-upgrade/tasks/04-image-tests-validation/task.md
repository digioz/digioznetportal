# 04-image-tests-validation: Add image tests and validate the .NET 10 solution

Add focused NUnit coverage using generated temporary images for landscape and portrait resize, centered crop dimensions/content, JPEG output and decode round-trip, and relevant failure behavior. Clean the Tests package reference, verify live package metadata and the complete dependency graph, and execute all requested restore, audit, build, and test gates.

**Done when**: Focused tests pass; no .NET 10 source or package graph resolves ImageSharp; stable SkiaSharp version, MIT license, net10.0 compatibility, and advisory status are verified; audited restore and vulnerability checks are clean; Debug and Release solution builds succeed; full tests pass; and the `9.0` tree remains unchanged.
