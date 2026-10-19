# 01.01-shared-image-helpers: Migrate shared image helpers and clean package references

## Objective
Replace ImageSharp in Utilities with SkiaSharp 4.153.1 using path/byte-oriented helper APIs, preserve resize/crop/JPEG/failure behavior, and remove the unused Dal image package reference.

## Scope
- `10.0/digioz.Portal.Utilities/digioz.Portal.Utilities.csproj`
- `10.0/digioz.Portal.Utilities/Helpers/ImageHelper.cs`
- `10.0/digioz.Portal.Utilities/Helpers/Utility.cs`
- `10.0/digioz.Portal.Dal/digioz.Portal.Dal.csproj`

## Key Requirements
- Preserve aspect-preserving downscale, centered crop, exact output dimensions, JPEG quality 100, and existing bool/SizeF failure contracts.
- Add file and byte entry points that avoid exposing SkiaSharp in Razor Page handlers.
- Dispose every SkiaSharp native-backed object deterministically.
- Keep per-project package conventions and make minimal edits.

**Done when**: Utilities references SkiaSharp 4.153.1 and no ImageSharp; Dal has no image package reference; shared helper APIs preserve documented semantics; and targeted Utilities/Dal builds succeed without migration-caused warnings.

## Research Findings

- Dependency inspection confirms standard per-project PackageReference management: Utilities and Dal each define ImageSharp 4.1.3 directly in their own project files; no CPM or imported package-version file is involved.
- Dal has no image source usage and already references Utilities, so its direct ImageSharp reference is removable with no replacement.
- `ImageHelper` is the shared behavior boundary. Its ImageSharp-typed public overloads are used only by Razor Page callers that load from a path or byte array; replacing them with path and byte-array overloads avoids exposing SkiaSharp outside Utilities.
- `Utility.SaveImageWithResize` is the only `Utility` image entry point found in the .NET 10 source graph. Its ImageSharp-typed `SaveImageWithCrop` overload has no external callers and can become a path-based wrapper while preserving `SizeF.Empty` on failure.
- The implementation will decode with `SKBitmap`, calculate the same rounded dimensions and centered source rectangles, render using Skia sampling, and encode explicitly as JPEG quality 100. Decoded bitmaps, resized outputs, `SKImage`, `SKData`, canvases, and streams will be disposed deterministically.
- Utilities and Dal are SDK-style net10.0 projects with no special build features requiring full Visual Studio MSBuild; targeted `dotnet build` is appropriate, followed by full solution builds in the validation subtask.
