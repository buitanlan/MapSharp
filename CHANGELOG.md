# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Fixed
- Multiple `[MapTo]` attributes now generate distinct method names (e.g. `MapToPersonDto()`)
- Generated file names use full type metadata to avoid namespace collisions
- Generated partial classes respect the source type's accessibility
- Numeric conversions use Roslyn's conversion classification instead of a loose heuristic
- Nested `MapTo` dispatch resolves the correct method when multiple targets exist

### Added
- Compile-time diagnostics (`MAP001`–`MAP003`) for non-partial classes, duplicate MapTo targets, and incompatible property mappings
- Multi-target mapping tests

### Changed
- Source generator uses `ForAttributeWithMetadataName` for faster discovery
- NuGet publish workflow ships only the `MapSharp` package (analyzer embedded)
- Prerelease workflow is manual (`workflow_dispatch`) instead of auto-publishing on every push
- Tests run on .NET 8, 9, and 10

## [1.0.0] - 2025-01-01

### Added
- Initial release with `[MapFrom]`, `[MapTo]`, and `[MapProperty]` attributes
- Nested object and collection mapping
- Bulk `MapFrom` / `MapTo` collection helpers
