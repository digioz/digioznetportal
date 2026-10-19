# 03-bulk-media-images: Migrate bulk media thumbnails and placeholders

Replace BulkMediaImport ImageSharp usage with SkiaSharp while preserving centered thumbnail crop/resize, JPEG quality 100, FFmpeg flow, failure propagation, and generated black 150x150 placeholders. Remove duplicated image code if source analysis confirms it is unused.

**Done when**: BulkMediaImport references SkiaSharp 4.153.1 and no ImageSharp, migrated operations preserve behavior and disposal, confirmed dead duplicate code is removed rather than ported, and a targeted BulkMediaImport build succeeds without migration-caused warnings.
