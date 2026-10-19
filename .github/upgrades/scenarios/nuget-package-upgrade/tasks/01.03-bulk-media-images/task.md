# 01.03-bulk-media-images: Migrate bulk media thumbnails and placeholders

## Objective
Replace BulkMediaImport ImageSharp usage with SkiaSharp while preserving centered thumbnail crop/resize, JPEG quality 100, FFmpeg flow, rethrow behavior, and generated 150x150 black placeholders. Remove dead duplicate image code if confirmed unused.

## Scope
- `10.0/digioz.Portal.BulkMediaImport/digioz.Portal.BulkMediaImport.csproj`
- `MediaImporter.cs`
- `VideoThumbnailHelper.cs`

**Done when**: BulkMediaImport references SkiaSharp 4.153.1 and no ImageSharp, migrated operations preserve behavior and disposal, confirmed dead duplicate code is removed rather than ported, and a targeted BulkMediaImport build succeeds without migration-caused warnings.
