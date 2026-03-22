# Contributing to RepoKit

First, thank you for your interest in contributing to RepoKit! We appreciate your effort and value your contributions.

This document provides guidelines and instructions for contributing to the project.

---

## Code of Conduct

We are committed to providing a welcoming environment to all contributors. Please be respectful and constructive in all interactions.

---

## Getting Started

### Prerequisites

- .NET 8.0 SDK or higher
- Git
- A GitHub account

### Development Setup

1. **Fork the repository**
   ```bash
   # Use GitHub's web interface to fork
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/RepoKit.git
   cd RepoKit
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/SouravDas/RepoKit.git
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

---

## How to Contribute

### Bug Reports

Found a bug? Here's how to report it:

1. **Check existing issues** - search [GitHub Issues](https://github.com/SouravDas/RepoKit/issues)
2. **Create a new issue** with:
   - Clear, descriptive title
   - Detailed description
   - Steps to reproduce
   - Expected vs actual behavior
   - .NET version and environment info

**Example:**
```
Title: IRepository<T>.GetPagedAsync throws ArgumentException when pageSize is negative

Description:
When calling GetPagedAsync with a negative pageSize, an ArgumentException is thrown 
instead of being caught and handled gracefully.

Steps:
1. Create a product repository
2. Call GetPagedAsync(spec, 1, -10)
3. ArgumentException is thrown

Expected: Should throw ArgumentException with clear message
Actual: ArgumentException message is unclear

Environment: .NET 8.0, Windows 11
```

### Feature Requests

Want to suggest a new feature?

1. **Check discussions** - browse [GitHub Discussions](https://github.com/SouravDas/RepoKit/discussions)
2. **Describe the feature**:
   - Why do you need it?
   - How would it be used?
   - Any alternatives considered?
3. **Wait for feedback** before implementing

### Code Changes

#### Branch Naming

Use descriptive branch names:
- `feature/add-async-enumerable` - new feature
- `fix/pagination-off-by-one` - bug fix
- `docs/update-readme` - documentation
- `refactor/simplify-specification-pattern` - refactoring

#### Commit Guidelines

- Write clear, concise commit messages
- Use present tense: "Add feature" not "Added feature"
- Reference issues: "Fix #123" or "Closes #456"
- Keep commits focused and logical

**Example:**
```bash
git commit -m "Fix pagination off-by-one error in EfRepository

- Skip calculation was incorrect for page 1
- Add unit test to prevent regression
- Fixes #123"
```

#### Code Style

We use **C# 12** with these principles:

1. **Nullable Reference Types**: Always enabled
   ```csharp
   public class MyClass
   {
       public string? OptionalValue { get; set; }
       public string RequiredValue { get; set; } = string.Empty;
   }
   ```

2. **XML Documentation**: Document all public APIs
   ```csharp
   /// <summary>
   /// Retrieves an entity by its primary key.
   /// </summary>
   /// <param name="id">The primary key value.</param>
   /// <returns>The entity, or null if not found.</returns>
   public async Task<T?> GetByIdAsync<TKey>(TKey id);
   ```

3. **Naming Conventions**:
   - `PascalCase` for types and members
   - `camelCase` for parameters and local variables
   - `_camelCase` for private fields
   - `CONSTANT_CASE` for constants

4. **Using Statements**: Organize alphabetically
   ```csharp
   using System;
   using System.Collections.Generic;
   using Microsoft.EntityFrameworkCore;
   ```

5. **Formatting**: Use `dotnet format`
   ```bash
   dotnet format
   ```

#### Testing

1. **Write tests** for all new features
2. **Add tests** for bug fixes
3. **Follow AAA pattern**:
   ```csharp
   [Test]
   public async Task GetPagedAsync_WithValidParams_ReturnsPaginatedResults()
   {
       // Arrange
       var spec = new TestSpec();
       
       // Act
       var result = await _repo.GetPagedAsync(spec, 1, 10);
       
       // Assert
       Assert.That(result.Items, Has.Count.Equal(10));
       Assert.That(result.PageNumber, Is.EqualTo(1));
   }
   ```

#### Documentation

1. **Update XML comments** for API changes
2. **Update README.md** if behavior changes
3. **Add examples** for new features
4. **Update CHANGELOG.md**

### Pull Request Process

1. **Update your fork**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create Pull Request**
   - Use template (if available)
   - Link related issues: "Fixes #123"
   - Describe changes clearly
   - Include before/after examples for UI changes

4. **PR Title Format**
   ```
   [type] Brief description
   
   Examples:
   - [feature] Add AsyncEnumerable support
   - [fix] Correct pagination calculation
   - [docs] Update API documentation
   - [refactor] Simplify repository factory
   ```

5. **PR Description Template**
   ```markdown
   ## Description
   Brief description of changes
   
   ## Related Issues
   Fixes #123
   
   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Breaking change
   - [ ] Documentation
   
   ## Checklist
   - [ ] Code builds successfully
   - [ ] Tests pass
   - [ ] New tests added
   - [ ] XML docs updated
   - [ ] README updated if needed
   - [ ] CHANGELOG updated
   
   ## Screenshots (if applicable)
   ```

### What We Look For

✅ **Good PRs have:**
- Clear, focused changes
- Comprehensive tests
- Updated documentation
- Follows code style
- Resolves the issue completely

❌ **PRs may be rejected if they:**
- Don't follow code style
- Lack tests
- Have breaking changes without discussion
- Don't resolve the stated issue
- Include unrelated changes

---

## Development Tasks

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/RepoKit.Core.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Building Documentation
```bash
# Generate XML docs
dotnet build -c Release

# Docs are in bin/Release/net*/RepoKit.*.xml
```

### Creating NuGet Package
```bash
# Pack all projects
dotnet pack -c Release

# Output: bin/Release/*.nupkg
```

---

## Areas for Contribution

- 🐛 **Bug fixes** - Fix reported issues
- ✨ **Features** - Implement requested features
- 📚 **Documentation** - Improve README, guides, examples
- 🧪 **Tests** - Improve test coverage
- 🎨 **Sample apps** - Create tutorials and examples
- 🚀 **Performance** - Optimize hot paths
- ♿ **Accessibility** - Make docs more accessible

---

## Getting Help

- 📖 [Documentation](https://github.com/SouravDas/RepoKit/wiki)
- 💬 [Discussions](https://github.com/SouravDas/RepoKit/discussions)
- 🐛 [Issues](https://github.com/SouravDas/RepoKit/issues)
- 📧 Contact maintainers in discussions

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to RepoKit! 🙏
