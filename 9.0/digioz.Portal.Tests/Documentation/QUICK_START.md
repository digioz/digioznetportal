# ?? Quick Start Guide - digioz.Portal.Tests

## Run Your First Tests (30 seconds)

### Option 1: Visual Studio
1. Open the solution in Visual Studio
2. Open **Test Explorer** (Test ? Test Explorer)
3. Click **Run All** ??
4. Watch your tests pass! ?

### Option 2: Command Line
```bash
cd C:\_Projects\digioz\digioznetportal\9.0\digioz.Portal.Tests
dotnet test
```

## View Test Coverage (2 minutes)

### PowerShell (Recommended)
```powershell
cd C:\_Projects\digioz\digioznetportal\9.0\digioz.Portal.Tests
.\run-tests-with-coverage.ps1
```

This will:
1. Run all tests
2. Collect coverage data
3. Generate HTML report
4. Ask if you want to open it in browser

### Manual Method
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Understanding Test Results

### ? Success Output
```
Passed! - Failed: 0, Passed: 90, Skipped: 0, Total: 90
```

### ? Failure Output
```
Failed TestName
  Expected: "expected value"
  Actual:   "actual value"
```

## Test Categories

Run specific test categories:

```bash
# Unit tests only
dotnet test --filter TestCategory=Unit

# Security tests only
dotnet test --filter TestCategory=Security

# Service tests only
dotnet test --filter TestCategory=Services

# Multiple categories
dotnet test --filter "TestCategory=Unit|TestCategory=Security"
```

## Adding Your First Test

### 1. Create Test Class
```csharp
using NUnit.Framework;
using FluentAssertions;

namespace digioz.Portal.Tests.Unit.Services
{
    [TestFixture]
    [Category("Unit")]
    public class MyNewServiceTests
    {
        [SetUp]
        public void Setup()
        {
            // Runs before each test
        }

        [Test]
        public void MyMethod_WithValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var input = "test";

            // Act
            var result = MyMethod(input);

            // Assert
            result.Should().Be("expected");
        }

        [TearDown]
        public void TearDown()
        {
            // Runs after each test
        }
    }
}
```

### 2. Run Your New Test
```bash
dotnet test --filter "FullyQualifiedName~MyNewServiceTests"
```

## Common Test Patterns

### Testing with In-Memory Database
```csharp
[Test]
public void TestWithDatabase()
{
    // Arrange
    var context = TestDataHelper.CreateInMemoryContext();
    var service = new MyService(context);
    
    // Act
    service.DoSomething();
    
    // Assert
    var result = context.MyEntities.FirstOrDefault();
    result.Should().NotBeNull();
    
    // Cleanup
    context.Database.EnsureDeleted();
    context.Dispose();
}
```

### Testing with Mock Dependencies
```csharp
[Test]
public void TestWithMock()
{
    // Arrange
    var mockService = new Mock<IMyDependency>();
    mockService.Setup(m => m.GetData()).Returns("test data");
    var service = new MyService(mockService.Object);
    
    // Act
    var result = service.Process();
    
    // Assert
    result.Should().Be("expected");
    mockService.Verify(m => m.GetData(), Times.Once);
}
```

### Testing Multiple Scenarios
```csharp
[Test]
[TestCase("input1", "output1")]
[TestCase("input2", "output2")]
[TestCase("input3", "output3")]
public void TestMultipleScenarios(string input, string expected)
{
    // Act
    var result = MyMethod(input);
    
    // Assert
    result.Should().Be(expected);
}
```

## Debugging Tests

### Visual Studio
1. Set a breakpoint in your test
2. Right-click the test ? **Debug**
3. Step through your code

### Command Line
```bash
# Run specific test with verbose output
dotnet test --filter "FullyQualifiedName~MyTest" --logger "console;verbosity=detailed"
```

## Test Naming Convention

```
MethodName_Scenario_ExpectedResult
```

Examples:
- `Get_WithValidId_ReturnsPage`
- `Add_WithNullInput_ThrowsException`
- `Update_WithExistingItem_UpdatesDatabase`

## FluentAssertions Cheat Sheet

```csharp
// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Null checks
result.Should().BeNull();
result.Should().NotBeNull();

// Strings
result.Should().StartWith("prefix");
result.Should().EndWith("suffix");
result.Should().Contain("substring");
result.Should().BeEmpty();

// Collections
list.Should().HaveCount(5);
list.Should().Contain(item);
list.Should().NotContain(item);
list.Should().BeEmpty();
list.Should().OnlyContain(i => i.IsValid);

// Booleans
result.Should().BeTrue();
result.Should().BeFalse();

// Numbers
number.Should().BeGreaterThan(5);
number.Should().BeLessThan(10);
number.Should().BeInRange(1, 100);

// Exceptions
Action act = () => MyMethod();
act.Should().Throw<ArgumentException>();
act.Should().NotThrow();
```

## Troubleshooting

### Tests Not Discovered
```bash
# Rebuild the test project
dotnet build digioz.Portal.Tests

# Clear test cache (Visual Studio)
# Test ? Test Explorer ? Clear All Results
```

### Coverage Not Generating
```powershell
# Install ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run coverage script again
.\run-tests-with-coverage.ps1
```

### In-Memory Database Issues
```csharp
// Always use unique database names
var options = new DbContextOptionsBuilder<MyContext>()
    .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
    .Options;
```

## Next Steps

1. **Run all tests** to see current coverage
2. **Review** [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) for full details
3. **Read** [README.md](README.md) for comprehensive guide
4. **Add tests** for your components

## Help & Resources

- **NUnit Docs**: https://docs.nunit.org/
- **FluentAssertions**: https://fluentassertions.com/
- **Moq**: https://github.com/moq/moq4
- **Coverlet**: https://github.com/coverlet-coverage/coverlet

---

**Happy Testing! ??**

Need help? Check the test examples in the `Unit/` folder.
