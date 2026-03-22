# Changelog

All notable changes to RepoKit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Multi-targeting support for .NET 8, 9, and 10
- Complete XML documentation for all public APIs
- Entity Framework Core and Dapper implementations

### Fixed
- Initial release

### Changed
- Initial release

### Removed
- Initial release

### Security
- Null validation on all public methods

---

## [1.0.0] - 2024-03-22

### Added
- ✅ `IRepository<T>` interface with full CRUD operations
- ✅ `IReadRepository<T>` interface for read-only access
- ✅ `[AutoRepository]` attribute for automatic registration
- ✅ `ISpecification<T>` and `BaseSpecification<T>` for query building
- ✅ `PagedResult<T>` for pagination support
- ✅ `IUnitOfWork` for transaction management
- ✅ Entity Framework Core implementation (`RepoKit.EfCore`)
- ✅ Dapper implementation (`RepoKit.Dapper`)
- ✅ Automatic dependency injection registration
- ✅ Multi-targeting (.NET 8, 9, 10)
- ✅ Complete XML documentation
- ✅ Support for custom repository implementations
- ✅ Transaction support for both EF Core and Dapper
- ✅ Specification pattern with LINQ and raw SQL support

### Features
- Zero configuration setup with `[AutoRepository]` attribute
- ORM-agnostic design supporting EF Core and Dapper interchangeably
- Type-safe query specifications
- Built-in pagination with metadata
- Atomic multi-repository operations via Unit of Work pattern
- Comprehensive null safety with nullable reference types
- Full IntelliSense support through XML documentation

---

## Versioning

- **Major**: Breaking changes in API or behavior
- **Minor**: New features that are backward compatible
- **Patch**: Bug fixes and minor improvements

## Future Roadmap

- [ ] Source generators for compile-time repository generation
- [ ] AsyncEnumerable support for streaming results
- [ ] GraphQL integration
- [ ] MongoDB/NoSQL implementations
- [ ] Performance benchmarks and optimizations
- [ ] Sample applications
- [ ] Tutorial videos

---

For detailed release notes, visit: https://github.com/SouravDas/RepoKit/releases
