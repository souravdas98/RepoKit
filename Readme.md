# RepoKit 🎯

> **One attribute. Zero boilerplate. Full repository pattern.**

A lightweight, production-ready .NET library that eliminates repository boilerplate through smart dependency injection. Add `[AutoRepository]` to your entity and instantly get a fully wired `IRepository<T>` with support for **Entity Framework Core**, **Dapper**, pagination, specifications, and Unit of Work.

[![NuGet version](https://img.shields.io/nuget/v/RepoKit.Core)](https://www.nuget.org/packages/RepoKit.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-blue)](https://dotnet.microsoft.com)

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
dotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdotnetdoine Entity

```csharp
using RepoKit.Core;

[AutoRepository]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool I    public bool I    public bool I    public bool I    public bool I    puework Core Only:**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositories(typeof(Program).Assembly);

var app = builder.Build();
```

**Dapper Only:**

```csharp
// Program.cs
var builder = WebApplication.CreateBuivar builder = WebApplication.C  var builder = WebApplication.Cr> 
        new SqlConnection("Serve     alhost;Database=MyDb;Trusted_Connection=true;"))
    .AddAutoReposito  es(typeof(Program).Assembly);

var app = builder.Bvar app = builder.Bvar app = bd Dapper:**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDb    .At>()    .AddDbContext<ito    .AddDbConteontext>()
    .AddAutoRepositoryDapper(sp => 
        new SqlConnection("Server=localhost;Database=MyDb;Trusted_Connection=true;"))
    .AddAutoRepositories(typeof(Program).Assembly);

var app = builder.Build();
```

### Step 3: Inject & Use

```csharp
[ApiCon[ApiCon]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
                                    du                       ProductsController(IRepository<Product> repo) => _repo = repo;

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
    {
blic async Task<ActionResult<Product>> Get(int id)
    ProductsCouct != null ? Ok(product) : NotFo    ProductsCouct != null ? Ok(product) : NotFo    ProductsCouct != null ? Ok(producuct)
    {
        await _repo.AddAsync(product);
        return Created($"products/{product.Id}", product);
    }

    [HttpPu    [HttpPu    public async Task<IActionResult> Update(int id, Pr    [HttpPu          [HttpPu    [HttpPu    public async Task<IActionResult> Update(int id, Prg =    [HttpPu    [HttpPu    public a 
        product.Id = id;
        await _repo.UpdateAsync(product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public a    public a    public > Delete(    public a    publicawait _repo.DeleteByIdAsync(id);
        return NoContent(        return NoContent(        return NoCon<Actio        return NoContent(        return NoContent(        return NoCon<Actio        return NoContent(     eSize = 10)
    {
                     aw                     aw                     aw        ec(    ageNumber, pageSize);
        return Ok(result);
    }
}
```

---

## 🔍 Advanced Usage

### Specification Pattern

Create reusable query specifications:

```csharp
public class ActiveProductsSpubl: BaseSpecification<Product>
{
    public    public    public    public    public    public    pub//    ter
        AddCriteria(p => p.IsActive);
        
        if (search != null)
            AddCriteria(p => p.Name.Contains(search));
        
        // Ordering
        ApplyOrderBy(p => p.Name);
        
        // EF Core: Eager loading
        AddInclude(p => p.Category);
        AddInclude(p => p.Su    er)        AddInclu   // Pagination (optional)
        ApplyPaging(skip: 0, take: 10);
    }
}

// Usage
var spec = new ActiveProductsSpec("laptop");
var products = await _repo.GetAsync(spec);
var paged = await _repo.GetPagedAsync(spec, pageNumber: 1, pageSize: 10);
```

### Dapper with Raw SQL

For complex queries, use raw SFor complex queries, use raw SFor complex queries, use raw SFor complex queries, use raw SFor complex qulic TopSellingProductsSpec(int limit Fo5)
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

// Usage
var spec = new TopSellingProductsSpec(5);
var topProducts = await _repo.GetAsync(spec);
```

### Unit of Work Pattern

Coordinate multi-repository operations with transactions:

```csharp
public class ProductTransferService
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProductTransferService(
        IRepository<Product> p        IRepository<Product> p        IRepository<Product> p        IRepork unitOfWork)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task MoveProductsAsync(int fromCategoryId, int toCategoryId)
    {
        awai        awai        awai        ync()        awai        awai              awai        awai        awai        ync()        a);
            var products = await _productRepo.GetAsync(spec);
            
            foreach (var product in products)
            {
                                        ategoryId;
                await _productRepo.UpdateAsync                awa    }
                                                         nc();
                                       nsactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
                                                        Re                                  f                                                        Re      ]
public class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime T    public DateTime T    pub In controller - only read operations available
private readonly IReadRepository<AuditLog> _auditprivate readonly IReadRepository<AuditLog> _auditprivate readonly IReadRepository<AuditLog> _auditprivate readonly IReadRepository<Audiom Imprivate readonly IReadRepository<AuditLog> _auditprivate`csharprivate  clasprivate readonly IReadRepository<AuditLog>ry<Pprivate {
      blic CustomProductRepository(IDbConnection connection) 
        : base(connection) { }
    
    protected override strin    protected override strin    protected override strin    ping GetIdColumnName() => "ProductID";
    
    public override async    public onl  ist<Product>> GetAllAsync(
        CancellationToken cancellationT        CancellationToken cancellationT        CancellationToken cancellationT        CancellationToken cancellationT        CancellationToken cancellationT ation = typeof(CustomProductRepository))]
public class Spepublic class Spepublic class Speory Lifetime Control

Control dependency injection scope:

```csharp
// Scoped (default) - one per request
[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[AutoReposito[e

### Project Structure

```
RepoKit.Core (Required)
├── IRepository<T>    ├── IRepository<T>   s
├── IReadRepository<T>      - Read-only operations
├── IUnitOfWork             - Transaction management
├── ISpecification<T>       - Query specification
├── PagedResult<T>          - Paged result container
├── BaseSpecification<T>    - Base class for specifications
└── [AutoRepository]        - Entity registration attribute

RepoKit.EfCore (Optional)
├── EfRepository<T>         - Entity Framework implementation
├── EfUnitOfWork            - DbContext-based trans├── EfUnitOfWork       onEvaluator  - Converts specs to LINQ
└── ServiceCollectionExtensions

RepoKit.Dapper (Optional)
├── DapperRepository<T>     - Dapper implementation
├── DapperUnitOfWork        - SQL transaction management
└── ServiceCollectionExtensions
```

### Design Patterns

- **Repository Pattern** - Encapsulates data access logic behind a clean interface
- **Specification Pattern** - Encapsulates query logic into reusable objects
- **Unit of Work Pattern** - Manages transactions across multiple repositories
- **Strategy Pattern** - Pluggable ORM implementations
- **Factory Pattern** - ORM-specific repository creation

---

## 💡 Usage Examples

### Search with Pagination

```csharp
public class ProductSearchSpec : BaseSpecification<Product>
{
    public ProductSearchSpec(string searchTerm, decimal? minPrice, decimal? maxPrice, int skip, int take)
    {
        // Build query dynamically
        AddCriteria(p => p.IsActive);
        
        if (!string.IsNullOrEmpty(searchTerm))
            AddCriteria(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
        
        if         if         if         if         if         if e >= minPrice.Value);
        
        if (maxPrice.HasValue)
                                      = maxPrice.Value);
        
        ApplyOrderBy(p => p.Name);
        ApplyPaging(skip, take);
    }
}

// Usage in controller
public async Task<IActionResult> Search(
    string? q, decimal? minPrice, decimal? maxPrice, 
    int page = 1, int pageSize = 10)
{
    var spec = new ProductSearchSpec(q, minPrice, maxPrice, (page - 1) * pageSize, pageSize);
    var result = await _repo.GetPagedAsync(spec, page, pageSize);
    return Ok(result);
}
`````````````````````````````````````````````````````````portProductsAsync(List<Product> products)
{
    var batchSize = 100;
    for (int i = 0; i < products.Count; i += batchSize)
    {
        var batch = products.Skip(i).Take(batchSize).ToList();
        await _repo.AddRangeAsync(batch);
    }
}

public async Task DeleteInactiveProductsAsync(int daysInactive)
{
    var spec = new InactiveProductsSpec(daysInactive);
    var inactiveProducts = await _repo.GetAsync(spec);
    
    if (inactiveProducts.Any())
        await _repo.DeleteRangeAsync(inactiveProducts);
}
```

### Testing

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
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repo = new EfRepository<Product>(_context        _repo = new EfRepository<Product>(_context        _repo = new EfRepository<Product>(_cont  // Arrange
        var product = new Product { Name = "Test Product", Price = 99.99m };

        // Act
        await _repo.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _repo.GetByIdAsync(product.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test Product"));
    }

    [Test    [Test    [Test    [TestPaged    [Test    [Test    [Test    [TestPaged    [Test    {
        // Arrange
        var products = Enumerable.Range(1, 25)
            .Select(i => new Product { Name = $"Product {i}", Price = i * 10m })
            .ToList();
        
        await _repo.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var spec = new AllProductsSpec();
        var result = await _repo.GetPagedAsync(spec, pageNumber: 2, pageSize:         var result = await _repo.GetPagedAsync(spec, pageNumber: 2, pageSize:         var result = await _repo.GetPagedAsync(spec, pageNumber: 2, pageSize: ult.        var result = await _repo.GetPagedAsync(spec, pageNumber: 2, ps.EqualTo(3));
        Assert.That(result.Items.Count, Is.EqualTo(10));
    }
}
```

---

## 🎯 Best Practices

1. **Use `IReadRepository<T>` for queries only**
   ```csharp
   // ✅ Correct
   private readonly IReadRepository<Product> _queryRepo;
   
   // ❌ Avoid if only reading
   private readonly IRepository<Product> _repo;
   ```

2. **Separate read and write concerns**
   ```csharp
   // ✅ Better
   private readonly IReadRepository<Product> _readRepo;
   private readonly IRepository<Product> _writeRepo;
   ```

3. **Use specifications for complex queries**
   ```csharp
   // ✅ Recommended
   var spec = new ProductsByPriceRangeSpec(100, 500);
   var products = await _repo.GetAsync(spec);
   
   // ❌ Avoid
   var products = await _repo.GetAllAsync(); // Then filter in memory
   ```

4. **Always paginate large result sets**
   ```csharp
   // ✅ Efficient
   var paged = await _repo.GetPagedAsync(spec, 1, 50);
   
   // ❌ Risky with large datasets
   var all = await _repo.GetAllAsync();
   ```

5. **Use Unit of Work for multi-repository operations**
   ```csharp
   // ✅ Atomic
   await _unitOfWork.BeginTransactionAsync();
   try
   {
       // Multiple repository operations
       await _unitOfWork.CommitTransactionAsync();
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync();
   }
   ```

6. **Choose ORM based on use case**
   ```csharp
   // ✅ EF Core for standard CRUD
   // ✅ Dapper for complex reporting/analytics
   ```

---

## 🔧 Configuration

### Entity Framework Core Setup

```c```c```c```c```c```c```c```c```c```c```c```c{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}

// Program.cs
builder.Services
    .AddDbContext<AppDbContext>(options => 
        options.UseSqlServer("your-connection-string"))
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositories(typeof(Program).Assembly);
```

### Dapper Setup

```csharp
// Program.cs
builder.Services
    .AddAutoRepositoryDapper(sp => 
        new SqlConnection("your-connection-string"))
    .AddAutoRepositories(typeof(Program).Assembly);
```

### Multiple Contexts

```csharp
[AutoRepository(Lifetime = ServiceLifetime.Transient)]
public class TenantSpecificEntity { }

// Register context-specific repositories
builder.Services
    .AddDbContext<AppDbContext>()
    .AddDbContext<TenantDbContext>()
    .AddAutoRepositoryEfCore<AppDbContext>()
    .AddAutoRepositoryEfCore<TenantDbContext>()
    .AddAutoRepositories(typeof(Program).Assembly);
```

---

## 📋 API Reference

### IRepository<T> / IReadRepository<T>

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetByIdAsync<TKey>(key)` | `T?` | Get entity by primary key |
| `GetAllAsync()` | `IReadOnlyList<T>` | Get all entities |
| `GetAsync(spec)` | `IReadOnlyList<T>` | Get entities| `GetAsync(spec)` | `IReadOnlyList<T>` | GultAsync(spec)` | `T?` | Get first matching entity or null |
| `GetPagedAsync(spec, pageNumber, pageSize)` | `PagedResult<T>` | Get paged results |
| `CountAsync(spec)` | `int` | Count entities matching specification |
| `AnyAsync(spec)` | `bool` | Check if any entity matches specification |
| `AddAsync(entity)` | `Task` | Add single entity |
| `AddRangeAsync(entities)` | `Task` | Add mul| `AddRangeAsync(entities)` | `Task` | ` | `Task` | Update entity |
| `DeleteAsync(entity)` | `Task` | Delete entity |
| `DeleteByIdAsync(id)` | `Task` | Delete by primary key |
| `DeleteRangeAsync(entities)` | `Task` | Delete multiple entities |

### PagedResult<T> Properties

```csh```csh```csh```csh``edR```csh```csh```csh```csh``edR```csh``> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { ge    public bool HasPreviousPage { ge    public bool HasPreviousPage { ge    public bool HasPreviousPage { ge    public bool HasPreviousPage { ge    public b AddCriteria(Expression<Func<T, bool>> criteria);
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy);
    protected void AddThenBy(Expression<    protected void AddThenBy(Expression<    protected void AddThenBy(ExpdInclude(Expression<Func<T, object>> navigationProperty);
    protected void AddInclude(string navigationPath);
    protected void ApplyPaging(int skip, int take);
    protected void UseRawSql(string sql, object? parameters = null);
}
```

---

## ⚙️ Requirements

- **.NET 8.0+** (supports 8.0, 9.0, 10.0)
- **C# 12+**
- Entity Framework Core 8.0+ (for RepoKit.EfCore)
- Dapper 2.1+ (for RepoKit.Dapper)

---

## 📝 License

MIT License - See [LICENSE](LIMIT License - See [LICENSE](LI
## �## �## �## �## �## �## �## �##  a## �## �## �## �## �## �## �## �#IN## �## �## �## �## �## �## �## �#ines.

---

## 📚 Resources

- [Quick Start Guide](QUICK_START.md)
- [Publishing Guide](PUBLISHING_GUIDE.md)
- [Changelog](CHANGELOG.md)
- [Contributing Guidelines](CONTRIBUTING.md)

---

**Made with ❤️ by Sourav Das**
