# ImageSharp to SkiaSharp Migration Plan

## Overview

**Target**: Replace SixLabors.ImageSharp 4.1.3 with the latest stable SkiaSharp compatible with .NET 10, currently verified as 4.153.1, while preserving existing image behavior.
**Scope**: Eight .NET 10 solution projects, shared utilities, Razor Pages upload handlers, bulk media import, and focused NUnit tests. The separate `9.0` tree is excluded.

## Tasks

### 01-migrate-images-to-skiasharp: Replace ImageSharp with SkiaSharp

Replace the .NET 10 solution's ImageSharp dependencies and APIs with SkiaSharp in dependency order. Refactor shared helper contracts and all Razor Pages and bulk-import callers while preserving file and byte loading, aspect-preserving resize, centered crop, JPEG quality 100, black placeholders, upload behavior, encoded formats, and deterministic ownership/disposal. Remove package references from projects without direct need rather than adding SkiaSharp everywhere.

Add focused NUnit coverage for landscape and portrait resize, center crop dimensions, JPEG output and round-trip, and relevant failure behavior using generated temporary images. Verify the live package version, MIT license, and advisory status, then run a fresh audited restore, confirm no ImageSharp remains in the .NET 10 package graph, build Debug and Release, and run relevant and full tests.

**Done when**: No .NET 10 source or package graph resolves SixLabors.ImageSharp; only projects that directly need SkiaSharp reference the verified stable version; all identified image behavior is preserved with safe disposal; focused tests pass; audited restore and vulnerability checks are clean; Debug and Release solution builds succeed; full relevant tests pass; and the `9.0` tree remains unchanged.

---

### 02-razor-image-callers: Migrate Razor Page image upload callers

Refactor every .NET 10 Razor Page ImageSharp call site to use the shared path/byte helper APIs without direct SkiaSharp coupling. Preserve upload paths, output dimensions, source data, and existing error handling for profile avatars, pictures, videos, and slideshows; remove stale image-library usings and unnecessary Web package references.

**Done when**: Web source contains no SixLabors/ImageSharp usage, upload flows use shared helpers with unchanged dimensions and behavior, Web has no unnecessary image-library reference, and a targeted Web build succeeds without migration-caused warnings.

---

### 03-bulk-media-images: Migrate bulk media thumbnails and placeholders

Replace BulkMediaImport ImageSharp usage with SkiaSharp while preserving centered thumbnail crop/resize, JPEG quality 100, FFmpeg flow, failure propagation, and generated black 150x150 placeholders. Remove duplicated image code if source analysis confirms it is unused.

**Done when**: BulkMediaImport references SkiaSharp 4.153.1 and no ImageSharp, migrated operations preserve behavior and disposal, confirmed dead duplicate code is removed rather than ported, and a targeted BulkMediaImport build succeeds without migration-caused warnings.

---

### 04-image-tests-validation: Add image tests and validate the .NET 10 solution

Add focused NUnit coverage using generated temporary images for landscape and portrait resize, centered crop dimensions/content, JPEG output and decode round-trip, and relevant failure behavior. Clean the Tests package reference, verify live package metadata and the complete dependency graph, and execute all requested restore, audit, build, and test gates.

**Done when**: Focused tests pass; no .NET 10 source or package graph resolves ImageSharp; stable SkiaSharp version, MIT license, net10.0 compatibility, and advisory status are verified; audited restore and vulnerability checks are clean; Debug and Release solution builds succeed; full tests pass; and the `9.0` tree remains unchanged.
