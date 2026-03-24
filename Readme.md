# RepoKit 🎯

> **One attribute. Zero boilerplate. Full repository pattern.**

A lightweight, production-ready .NET library that eliminates repository boilerplate through smart dependency injection. Add `[AutoRepository]` to your entity and instantly get a fully wired `IRepository<T>` with support for **Entity Framework Core**, **Dapper**, pagination, specifications, and Unit of Work.

[![NuGet version](https://img.shields.io/nuget/v/RepoKit.Core)](https://www.nuget.org/packages/RepoKit.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-blue)](https://dotnet.microsoft.com)
[![Build Status](https://img.shields.io/github/actions/workflow/status/souravdas98/RepoKit/ci.yml?branch=main)](https://github.com/souravdas98/RepoKit/actions/workflows/ci.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=souravdas98_RepoKit&metric=alert_status)](https://sonarcloud.io/dashboard?id=souravdas98_RepoKit)

---

## ✨ Features

- **🎯 Zero Configuration** - Single `[AutoRepository]` attribute to register repositories
- **🔄 ORM Agnostic** - Entity Framework Core, Dapper, or both in the same project
- **📖 Specification Pattern** - Type-safe, reusable query building
- **📄 Built-in Pagination** - `PagedResult<T>` with metadata
- **🔗 Unit of Work** - Atomic multi-repository operations with transactions
- **⚡ High Performance** - Minimal overhead; Dapper for raw SQL workloads
- **📚 Full XML Docs** - 100% documented with complete IntelliSense support
- **🛡️ Null-Safe** - Nullable reference types; comprehensive validation
- **🎓 Clean Architecture Ready** - SOLID principles and DDD compatible
- **💾 Multi-Targeting** - .NET 8, 9, and 10

---

## 📦 Installation

### Option 1: Entity Framework Core Only

```bash
dotnet add package RepoKit.Core
dotnet add package RepoKit.EfCore
```

### Option 2: Dapper Only

```bash
dotnet add package RepoKit.Core
dotnet add package RepoKit.Dapper
```

### Option 3: Both EF Core and Dapper

```bash
dotnet add package RepoKit.Core
dotnet add package RepoKit.EfCore
dotnet add package RepoKit.Dapper
```

---

## 🚀 Quick Start

### Step 1: Define Entity

```csharp
using RepoKit.Core;

[AutoRepository]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
```

### Step 2: Configure Services

**Entity Framework Core Only:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositories(typeof(Program).Assembly);
```

**Dapper Only:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAutoRepositoryDapper(sp => 
        new SqlConnection("Server=localhost;Database=MyDb;Trusted_Connection=true;"))
    .AddAutoRepositories(typeof(Program).Assembly);
```

**Both EF Core and Dapper:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositoryDapper(sp => 
        new SqlConnection("Server=localhost;Database=MyDb;Trusted_Connection=true;"))
    .AddAutoRepositories(typeof(Program).Assembly);
```

### Step 3: Inject & Use

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _repo;

    public ProductsController(IRepository<Product> repo) => _repo = repo;

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product != null ? Ok(product) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Product product)
    {
        await _repo.AddAsync(product);
        return Created($"products/{product.Id}", product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        
        product.Id = id;
        await _repo.UpdateAsync(product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteByIdAsync(id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _repo.GetPagedAsync(
            new AllProductsSpec(), pageNumber, pageSize);
        return Ok(result);
    }
}
```

---

## 🔍 Advanced Usage

### Specification Pattern

```csharp
public class ActiveProductsSpec : BaseSpecification<Product>
{
    public ActiveProductsSpec(string? search = null)
    {
        AddCriteria(p => p.IsActive);
        
        if (search != null)
            AddCriteria(p => p.Name.Contains(search));
        
        ApplyOrderBy(p => p.Name);
        AddInclude(p => p.Category);
        AddInclude(p => p.Supplier);
        ApplyPaging(skip: 0, take: 10);
    }
}

var spec = new ActiveProductsSpec("laptop");
var products = await _repo.GetAsync(spec);
var paged = await _repo.GetPagedAsync(spec, pageNumber: 1, pageSize: 10);
```

### Dapper with Raw SQL

```csharp
public class TopSellingProductsSpec : BaseSpecification<Product>
{
    public TopSellingProductsSpec(int limit = 5)
    {
        UseRawSql(
            @"SELECT TOP @Limit p.* 
              FROM Products p
              INNER JOIN OrderItems oi ON p.Id = oi.ProductId
              GROUP BY p.Id, p.Name, p.Price
              ORDER BY COUNT(*) DESC",
            new { Limit = limit });
    }
}

var spec = new TopSellingProductsSpec(5);
var topProducts = await _repo.GetAsync(spec);
```

### Unit of Work Pattern

```csharp
public class ProductTransferService
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task MoveProductsAsync(int fromCategoryId, int toCategoryId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var spec = new ProductsByCategorySpec(fromCategoryId);
            var products = await _productRepo.GetAsync(spec);
            
            foreach (var product in products)
            {
                product.CategoryId = toCategoryId;
                await _productRepo.UpdateAsync(product);
            }
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### Read-Only Repositories

```csharp
[AutoRepository(ReadOnly = true)]
public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

private readonly IReadRepository<AuditLog> _auditRepo;
public async Task<IActionResult> History()
{
    var logs = await _auditRepo.GetAllAsync();
    return Ok(logs);
}
```

### Custom Implementations

```csharp
public class CustomProductRepository : DapperRepository<Product>
{
    public CustomProductRepository(IDbConnection connection) 
        : base(connection) { }
    
    protected override string GetTableName() => "tbl_Product";
    protected override string GetIdColumnName() => "ProductID";
}

[AutoRepository(CustomImplementation = typeof(CustomProductRepository))]
public class SpecialProduct { }
```

---

## 📊 Architecture

### Project Structure

```
RepoKit.Core (Required)
├── IRepository<T>          - Full CRUD operations
├── IReadRepository<T>      - Read-only operations
├── IUnitOfWork             - Transaction management
├── ISpecification<T>       - Query specification
├── PagedResult<T>          - Paged result container
├── BaseSpecification<T>    - Base class for specifications
└── [AutoRepository]        - Entity registration attribute

RepoKit.EfCore (Optional)
├── EfRepository<T>         - Entity Framework implementation
├── EfUnitOfWork            - DbContext-based transactions
├── SpecificationEvaluator  - Converts specs to LINQ
└── ServiceCollectionExtensions

RepoKit.Dapper (Optional)
├── DapperRepository<T>     - Dapper implementation
├── DapperUnitOfWork        - SQL transaction management
└── ServiceCollectionExtensions
```

---

## 🎯 Best Practices

✅ **DO:**
- Use `IReadRepository<T>` for queries only
- Create specifications for complex queries
- Use Unit of Work for multi-repository operations
- Always paginate large datasets
- Use `[AutoRepository(ReadOnly = true)]` for audit entities

❌ **DON'T:**
- Use `IRepository<T>` where `IReadRepository<T>` suffices
- Retrieve all rows without pagination
- Pass raw SQL without parameters (SQL injection risk)

---

## 📋 API Reference

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetByIdAsync<TKey>(key)` | `T?` | Get entity by primary key |
| `GetAllAsync()` | `IReadOnlyList<T>` | Get all entities |
| `GetAsync(spec)` | `IReadOnlyList<T>` | Get by specification |
| `GetPagedAsync(spec, page, size)` | `PagedResult<T>` | Get paginated results |
| `CountAsync(spec)` | `int` | Count matching entities |
| `AnyAsync(spec)` | `bool` | Check if any exists |
| `AddAsync(entity)` | `Task` | Add single entity |
| `UpdateAsync(entity)` | `Task` | Update entity |
| `DeleteAsync(entity)` | `Task` | Delete entity |

---

## ⚙️ Requirements

- **.NET 8.0+** - Supports 8.0, 9.0, 10.0
- **C# 12+** - Using nullable reference types
- **Entity Framework Core 8.0+** - For RepoKit.EfCore
- **Dapper 2.1+** - For RepoKit.Dapper

---

## 📝 License

MIT License - See [LICENSE](LICENSE) for details.

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📚 Documentation

- [Quick Start Guide](QUICK_START.md)
- [CI/CD Pipeline](CI_CD_PIPELINE.md)
- [Publishing Guide](PUBLISHING_GUIDE.md)
- [Branch Protection](BRANCH_PROTECTION.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Changelog](CHANGELOG.md)

---

## 💬 Support

- 📋 [GitHub Issues](https://github.com/SouravDas/RepoKit/issues)
- 💡 [GitHub Discussions](https://github.com/SouravDas/RepoKit/discussions)
- 📖 [Full Documentation](https://github.com/SouravDas/RepoKit/wiki)

---

**Made with ❤️ by [Sourav Das](https://github.com/SouravDas)**
