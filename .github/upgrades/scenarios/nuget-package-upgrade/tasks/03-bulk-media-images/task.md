# 03-bulk-media-images: Migrate bulk media thumbnails and placeholders

Replace BulkMediaImport ImageSharp usage with SkiaSharp while preserving centered thumbnail crop/resize, JPEG quality 100, FFmpeg flow, failure propagation, and generated black 150x150 placeholders. Remove duplicated image code if source analysis confirms it is unused.

**Done when**: BulkMediaImport references SkiaSharp 4.153.1 and no ImageSharp, migrated operations preserve behavior and disposal, confirmed dead duplicate code is removed rather than ported, and a targeted BulkMediaImport build succeeds without migration-caused warnings.

## Research Findings

- BulkMediaImport defines ImageSharp 4.1.3 directly in its SDK-style net10.0 project; it needs a direct SkiaSharp reference only for black placeholder generation.
- `MediaImporter.SaveImageWithCrop` is not dead: symbol analysis found its picture-import call at line 90. Its implementation duplicates the shared centered-crop behavior, so both the method and its caller can be replaced by `ImageHelper.SaveImageWithCrop(path, ...)` with an explicit exception when the helper reports failure.
- `VideoThumbnailHelper.ResizeAndCropThumbnail` duplicates the same centered-crop algorithm and currently logs then rethrows. It can call the shared path helper and throw on `false`, preserving the existing log/rethrow contract without direct SkiaSharp use in that file.
- `CreatePlaceholderThumbnail` must remain local and create a 150x150 black JPEG at quality 100. It will use a black-filled `SKBitmap`, `SKImage`, and encoded `SKData`, all disposed deterministically.
- FFmpeg extraction, temporary thumbnail deletion, and the surrounding placeholder fallback flow do not require changes.
