# 01.02-razor-image-callers: Migrate Razor Page image upload callers

## Objective
Refactor every .NET 10 Razor Page ImageSharp call site to use the shared path/byte helper APIs without direct SkiaSharp coupling, preserving upload paths, dimensions, error handling, and behavior.

## Scope
Razor PageModels under `10.0/digioz.Portal.Web` for profile avatars, pictures, videos, slideshows, and any stale ImageSharp usings in product pages. Update the Web package reference based on actual direct need.

**Done when**: Web source contains no SixLabors/ImageSharp usage, upload flows call shared helpers with the same dimensions and source data, Web has no unnecessary image-library reference, and a targeted Web build succeeds without migration-caused warnings.
