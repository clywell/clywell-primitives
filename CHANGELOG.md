# Changelog

All notable changes to Clywell.Primitives will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.1.3] - 2026-03-14

### Changed

- `PagingParameters` — restored DataAnnotations validation attributes. Carries `[Range(1, int.MaxValue)]` on `Page` and `[Range(1, MaxPageSize)]` on `PageSize`, enabling direct use with `[AsParameters]` in minimal API endpoints and ASP.NET Core's built-in `AddValidation()`. No separate request model needed.

---

## [1.1.2] - 2026-03-14

### Changed

- `PagingParameters` — reverted DataAnnotations support; validation attributes are an API/HTTP concern and belong in the API binding layer, not in the domain model. Use `PagingRequest` from `Clywell.ServiceDefaults` for endpoint parameter binding with built-in validation.

---

## [1.1.1] - 2026-03-14

### Changed

- `PagingParameters` — added `System.ComponentModel.DataAnnotations` validation support:
	- `[Range(1, int.MaxValue)]` on `Page` property
	- `[Range(1, MaxPageSize)]` on `PageSize` property
	- Implements `IValidatableObject` for use with ASP.NET Core minimal API `.WithParameterValidation()`
	- Constructor overloads added to support parameterless and `(int page, int pageSize)` construction

---

## [1.1.0] - 2026-02-28

### Added

- `PagedResult<T>` — immutable paged result carrying `Items`, `TotalCount`, `Page`, `PageSize`, derived `TotalPages`, `HasPreviousPage`, `HasNextPage`, and a static `Empty()` factory
- `PagingParameters` — value object encapsulating `Page` (1-based), `PageSize` (capped at 100), and a derived `Skip` helper; `Default` singleton for convenience
- `SortingParameters` — value object encapsulating `SortBy` (field name) and `Direction` (`SortDirection` enum: `Ascending` / `Descending`); `IsDescending` convenience property
- `SortDirection` — `Ascending` / `Descending` enum used by `SortingParameters`

---

## [1.0.0] - 2026-02-25

### Added

- Initial release of Clywell.Primitives (zero external dependencies) targeting .NET 10.0+ / C# 14
- Core result types: `Result` (void-like) and `Result<T>` (value-returning), with equality and `ToString()`
- Core operations: `Match` / `Switch`, `Map` / `Bind`, `Tap`, `OnSuccess` / `OnFailure`, `ValueOr`, `Deconstruct`
- Bridging between result shapes: `Result.Map` / `Result.Bind` (to produce `Result<T>`), `Result<T>.ToResult()`
- Async instance operations: `TapAsync`, `TapErrorAsync`, `MapAsync`, `BindAsync`, `MatchAsync`
- Typed error model: `ErrorCode` (8 built-in codes + implicit `string` conversion) and `Error` with metadata + inner-error chaining
- Validation primitives: `ValidationFailure` and `ValidationError` for field-level validation failures
- Factory helpers: `Result.Success`, `Result.Failure`, `Result.Try` / `TryAsync`, `Result.FromNullable` (reference + nullable value types)
- Tuple composition: `Result.Combine` overloads for 2–5 results
- Railway-oriented extension members (sync + async) for `Result`, `Result<T>`, `Task<Result>`, and `Task<Result<T>>`: `Ensure`, `MapError`, `TapError`, plus `Collect` for aggregating `IEnumerable<Result<T>>`
- Implicit conversions from values/errors to `Result<T>`, and from `Error` to `Result`
- Public API XML documentation + Source Link enabled for step-into debugging from the NuGet package
- GitHub Actions workflows for CI/CD, release, and security scanning; README; MIT License
- 211 unit tests covering all types, operations, async pipelines, equality, and edge cases

---

[Unreleased]: https://github.com/clywell/clywell-primitives/compare/v1.1.3...HEAD
[1.1.3]: https://github.com/clywell/clywell-primitives/compare/v1.1.2...v1.1.3
[1.1.2]: https://github.com/clywell/clywell-primitives/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/clywell/clywell-primitives/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/clywell/clywell-primitives/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/clywell/clywell-primitives/releases/tag/v1.0.0
