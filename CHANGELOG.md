# Changelog

All notable changes to Clywell.Primitives will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- N/A

### Changed

- N/A

### Deprecated

- N/A

### Removed

- N/A

### Fixed

- N/A

---

## [1.0.0-alpha.1] - 2026-02-0=

### Added

- Initial alpha release of Clywell.Primitives (zero external dependencies) targeting .NET 10.0+ / C# 14
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
- GitHub Actions workflows for CI/CD, release, and security scanning; README + quick reference; MIT License

---

[Unreleased]: https://github.com/clywell/clywell-primitives/compare/v1.0.0-alpha.1...HEAD
[1.0.0-alpha.1]: https://github.com/clywell/clywell-primitives/releases/tag/v1.0.0-alpha.1
