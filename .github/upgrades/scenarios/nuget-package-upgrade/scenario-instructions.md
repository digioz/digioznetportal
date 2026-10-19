# ImageSharp to SkiaSharp Migration

## Preferences
- **Flow Mode**: Automatic
- **Scope**: Migrate and validate `10.0/digioz.Portal.sln`; leave `9.0` unchanged unless the user explicitly expands scope.
- **Package**: Replace SixLabors.ImageSharp 4.1.3 with the latest stable SkiaSharp compatible with .NET 10, currently verified as 4.153.1.
- **Version Policy**: Stable releases only; re-verify current NuGet version, MIT license, compatibility, and advisories before final validation.
- **Changes**: Minimal required edits; preserve existing .NET 10, Razor Pages, and per-project package conventions.
- **Validation**: Fresh audited restore with `NuGetAudit=true` and `NuGetAuditMode=all`, direct/transitive graph and vulnerability checks, Debug and Release solution builds, and relevant/full tests.

## Source Control
- **Source Branch**: dev-pete
- **Working Branch**: nuget-package-upgrade-imagesharp
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)
- **Pending Changes**: Saved with user approval in commit 6b81440c before branching. Five .NET 10 ImageSharp references already specify 4.1.3 at review baseline.

## Strategy
Replace ImageSharp with SkiaSharp in dependency order, centralize shared image operations where consistent with the existing architecture, migrate remaining callers, add focused generated-image tests, then validate the complete .NET 10 solution.

## Decisions
- Assessment selects SixLabors.ImageSharp 4.1.3 uniformly across five direct referencing projects; no version divergence or API transition.
- Preserve per-project PackageReference version declarations; no central package-management conversion.
- The excluded 9.0 tree references 3.1.12, within published affected advisory ranges; report this remaining risk without editing that tree.
- Git pre-upgrade save commit succeeded; automatic maintenance encountered locked pack files, so disable automatic maintenance for subsequent task commits.
- Replace ImageSharp because its Release build license target blocks the .NET 10 solution; do not add or bypass a Six Labors license.
- Remove unnecessary direct image-library references, including Dal if its no-usage finding is confirmed, rather than replacing every reference blindly.
- Preserve file and byte loading, aspect-preserving resize, centered crop, JPEG quality 100, black generated placeholders, upload flows, and encoded-format behavior while explicitly managing SkiaSharp ownership and disposal.
- Prefer shared helper APIs that keep SkiaSharp out of Razor Page handlers where this remains a minimal, convention-aligned change.
- Add focused NUnit coverage using generated temporary images with cleanup; do not add checked-in binary fixtures.
- Use `apply_patch` for existing files and preserve the untracked workflow artifacts.
