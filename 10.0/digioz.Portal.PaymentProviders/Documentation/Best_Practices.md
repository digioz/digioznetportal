# Best Practices for Payment Processing

This document outlines best practices for using the `digioz.Portal.PaymentProviders` library safely and effectively.

## Security Best Practices

### 1. Credential Management

**DO:**
- Store credentials in secure configuration (Azure Key Vault, AWS Secrets Manager)
- Use different credentials for test and production environments
- Rotate credentials regularly
- Use IAM roles instead of hardcoded credentials when possible

**DON'T:**
- Store credentials in appsettings.json in source control
- Log credentials or API keys
- Share credentials between developers
- Use the same credentials for multiple applications

```csharp
// Good: Using Azure Key Vault
services.AddKeyVaultConfiguration("https://yourvault.vault.azure.net/");

// Or using User Secrets for development
builder.Configuration.AddUserSecrets<Program>();
```

### 2. Card Data Handling

**DO:**
- Accept card data only through HTTPS/TLS
- Never store full card numbers in your database
- Never log card numbers, CVV codes, or expiration dates
- Use tokenization for recurring payments
- Implement PCI DSS compliance

**DON'T:**
- Store card data in logs, databases, or session state
- Transmit card data unencrypted
- Keep card data longer than necessary
- Handle raw card data directly

```csharp
// Bad: Never do this
logger.LogInformation($"Processing payment with card: {creditCardNumber}");

// Good: Log only non-sensitive identifiers
logger.LogInformation("Processing payment for transaction: {TransactionId}", transactionId);
```

### 3. Request Validation

**DO:**
- Validate all user input before processing
- Verify payment amounts match order totals
- Validate card format before sending to provider
- Check for fraud indicators

**DON'T:**
- Trust client-side validation alone
- Allow negative amounts
- Skip validation on internal transactions
- Process requests with mismatched amounts

```csharp
// Good: Server-side validation
if (checkoutAmount != cartTotal)
{
    throw new InvalidOperationException("Cart total mismatch");
}

if (!IsValidCardNumber(creditCard))
{
    return BadRequest("Invalid card number format");
}
```

## Implementation Best Practices

### 1. Error Handling

**DO:**
- Provide clear error messages to users
- Log detailed error information for debugging
- Distinguish between user errors and system errors
- Implement retry logic for transient failures

**DON'T:**
- Expose provider-specific error details to users
- Fail silently
- Retry too aggressively (respect provider rate limits)
- Lose error information

```csharp
// Good: Clear error messages and logging
try
{
    var response = await provider.ProcessPaymentAsync(request);
    
    if (!response.IsApproved)
    {
        logger.LogWarning("Payment declined: {ErrorCode} - {ErrorMessage}",
            response.ErrorCode,
            response.ErrorMessage);
        
        return new
        {
            success = false,
            userMessage = "Your payment was declined. Please try another card.",
            transactionId = response.TransactionId
        };
    }
}
catch (ArgumentException ex)
{
    logger.LogError(ex, "Invalid payment request");
    return BadRequest("Please check your payment information");
}
catch (Exception ex)
{
    logger.LogError(ex, "Payment processing error");
    return StatusCode(500, "Please try again later");
}
```

### 2. Idempotency

**DO:**
- Use unique transaction IDs to prevent duplicate charges
- Implement idempotency keys for critical operations
- Check for existing transactions before processing
- Support retries safely

**DON'T:**
- Process the same payment twice
- Allow duplicate transactions
- Lose transaction history

```csharp
// Good: Idempotent payment processing
var request = new PaymentRequest
{
    TransactionId = order.Id, // Unique identifier
    Amount = order.Total * 100,
    // ... other fields
};

// If the request is retried, the TransactionId remains the same
var response = await provider.ProcessPaymentAsync(request);
```

### 3. Transaction Recording

**DO:**
- Record all transaction details immediately after processing
- Store provider response codes and messages
- Maintain audit trail of payment attempts
- Track refunds and adjustments

**DON'T:**
- Lose transaction information
- Overwrite transaction history
- Process refunds without tracking
- Lose failed transaction details

```csharp
// Good: Complete transaction recording
order.TrxApproved = response.IsApproved;
order.TrxAuthorizationCode = response.AuthorizationCode;
order.TrxId = response.TransactionId;
order.TrxMessage = response.Message;
order.TrxResponseCode = response.ResponseCode;
order.Ccamount = response.Amount / 100m;

// Record the full response for debugging
var transactionLog = new PaymentTransactionLog
{
    OrderId = order.Id,
    Provider = providerName,
    Amount = response.Amount,
    IsApproved = response.IsApproved,
    AuthCode = response.AuthorizationCode,
    TransactionId = response.TransactionId,
    ErrorCode = response.ErrorCode,
    ErrorMessage = response.ErrorMessage,
    RawResponse = JsonConvert.SerializeObject(response.RawResponse),
    ProcessedAt = DateTime.UtcNow
};

await _transactionLogRepository.AddAsync(transactionLog);
```

### 4. Testing

**DO:**
- Use test/sandbox credentials for development
- Test with provider test card numbers
- Mock providers in unit tests
- Test error scenarios
- Load test payment processing

**DON'T:**
- Test with real payment cards
- Use production credentials in development
- Skip testing edge cases
- Test directly in production

```csharp
// Good: Unit test with mock provider
[TestMethod]
public async Task ProcessPayment_WithDeclinedCard_ReturnsError()
{
    // Arrange
    var mockProvider = new Mock<IPaymentProvider>();
    mockProvider.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
        .ReturnsAsync(new PaymentResponse
        {
            IsApproved = false,
            ErrorCode = "CARD_DECLINED",
            ErrorMessage = "Card declined"
        });

    var service = new PaymentService(mockProvider.Object);
    var request = new PaymentRequest
    {
        CardNumber = "4000000000000002", // PayPal test declined card
        Amount = 10000
    };

    // Act
    var response = await service.ProcessPaymentAsync(request);

    // Assert
    Assert.IsFalse(response.IsApproved);
    Assert.AreEqual("CARD_DECLINED", response.ErrorCode);
}
```

### 5. Async/Await

**DO:**
- Use async/await for all I/O operations
- Properly handle task cancellation
- Use ConfigureAwait(false) in libraries

**DON'T:**
- Block on async operations (deadlocks)
- Ignore task exceptions
- Use Task.Result or Task.Wait

```csharp
// Good: Proper async/await
public async Task<PaymentResponse> ProcessAsync(PaymentRequest request)
{
    return await provider.ProcessPaymentAsync(request).ConfigureAwait(false);
}

// Bad: Blocking on async
public PaymentResponse Process(PaymentRequest request)
{
    return provider.ProcessPaymentAsync(request).Result; // Deadlock!
}
```

## Operational Best Practices

### 1. Monitoring and Logging

**DO:**
- Monitor payment success rates
- Log all payment attempts (without sensitive data)
- Set up alerts for unusual patterns
- Track provider health and availability

**DON'T:**
- Ignore payment failures
- Log card data or CVV codes
- Lose transaction history
- Miss fraud indicators

```csharp
// Good: Structured logging
logger.LogInformation("Payment processing started for order {OrderId}", orderId);
logger.LogInformation("Payment provider: {Provider}, Amount: {Amount} cents",
    providerName, amountInCents);
logger.LogInformation("Payment result: Approved={IsApproved}, TransactionId={TransactionId}",
    response.IsApproved, response.TransactionId);

// Monitor metrics
metrics.RecordPaymentAttempt(providerName, response.IsApproved);
metrics.RecordPaymentAmount(response.Amount);
```

### 2. Rate Limiting

**DO:**
- Implement rate limiting to prevent abuse
- Respect provider rate limits
- Implement exponential backoff for retries
- Implement circuit breaker patterns

**DON'T:**
- Hammer the provider with requests
- Retry infinitely
- Ignore provider throttling responses

```csharp
// Good: Rate limiting and retry logic
var retryPolicy = Policy.Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var circuitBreaker = Policy.Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30));

var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreaker);

var response = await combinedPolicy.ExecuteAsync(
    () => provider.ProcessPaymentAsync(request));
```

### 3. Reconciliation

**DO:**
- Regularly reconcile payments with provider statements
- Investigate discrepancies
- Maintain audit trails
- Handle provider refunds/chargebacks

**DON'T:**
- Trust only local records
- Ignore discrepancies
- Lose reconciliation data
- Fail to respond to chargebacks

```csharp
// Good: Reconciliation tracking
public class PaymentReconciliation
{
    public string TransactionId { get; set; }
    public string ProviderId { get; set; }
    public decimal Amount { get; set; }
    public bool LocalRecordExists { get; set; }
    public bool ProviderRecordExists { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public string DiscrepancyNotes { get; set; }
}
```

## Configuration Best Practices

### 1. Environment-Specific Configuration

```json
{
  "PaymentProviders": {
    "AuthorizeNet": {
      "ApiKey": "${AUTHORIZE_NET_API_KEY}",
      "ApiSecret": "${AUTHORIZE_NET_API_SECRET}",
      "IsTestMode": false
    }
  }
}
```

### 2. Configuration Validation

```csharp
services.AddPaymentProviders(builder =>
{
    builder.ConfigureProvider("AuthorizeNet", config =>
    {
        // Validate configuration on startup
        if (string.IsNullOrEmpty(config.ApiKey))
            throw new InvalidOperationException("Authorize.net ApiKey is not configured");
            
        if (string.IsNullOrEmpty(config.ApiSecret))
            throw new InvalidOperationException("Authorize.net ApiSecret is not configured");
    });
});
```

## Checklist for Production Deployment

- [ ] All sensitive credentials are in secure storage (not in code)
- [ ] HTTPS is enforced for all payment pages
- [ ] PCI DSS compliance is implemented
- [ ] Error handling is comprehensive and user-friendly
- [ ] Logging is configured (sensitive data excluded)
- [ ] Monitoring and alerts are set up
- [ ] Unit and integration tests pass
- [ ] Load testing has been performed
- [ ] Disaster recovery plan is documented
- [ ] Provider documentation is reviewed and understood
- [ ] Test credentials work correctly
- [ ] Refund process is tested
- [ ] Compliance audit is complete
- [ ] Team is trained on payment handling procedures

