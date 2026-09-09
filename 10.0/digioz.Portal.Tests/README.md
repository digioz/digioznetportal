# digioz.Portal.Tests

Comprehensive test suite for the digioz Portal application.

## Test Organization

### Unit Tests (`Unit/`)
Tests for individual components in isolation with mocked dependencies.

- **Services/** - Tests for service layer (Dal.Services)
- **Utilities/** - Tests for utility classes and helpers
- **ViewModels/** - Tests for view model logic
- **Helpers/** - Tests for helper classes

### Integration Tests (`Integration/`)
Tests that verify components work together correctly.

- **Pages/** - Tests for Razor Pages with real dependencies
- **Database/** - Tests for EF Core database operations
- **Services/** - Tests for service integration with database

### End-to-End Tests (`E2E/`)
Full application tests simulating real user scenarios (optional/future).

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run with coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run specific test category
```bash
dotnet test --filter TestCategory=Unit
dotnet test --filter TestCategory=Integration
```

## Test Naming Convention

Tests follow the pattern: `MethodName_Scenario_ExpectedResult`

Example: `Get_WithValidId_ReturnsPage`

## Coverage Goals

- **Critical Services**: 90%+ coverage
- **Business Logic**: 80%+ coverage
- **Utilities**: 95%+ coverage (especially security-related)
- **UI/Pages**: 60%+ coverage

## Key Testing Areas

1. **High Priority**
   - PageService - Core content management
   - CommentService - User-generated content
   - PollService - Voting logic
   - OrderService - E-commerce transactions
   - InputSanitizer - Security validation
   - LinkCheckerService - Link validation

2. **Medium Priority**
   - StringUtils - String manipulation
   - CommentsHelper & UserHelper - Business logic
   - Razor Pages - Admin and user-facing pages

3. **Provider Testing**
   - PaymentProviders - Payment processing
   - EmailProviders - Email sending

## Best Practices

- Use AAA pattern (Arrange, Act, Assert)
- One logical assertion per test
- Use descriptive test names
- Keep tests independent and isolated
- Mock external dependencies
- Test edge cases and error conditions
