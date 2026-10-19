# Test Architecture & Development Guide

**File:** NET10_TEST_ARCHITECTURE.md  
**Purpose:** Reference guide for developers working with the digioznetportal test suite  
**Audience:** Development team, QA engineers, new contributors

---

## Quick Start

### Running All Tests

```powershell
# Run all tests
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj

# Run tests with verbose output
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --verbosity detailed

# Run tests with code coverage
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj /p:CollectCoverage=true
```

### Running Specific Test Files

```powershell
# Run a single test class
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --filter "TestClass=digioz.Portal.Tests.Unit.Services.PageServiceTests"

# Run tests matching a pattern
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --filter "Name~PageService"
```

### Running from Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Search for specific tests or classes
3. Right-click and select "Run" or "Debug"

---

## Test File Structure

### Naming Convention

```
[ServiceName]ServiceTests.cs
```

Examples:
- `PageServiceTests.cs` → Tests for `PageService`
- `AspNetUserServiceTests.cs` → Tests for `AspNetUserService`
- `EmailNotificationServiceTests.cs` → Tests for `IEmailNotificationService`

### Class Structure

```csharp
using NUnit.Framework;
using FluentAssertions;
using Moq;  // if testing async/mock scenarios
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using Microsoft.EntityFrameworkCore;

namespace digioz.Portal.Tests.Unit.Services
{
	[TestFixture]
	public class ExampleServiceTests
	{
		private DbContextOptions<digiozPortalContext> _options;
		private digiozPortalContext _dbContext;
		private ExampleService _service;

		[SetUp]
		public void Setup()
		{
			// Create fresh InMemory context for test isolation
			_options = new DbContextOptionsBuilder<digiozPortalContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			_dbContext = new digiozPortalContext(_options);
			_service = new ExampleService(_dbContext);
		}

		[TearDown]
		public void TearDown()
		{
			_dbContext?.Dispose();
		}

		// Test methods follow below...
	}
}
```

---

## Test Method Naming

### Pattern: `[MethodName]_[Scenario]_[ExpectedResult]`

**Examples:**

```csharp
[Test]
public void Get_WithValidId_ReturnsEntity()
{ }

[Test]
public void GetAll_WithNoRecords_ReturnsEmptyList()
{ }

[Test]
public void Add_WithValidEntity_InsertsSuccessfully()
{ }

[Test]
public void Update_WithExistingEntity_UpdatesSuccessfully()
{ }

[Test]
public void Delete_WithValidId_RemovesEntity()
{ }

[Test]
public void Search_WithMultipleCriteria_ReturnsFiltered()
{ }

[Test]
public void GetPaged_WithValidPageNumber_ReturnsPaginatedResults()
{ }

[Test]
public void Add_WithDuplicateKey_ThrowsException()
{ }

[Test]
public void Update_WithNullEntity_ThrowsArgumentNullException()
{ }
```

---

## Standard Test Patterns

### Pattern 1: CRUD Testing

```csharp
[Test]
public void Add_WithValidEntity_InsertsIntoDatabase()
{
	// Arrange
	var entity = new Page
	{
		UserId = "test-user",
		Title = "Test Page",
		Url = "test-page",
		Body = "This is a test page",
		Keywords = "test",
		Description = "A test",
		Visible = true,
		Timestamp = DateTime.UtcNow
	};

	// Act
	var result = _service.Add(entity);

	// Assert
	result.Id.Should().BeGreaterThan(0);
	_service.Get(result.Id).Should().NotBeNull();
	_service.Get(result.Id).Title.Should().Be("Test Page");
}

[Test]
public void Update_WithExistingEntity_ModifiesDatabase()
{
	// Arrange
	var entity = new Page { /* ... */ };
	var added = _service.Add(entity);

	// Act
	added.Title = "Updated Title";
	_service.Update(added);

	// Assert
	var updated = _service.Get(added.Id);
	updated.Title.Should().Be("Updated Title");
}

[Test]
public void Delete_WithValidId_RemovesEntity()
{
	// Arrange
	var entity = new Page { /* ... */ };
	var added = _service.Add(entity);

	// Act
	_service.Delete(added.Id);

	// Assert
	_service.Get(added.Id).Should().BeNull();
}
```

### Pattern 2: Search & Filtering

```csharp
[Test]
public void Search_WithCriteria_ReturnsMatchingResults()
{
	// Arrange
	_service.Add(new Page { Title = "Products" });
	_service.Add(new Page { Title = "Services" });
	_service.Add(new Page { Title = "About Products" });

	// Act
	var results = _service.Search("Products");

	// Assert
	results.Should().HaveCount(2);
	results.Should().AllSatisfy(p => p.Title.Should().Contain("Products"));
}

[Test]
public void GetPaged_WithValidPageNumber_ReturnsPaginatedResults()
{
	// Arrange
	for (int i = 0; i < 25; i++)
	{
		_service.Add(new Page { Title = $"Page {i}" });
	}

	// Act
	var page1 = _service.GetPaged(1, 10);

	// Assert
	page1.Should().HaveCount(10);
}
```

### Pattern 3: Relationship Testing

```csharp
[Test]
public void Delete_WithCascadeConfig_RemovesRelatedEntities()
{
	// Arrange
	var parent = _service.Add(new Parent { Name = "Parent" });
	var child = _childService.Add(new Child { ParentId = parent.Id, Name = "Child" });

	// Act
	_service.Delete(parent.Id);

	// Assert
	_childService.Get(child.Id).Should().BeNull();
}
```

### Pattern 4: Async Testing with Mocks

```csharp
[Test]
public async Task SendEmailAsync_WithValidParameters_ReturnsTrue()
{
	// Arrange
	var mockService = new Mock<IEmailNotificationService>();
	mockService
		.Setup(s => s.SendEmailAsync(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>()))
		.ReturnsAsync(true);

	// Act
	var result = await mockService.Object.SendEmailAsync(
		"test@example.com",
		"Subject",
		"Body");

	// Assert
	result.Should().BeTrue();
	mockService.Verify(
		s => s.SendEmailAsync(
			It.IsAny<string>(),
			It.IsAny<string>(),
			It.IsAny<string>()),
		Times.Once);
}
```

### Pattern 5: Edge Case Testing

```csharp
[Test]
public void Add_WithNullEntity_ThrowsArgumentNullException()
{
	// Assert
	Assert.Throws<ArgumentNullException>(() => _service.Add(null));
}

[Test]
public void Get_WithInvalidId_ReturnsNull()
{
	// Act
	var result = _service.Get(-1);

	// Assert
	result.Should().BeNull();
}

[Test]
public void GetAll_WithNoRecords_ReturnsEmptyList()
{
	// Act
	var results = _service.GetAll();

	// Assert
	results.Should().BeEmpty();
}
```

---

## Common Assertions

### Using FluentAssertions

```csharp
// Null checks
entity.Should().NotBeNull();
entity.Should().BeNull();

// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Collections
results.Should().HaveCount(5);
results.Should().BeEmpty();
results.Should().Contain(item);
results.Should().NotContain(item);
results.Should().AllSatisfy(r => r.IsActive.Should().BeTrue());

// Strings
name.Should().StartWith("Test");
name.Should().EndWith("Service");
name.Should().Contain("Service");

// Numerics
count.Should().BeGreaterThan(0);
count.Should().BeLessThanOrEqualTo(100);
value.Should().BeInRange(1, 10);

// Boolean
isActive.Should().BeTrue();
isActive.Should().BeFalse();

// Exceptions
var ex = Assert.Throws<InvalidOperationException>(() => method());
ex.Message.Should().Contain("Expected message");
```

---

## Troubleshooting Common Issues

### Issue: "The InMemory database does not support transactions"

**Solution:** Don't wrap InMemory test operations in transactions. If testing transaction behavior, use SQLite or SQL Server integration tests.

```csharp
// ❌ Don't do this in InMemory tests
using (var transaction = _dbContext.Database.BeginTransaction())
{
	_service.Add(entity);
	transaction.Commit();
}

// ✅ Do this instead
_service.Add(entity);
```

### Issue: "ExecuteUpdate is not supported by the InMemory database provider"

**Solution:** Mark the test with `[Ignore]` and add a comment explaining why.

```csharp
[Test]
[Ignore("EF Core InMemory does not support ExecuteUpdate operations")]
public void IncrementViews_WithValidId_IncrementsViewCount()
{
	// This test requires SQL Server or SQLite
}
```

### Issue: "Foreign key constraint violated"

**Solution:** Ensure parent entities exist before adding child entities.

```csharp
[Test]
public void Add_WithInvalidForeignKey_ThrowsException()
{
	// Arrange
	var child = new Comment { PostId = 999 }; // Invalid parent ID

	// Assert
	Assert.Throws<DbUpdateException>(() => _service.Add(child));
}
```

### Issue: "Test passes locally but fails in CI"

**Solution:** Ensure test isolation by using unique GUIDs for InMemory database names.

```csharp
// ✅ Good: Unique database per test
var options = new DbContextOptionsBuilder<digiozPortalContext>()
	.UseInMemoryDatabase(Guid.NewGuid().ToString())
	.Options;

// ❌ Bad: Shared database across tests
var options = new DbContextOptionsBuilder<digiozPortalContext>()
	.UseInMemoryDatabase("TestDb")
	.Options;
```

---

## Adding New Service Tests

### Step-by-Step Guide

1. **Create the test file**

   Create `[ServiceName]ServiceTests.cs` in `10.0/digioz.Portal.Tests/Unit/Services/`

2. **Define the test class**

   ```csharp
   [TestFixture]
   public class NewServiceServiceTests
   {
	   // ...
   }
   ```

3. **Add Setup/TearDown**

   ```csharp
   [SetUp]
   public void Setup()
   {
	   var options = new DbContextOptionsBuilder<digiozPortalContext>()
		   .UseInMemoryDatabase(Guid.NewGuid().ToString())
		   .Options;
	   _dbContext = new digiozPortalContext(options);
	   _service = new NewServiceService(_dbContext);
   }

   [TearDown]
   public void TearDown()
   {
	   _dbContext?.Dispose();
   }
   ```

4. **Add CRUD tests**

   - Test Create (Add)
   - Test Read (Get, GetAll)
   - Test Update
   - Test Delete

5. **Add specialized tests**

   - Search/filtering if applicable
   - Pagination if applicable
   - Relationships if applicable

6. **Run tests locally**

   ```powershell
   dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --filter "Name~NewService"
   ```

7. **Run full suite to ensure no regressions**

   ```powershell
   dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj
   ```

---

## Debugging Tests

### Using Visual Studio Debugger

1. Set a breakpoint in the test method
2. Right-click the test in Test Explorer
3. Select "Debug Selected Tests"
4. Use F10/F11 to step through code

### Adding Debug Output

```csharp
[Test]
public void Example_WithDebug_PrintsValues()
{
	// Arrange
	var entity = new Page { Title = "Test" };

	// Act
	var result = _service.Add(entity);

	// Debug output
	Console.WriteLine($"Result ID: {result.Id}");
	Console.WriteLine($"Result Title: {result.Title}");

	// Assert
	result.Should().NotBeNull();
}
```

Output appears in Test Explorer's output pane.

### Unit Test Profiling

To measure test performance:

```powershell
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --logger:html
```

Opens an HTML report with timing information.

---

## Continuous Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
	runs-on: windows-latest
	steps:
	  - uses: actions/checkout@v2
	  - uses: actions/setup-dotnet@v1
		with:
		  dotnet-version: '10.0.x'
	  - run: dotnet restore 10.0/digioz.Portal.sln
	  - run: dotnet build 10.0/digioz.Portal.sln
	  - run: dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj --no-build --verbosity normal
```

---

## Performance Benchmarks

### Expected Test Execution Times

| Test Count | Expected Time | Hardware |
|-----------|---------------|----------|
| 100 tests | ~2-3 seconds | Modern workstation |
| 500 tests | ~10-15 seconds | Modern workstation |
| 1000+ tests | ~25-35 seconds | Modern workstation |

**Optimization Tips:**

- Minimize database initialization in Setup
- Use InMemory provider (not SQL Server) for unit tests
- Avoid heavy computations in tests
- Use Parallel Test Execution (add to .csproj):

```xml
<PropertyGroup>
  <ParallelizeTestCollections>true</ParallelizeTestCollections>
</PropertyGroup>
```

---

## Code Coverage

### Generating Coverage Reports

```powershell
# Using OpenCover and ReportGenerator
dotnet test 10.0/digioz.Portal.Tests/digioz.Portal.Tests.csproj `
  /p:CollectCoverage=true `
  /p:CoverletOutputFormat=opencover `
  /p:CoverletOutput=coverage.xml

# Generate HTML report
reportgenerator -reports:coverage.xml -targetdir:CoverageReport
```

### Coverage Targets

- **DAL Services:** 85%+ coverage
- **Business Logic:** 80%+ coverage
- **Utilities:** 90%+ coverage
- **Infrastructure:** 70%+ coverage

---

## Best Practices

✅ **Do:**
- Use descriptive test names
- Follow the Arrange-Act-Assert pattern
- Keep tests isolated and independent
- Use fresh DbContext per test
- Assert exactly one thing per test (or use AssertMultiple for related assertions)
- Document complex test logic with comments
- Use test fixtures for common setup

❌ **Don't:**
- Depend on test execution order
- Share state between tests
- Use real databases in unit tests
- Write overly complex assertions
- Ignore failing tests without investigation
- Test multiple unrelated concerns in one test
- Mock everything (sometimes real implementations are better)

---

## References

- [NUnit Documentation](https://docs.nunit.org/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/Moq/moq4/wiki/Quickstart)
- [EF Core InMemory Provider](https://docs.microsoft.com/en-us/ef/core/testing/)

---

**Last Updated:** 2024  
**Version:** 1.0

