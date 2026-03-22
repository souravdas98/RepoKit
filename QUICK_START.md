# RepoKit Quick Reference Guide

A fast, practical reference for common RepoKit patterns and usage.

## Installation

```bash
# Minimal setup (EF Core)
dotnet add package RepoKit.Core
dotnet add package RepoKit.EfCore

# For Dapper
dotnet add package RepoKit.Dapper

# For both
dotnet add package RepoKit.Core
dotnet add package RepoKit.EfCore
dotnet add package RepoKit.Dapper
```

## Basic Setup (Program.cs)

### Entity Framework Core

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositories(typeof(Program).Assembly);

var app = builder.Build();
```

### Dapper

```csharp
builder.Services
    .AddAutoRepositoryDapper(sp => 
        new SqlConnection("connection-string"))
    .AddAutoRepositories(typeof(Program).Assembly);
```

## Entity Definition

```csharp
using RepoKit.Core;

[AutoRepository]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

## Usage Patterns

### Dependency Injection

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repo;
    
    public ProductService(IRepository<Product> repo)
    {
        _repo = repo;
    }
}
```

### CRUD Operations

```csharp
// Create
await _repo.AddAsync(new Product { Name = "Laptop", Price = 999 });

// Read
var product = await _repo.GetByIdAsync(1);
var all = await _repo.GetAllAsync();

// Update
product.Price = 899;
await _repo.UpdateAsync(product);

// Delete
await _repo.DeleteAsync(product);
// or
await _repo.DeleteByIdAsync(1);

// Batch operations
await _repo.AddRangeAsync(new[] { product1, product2 });
await _repo.DeleteRangeAsync(products);
```

### Pagination

```csharp
var result = await _repo.GetPagedAsync(
    spec: new AllProductsSpec(),
    pageNumber: 1,
    pageSize: 10);

// PagedResult<T> properties:
// - Items: IReadOnlyList<Product>
// - PageNumber: int
// - PageSize: int
// - TotalCount: int
// - TotalPages: int
// - HasPreviousPage: bool
// - HasNextPage: bool
```

### Specifications

```csharp
public class ActiveProductsSpec : BaseSpecification<Product>
{
    public ActiveProductsSpec(string? search = null)
    {
        // Filter
        AddCriteria(p => p.IsActive);
        
        if (search != null)
            AddCriteria(p => p.Name.Contains(search));
        
        // Order
        ApplyOrderBy(p => p.Name);
        
        // Includes (EF Core only)
        AddInclude(p => p.Category);
        
        // Pagination
        ApplyPaging(0, 20);
    }
}

// Usage
var spec = new ActiveProductsSpec("laptop");
var products = await _repo.GetAsync(spec);
var count = await _repo.CountAsync(spec);
var exists = await _repo.AnyAsync(spec);
var paged = await _repo.GetPagedAsync(spec, 1, 10);
var first = await _repo.GetFirstOrDefaultAsync(spec);
```

### Read-Only Repositories

```csharp
[AutoRepository(ReadOnly = true)]
public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
}

// Usage - no Add/Update/Delete available
private readonly IReadRepository<AuditLog> _auditRepo;
var logs = await _auditRepo.GetAllAsync();
```

### Unit of Work Pattern

```csharp
public async Task TransferFundsAsync(int accountId, decimal amount)
{
    await _unitOfWork.BeginTransactionAsync();
    try
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        account.Balance -= amount;
        await _accountRepo.UpdateAsync(account);
        
        await _transactionRepo.AddAsync(
            new Transaction { AccountId = accountId, Amount = amount });
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Dapper Raw SQL

```csharp
public class TopProductsSpec : BaseSpecification<Product>
{
    public TopProductsSpec(int limit = 10)
    {
        UseRawSql(
            @"SELECT TOP @Limit p.* FROM Products p
              WHERE p.IsActive = 1
              ORDER BY p.Sales DESC",
            new { Limit = limit });
    }
}
```

### Custom Repository

```csharp
public class CustomProductRepository : DapperRepository<Product>
{
    public CustomProductRepository(IDbConnection conn) : base(conn) { }
    
    protected override string GetTableName() => "t_Products";
    protected override string GetIdColumnName() => "ProductID";
}

[AutoRepository(CustomImplementation = typeof(CustomProductRepository))]
public class Product { }
```

### Lifetime Control

```csharp
[AutoRepository(Lifetime = ServiceLifetime.Singleton)]
public class Config { }

[AutoRepository(Lifetime = ServiceLifetime.Transient)]
public class TempData { }

// Default: ServiceLifetime.Scoped
[AutoRepository]
public class Product { }
```

## Common Queries

### Basic Filter

```csharp
public class ProductsByPriceSpec : BaseSpecification<Product>
{
    public ProductsByPriceSpec(decimal minPrice, decimal maxPrice)
    {
        AddCriteria(p => p.Price >= minPrice && p.Price <= maxPrice);
        ApplyOrderBy(p => p.Name);
    }
}
```

### With Includes

```csharp
public class ProductsWithDetailsSpec : BaseSpecification<Product>
{
    public ProductsWithDetailsSpec()
    {
        AddInclude(p => p.Category);
        AddInclude(p => p.Supplier);
        AddInclude("Reviews");  // String path for nested includes
    }
}
```

### Multiple Ordering

```csharp
public class ProductsOrderedSpec : BaseSpecification<Product>
{
    public ProductsOrderedSpec()
    {
        ApplyOrderBy(p => p.CategoryId);
        AddThenBy(p => p.Price);
        AddThenBy(p => p.Name, descending: true);
    }
}
```

## Testing

```csharp
[TestFixture]
public class ProductRepositoryTests
{
    private IRepository<Product> _repo;
    private AppDbContext _context;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("test")
            .Options;
        _context = new AppDbContext(options);
        _repo = new EfRepository<Product>(_context);
    }

    [Test]
    public async Task AddAsync_WithValidProduct_AddsToDb()
    {
        // Arrange
        var product = new Product { Name = "Test", Price = 100 };

        // Act
        await _repo.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repo.GetByIdAsync(product.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test"));
    }
}
```

## Performance Tips

1. **Use `IReadRepository<T>` for queries**
   ```csharp
   private readonly IReadRepository<Product> _queryRepo;  // ✅
   private readonly IRepository<Product> _repo;            // ❌ if only reading
   ```

2. **Use paginated results for large datasets**
   ```csharp
   var paged = await _repo.GetPagedAsync(spec, page, 50);  // ✅
   var all = await _repo.GetAllAsync();                     // ❌ could be slow
   ```

3. **Use specifications to avoid N+1 queries**
   ```csharp
   var spec = new ProductsWithDetailsSpec();  // Includes loaded
   await _repo.GetAsync(spec);                 // ✅ Single query
   ```

4. **Use Dapper for complex queries**
   ```csharp
   // EF Core for standard CRUD
   // Dapper for complex reporting/analytics
   ```

## Troubleshooting

### "No repository factory registered"

```csharp
// ❌ Missing ORM registration
builder.Services.AddAutoRepositories(typeof(Program).Assembly);

// ✅ Correct - Add ORM first
builder.Services
    .AddDbContext<AppDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()  // Add this
    .AddAutoRepositories(typeof(Program).Assembly);
```

### Specification not working with Dapper

```csharp
// ❌ Dapper doesn't support LINQ
public class MySpec : BaseSpecification<Product>
{
    public MySpec()
    {
        AddCriteria(p => p.IsActive);  // ❌ Won't work
    }
}

// ✅ Use RawSql for Dapper
public class MySpec : BaseSpecification<Product>
{
    public MySpec()
    {
        UseRawSql("SELECT * FROM Products WHERE IsActive = 1");  // ✅
    }
}
```

### Transaction not committing

```csharp
// ❌ Forgot to commit
await _unitOfWork.BeginTransactionAsync();
await _repo.AddAsync(entity);
// ❌ Never called CommitTransactionAsync

// ✅ Proper transaction
await _unitOfWork.BeginTransactionAsync();
try
{
    await _repo.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();  // ✅
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
}
```

## API Cheat Sheet

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetByIdAsync<TKey>` | `T?` | Get by primary key |
| `GetAllAsync` | `IReadOnlyList<T>` | Get all entities |
| `GetAsync` | `IReadOnlyList<T>` | Get by specification |
| `GetFirstOrDefaultAsync` | `T?` | Get first or null |
| `GetPagedAsync` | `PagedResult<T>` | Get page by specification |
| `CountAsync` | `int` | Count by specification |
| `AnyAsync` | `bool` | Check if exists |
| `AddAsync` | `Task` | Add one entity |
| `AddRangeAsync` | `Task` | Add multiple entities |
| `UpdateAsync` | `Task` | Update entity |
| `DeleteAsync` | `Task` | Delete entity |
| `DeleteByIdAsync<TKey>` | `Task` | Delete by primary key |
| `DeleteRangeAsync` | `Task` | Delete multiple entities |

---

For more details, see [README.md](README.md) or [full documentation](https://github.com/SouravDas/RepoKit/wiki).
