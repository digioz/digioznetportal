# 02-razor-image-callers: Migrate Razor Page image upload callers

Refactor every .NET 10 Razor Page ImageSharp call site to use the shared path/byte helper APIs without direct SkiaSharp coupling. Preserve upload paths, output dimensions, source data, and existing error handling for profile avatars, pictures, videos, and slideshows; remove stale image-library usings and unnecessary Web package references.

**Done when**: Web source contains no SixLabors/ImageSharp usage, upload flows use shared helpers with unchanged dimensions and behavior, Web has no unnecessary image-library reference, and a targeted Web build succeeds without migration-caused warnings.

## Research Findings

- Web directly references ImageSharp 4.1.3 even though all image processing can now be reached through Utilities; the direct package should be removed rather than replaced with SkiaSharp.
- Thirteen PageModel files load an image only to pass it immediately to `ImageHelper.SaveImageWithCrop`, using either 150x150 picture/video thumbnails, 850x450 slides, or 120x120 slide thumbnails.
- Profile avatar upload loads the same in-memory byte array twice for an 800x800 aspect-preserving full image and a 100x100 centered crop; the new byte-array overloads preserve this without native image ownership in the PageModel.
- SlideShow processing intentionally overwrites the uploaded full file with an 850x450 JPEG before reading that result to create the 120x120 thumbnail. Calls must remain ordered against the same file path.
- Product Add/Edit contain stale ImageSharp namespace imports but no image API calls.
- The targeted build disproved the initial Product finding: both Product handlers decode uploads, save the full image in its original encoded format, and create a 200x200 aspect-preserving thumbnail in that same format. A shared byte-array resize API must therefore retain the source codec format instead of forcing JPEG.
- Existing handlers generally ignore helper return values, so the migration will preserve that convention and surrounding catch/error behavior rather than introducing unrelated flow changes.
