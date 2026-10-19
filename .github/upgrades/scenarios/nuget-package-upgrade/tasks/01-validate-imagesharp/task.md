# 01-validate-imagesharp: Verify ImageSharp usage, security, and compatibility

Inspect actual ImageSharp calls in Utilities, Razor Pages, BulkMediaImport, and tests, and trace direct/transitive dependencies across `10.0/digioz.Portal.sln`. Confirm the per-project references already specify 4.1.3 and independently verify the latest stable release and advisory ranges against live NuGet feeds. Preserve the current per-project package convention; no central package migration or unnecessary code edits. Assessment produced no API diff because existing and recommended versions are identical.

Validate a fresh audited restore, complete solution build, and existing relevant tests. Only apply minimal fixes required for ImageSharp compatibility; report unrelated validation blockers rather than expanding scope. Record the existing vulnerable 9.0 references as excluded from changes under the confirmed scope.

**Done when**: Usage and direct/transitive package resolution are documented, live feeds confirm 4.1.3 is latest stable compatible and has no known advisories, restore/build/tests results are recorded with any blockers, and no unnecessary package or application changes are made.

## Scope Inventory
- Validation covers eight net10.0 projects. Web, Dal, Utilities, BulkMediaImport, and Tests directly request and resolve SixLabors.ImageSharp 4.1.3; Bo, EmailProviders, and PaymentProviders do not resolve ImageSharp. No older transitive version appears in the solution graph.
- No application/project changes are planned. One concern: verify existing ImageSharp security and compatibility. No API migration or version divergence; no independent implementation units requiring decomposition.
- No Directory.Packages.props or package lock files were found. get_project_dependencies confirms the Utilities package version is defined in its project file, not CPM.

## Research Findings
- Assessment selects latest stable compatible 4.1.3 across all five referencing projects; old/current and proposed versions are identical, so no API diff is generated.
- Live NuGet flat-container index independently reports LATEST_STABLE=4.1.3. NuGet vulnerability feed includes affected ranges through 4.1.1 for GHSA-gwg2-r3hj-4w44, GHSA-j3p4-wp97-rph4, GHSA-j9gm-c75j-xc9q, GHSA-wmxv-xphr-5c9g, GHSA-ffp7-56pq-64mr, GHSA-4q3p-rj5x-xv7p, and GHSA-jjfr-hcj7-qf5w. 4.1.3 is outside all published affected ranges checked. Initial dotnet package audit reports no vulnerable direct/transitive packages.
- Utilities/Helpers/ImageHelper.cs loads files and exposes Image in public helper methods, uses Mutate/Crop/Resize, and writes JpegEncoder(Quality=100). Utilities/Helpers/Utility.cs implements SaveImageWithResize and SaveImageWithCrop for uploaded images.
- Razor Pages for pictures, videos, slideshows, products, and profile avatars call Image.Load and these helpers. BulkMediaImport/MediaImporter.cs processes image thumbnails, JPEG output, and Rgba32 placeholder images. VideoThumbnailHelper.cs uses Image.Load, centered Crop/Resize, and JpegEncoder after FFmpeg frame extraction.
- Dal has a direct package reference but no ImageSharp source usages were found by initial workspace inspection. Tests directly reference 4.1.3, use NUnit 4.2.2 and FluentAssertions 6.12.1, and reference application projects. No ImageSharp/ImageHelper/SaveJigsawImage/SaveImageWithCrop matches exist in test source.
- Existing NUnit suite will be run. An isolated temporary console smoke check can exercise helpers against synthetic images to cover image load/crop/resize/JPEG round trips without adding repository code or external files.
- Separate 9.0 Web, Dal, Utilities, and BulkMediaImport projects reference 3.1.12, within current advisory ranges; excluded under confirmed scope and must be reported.

## Validation Approach
- Use installed SDK 10.0.402 for explicit fresh restore with NuGetAudit=true and NuGetAuditMode=all; use the IDE solution build tool first per building-projects guidance.
- Execute existing NUnit tests and repeat direct/transitive vulnerability audit after restore. Capture logs/results outside the repository; document unrelated failures without broadening the requested minimal-change scope.
