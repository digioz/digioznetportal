# 01-migrate-images-to-skiasharp: Replace ImageSharp with SkiaSharp

Replace the .NET 10 solution's ImageSharp dependencies and APIs with SkiaSharp in dependency order. Refactor shared helper contracts and all Razor Pages and bulk-import callers while preserving file and byte loading, aspect-preserving resize, centered crop, JPEG quality 100, black placeholders, upload behavior, encoded formats, and deterministic ownership/disposal. Remove package references from projects without direct need rather than adding SkiaSharp everywhere.

Add focused NUnit coverage for landscape and portrait resize, center crop dimensions, JPEG output and round-trip, and relevant failure behavior using generated temporary images. Verify the live package version, MIT license, and advisory status, then run a fresh audited restore, confirm no ImageSharp remains in the .NET 10 package graph, build Debug and Release, and run relevant and full tests.

**Done when**: No .NET 10 source or package graph resolves SixLabors.ImageSharp; only projects that directly need SkiaSharp reference the verified stable version; all identified image behavior is preserved with safe disposal; focused tests pass; audited restore and vulnerability checks are clean; Debug and Release solution builds succeed; full relevant tests pass; and the `9.0` tree remains unchanged.

## Scope Inventory

- **Projects affected**: Utilities owns the shared image APIs; Web contains Razor Page callers; BulkMediaImport has two local image paths; Tests will gain focused coverage. Dal has an unused direct ImageSharp reference to remove. All projects use per-project package versions rather than Central Package Management.
- **Package state**: Web, Dal, Utilities, BulkMediaImport, and Tests currently reference ImageSharp 4.1.3 directly. Web also receives Utilities transitively. The replacement target is stable SkiaSharp 4.153.1, subject to final live-feed verification.
- **Distinct concerns**: package graph cleanup, shared API refactoring, Razor Page call-site migration, bulk thumbnail and placeholder migration, generated-image NUnit tests, and full audited validation.
- **Dependency order**: Utilities first; Web and BulkMediaImport next; Tests and solution-wide validation last.

## Research Findings

- `Helpers/ImageHelper.cs` exposes ImageSharp `Image` in two public overloads, performs downscale-only aspect-preserving resize, centered crop to exact dimensions, and JPEG quality 100. Its methods catch failures and return `false`.
- `Helpers/Utility.cs` has parallel resize/crop methods returning `SizeF` or `SizeF.Empty`. The path-based resize entry point can be migrated without exposing SkiaSharp to callers.
- Razor Page handlers for profile avatars, pictures, videos, and slideshows load ImageSharp images only to pass them to `ImageHelper`. File-path and byte-array helper overloads can preserve behavior while removing direct SkiaSharp coupling from PageModels.
- BulkMediaImport uses ImageSharp in `MediaImporter.cs` for a black 150x150 JPEG placeholder and contains an apparently unused private crop method. `VideoThumbnailHelper.cs` performs centered crop/resize and rethrows failures after logging.
- Dal has no ImageSharp source usage. Tests have no current image-library usage or image helper coverage. Existing NUnit utility tests use `[TestFixture]`, `[Category("Unit")]`, `[Category("Utilities")]`, FluentAssertions, and `Method_Scenario_ExpectedResult` names.
- SkiaSharp requires explicit disposal of decoded `SKBitmap`, resized/cropped intermediates, `SKImage`, and encoded `SKData`. JPEG output must be selected explicitly with quality 100 regardless of input extension.

## Decomposition Decision

The migration spans dependent projects and has a final validation gate, so it is split into shared helpers/package cleanup, Razor Page callers, bulk media paths, and tests/solution validation.
