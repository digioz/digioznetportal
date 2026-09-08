# Link Checker Documentation

## Overview

The Link Checker is a production-ready ASP.NET Core service that validates and maintains the health of external links stored in the portal database. It performs concurrent HTTP checks, extracts metadata, and automatically updates link status while preventing common security vulnerabilities.

## Architecture

### Components

```
???????????????????????????????????????????????????????????????
?                     Link Checker System                      ?
???????????????????????????????????????????????????????????????
?                                                               ?
?  ????????????????????      ????????????????????            ?
?  ?  CheckLinks.     ?      ?  LinkChecker     ?            ?
?  ?  cshtml.cs       ????????  Service         ?            ?
?  ?  (Razor Page)    ?      ?  (Core Logic)    ?            ?
?  ????????????????????      ????????????????????            ?
?         ?                           ?                        ?
?         ?                           ?                        ?
?         ?                  ????????????????????             ?
?         ?                  ?  IHttpClient     ?             ?
?         ?                  ?  Factory         ?             ?
?         ?                  ????????????????????             ?
?         ?                           ?                        ?
?         ?                           ?                        ?
?         ?                  ????????????????????             ?
?         ?                  ?  Target Websites ?             ?
?         ?                  ????????????????????             ?
?         ?                                                    ?
?         ?                                                    ?
?  ????????????????????      ????????????????????            ?
?  ?  IServiceScope   ?      ?  ILinkService    ?            ?
?  ?  Factory         ????????  (Database)      ?            ?
?  ????????????????????      ????????????????????            ?
?                                                               ?
???????????????????????????????????????????????????????????????
```

### Key Classes

| Class | Location | Purpose |
|-------|----------|---------|
| `LinkCheckerService` | `digioz.Portal.Web/Services/` | Core service with HTTP checking logic |
| `CheckLinksModel` | `Areas/Admin/Pages/Link/` | Razor Page model for UI |
| `LinkCheckResult` | `digioz.Portal.Bo/ViewModels/` | Output result of link check |
| `LinkUpdateInfo` | `digioz.Portal.Bo/ViewModels/` | Captured changes for database update |

## Features

### 1. Concurrent HTTP Checks
- **Batch Processing**: Links checked in batches of 10 (configurable)
- **Parallel HTTP Requests**: Within each batch, HTTP checks run concurrently
- **Sequential Database Updates**: Database operations serialized to prevent threading issues

### 2. Smart HTTP Strategy
```
Step 1: Try HEAD request (faster, no body download)
   ?
   ?? Success ? Use response
   ?
   ?? HttpRequestException ? Try GET request
         ?
         ?? Use GET response
```

**Benefits:**
- HEAD request: ~50-100ms per link
- GET fallback: Ensures compatibility with all servers
- ResponseHeadersRead: Streams response, doesn't buffer entire body

### 3. Automatic Metadata Extraction

When a link has no description, the checker automatically:

1. Downloads up to 50KB of HTML (or until `</head>` tag)
2. Parses with HtmlAgilityPack (handles malformed HTML)
3. Extracts description in priority order:
   - `<meta name="description">`
   - `<meta property="og:description">` (Open Graph)
   - `<meta name="twitter:description">` (Twitter Cards)
   - `<title>` (fallback)
4. Cleans and truncates to 500 characters
5. Tags with `[DESCRIPTION UPDATED]`

### 4. Status Classification

| HTTP Status | Classification | Action |
|-------------|----------------|--------|
| 2xx | ? Success | Mark as valid, extract description if missing |
| 301, 302, 307, 308 | ?? Redirect | Hide link, tag `[REDIRECT LINK]` |
| 400, 403, 404 | ? Dead Link | Hide link, tag `[DEAD LINK]` |
| 500, 503 | ?? Server Error | Hide link, tag `[ERROR LINK]` |
| Timeout | ?? Timeout | Mark timeout, keep visible |
| Network Error | ?? Network Error | Hide link, tag `[DEAD LINK]` |

### 5. SSRF Protection

Comprehensive validation prevents Server-Side Request Forgery attacks:

#### Blocked URL Schemes
```
? Allowed:     http://, https://
? Blocked:     file://, ftp://, gopher://, data://, javascript://
```

#### Blocked Addresses

| Type | CIDR | Example | Purpose |
|------|------|---------|---------|
| Localhost | - | `localhost`, `127.0.0.1`, `::1` | Local services |
| Private (Class A) | 10.0.0.0/8 | `10.0.0.1` | Internal networks |
| Private (Class B) | 172.16.0.0/12 | `172.16.0.1` | Internal networks |
| Private (Class C) | 192.168.0.0/16 | `192.168.1.1` | Home/office networks |
| Link-Local | 169.254.0.0/16 | `169.254.169.254` | AWS/Azure metadata |

**Critical Protection**: Blocks access to cloud metadata services (AWS, Azure, GCP) at `169.254.169.254`

## Performance Optimizations

### 1. Memory Efficiency

**HTML Reading:**
```csharp
Max Read: 50KB (not entire page)
Stop Early: When </head> found
Streaming: No buffering entire response
```

**Impact:**
- Before: Could load 5MB+ HTML into memory
- After: Max 50KB per link
- Savings: ~95% memory reduction for large pages

### 2. Connection Pooling

```csharp
// HttpClient from IHttpClientFactory
builder.Services.AddHttpClient("LinkChecker", client => {
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});
```

**Benefits:**
- Reuses TCP connections
- Prevents socket exhaustion
- Handles DNS refresh properly

### 3. Batch Processing

```
100 links ? 10 batches of 10
Each batch:
  - 10 concurrent HTTP checks (parallel)
  - Sequential database updates (serialized)
  
Total time: ~15-30 seconds (vs 150+ seconds sequential)
```

### 4. Database Isolation

**Fresh DbContext per Update:**
```csharp
foreach (var updateInfo in updates) {
    using var scope = _scopeFactory.CreateScope();
    var linkService = scope.GetRequiredService<ILinkService>();
    
    var freshLink = linkService.Get(updateInfo.LinkId);
    // Apply changes
    linkService.Update(freshLink);
}
```

**Why:**
- Prevents stale DbContext issues
- Avoids detached entity problems
- Ensures thread safety
- Each update has clean change tracker

## Thread Safety

### Race Condition Prevention

**Problem:** Concurrent tasks modifying shared Link objects

**Solution:** Capture changes independently
```csharp
// Each task creates its own update info
var updateInfo = new LinkUpdateInfo {
    LinkId = link.Id,
    Visible = link.Visible,        // Captured at this moment
    Description = link.Description  // Captured at this moment
};
```

**Result:** No shared mutable state between concurrent operations

### Sequential Database Updates

```csharp
await _dbSemaphore.WaitAsync(cancellationToken);
try {
    // Only one thread can update database at a time
    linkService.Update(freshLink);
}
finally {
    _dbSemaphore.Release();
}
```

## Resource Management

### 1. HttpClient Lifecycle
- ? Created from `IHttpClientFactory` (managed by framework)
- ? Reused across requests via connection pooling
- ? Automatically disposed by factory

### 2. HttpRequestMessage Disposal
```csharp
using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
// Automatically disposed at end of scope
```

### 3. HttpResponseMessage Disposal
```csharp
HttpResponseMessage response = null;
try {
    response = await httpClient.SendAsync(headRequest);
}
catch (HttpRequestException) {
    response?.Dispose();  // Cleanup failed response
    response = null;
    // Retry with GET
}
catch {
    response?.Dispose();  // Cleanup on any exception
    throw;
}

using (response) {  // Final disposal
    // Process response
}
```

### 4. Service Disposal
```csharp
public class LinkCheckerService : IDisposable {
    private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);
    
    public void Dispose() {
        _dbSemaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## Cancellation Support

### User-Initiated Cancellation
```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
    _logger.LogInformation("Cancelled by user");
    throw;  // Propagate to stop processing
}
```

### Timeout Handling
```csharp
catch (OperationCanceledException) {
    // Timeout from HttpClient (not user)
    result.Status = LinkCheckStatus.Timeout;
    result.Message = "Request timed out (15s)";
}
```

**Distinction:**
- User cancellation ? Stop all processing
- HTTP timeout ? Mark as timeout, continue with next link

## Usage

### Admin Interface

**Location:** `/Admin/Link/CheckLinks`

**Requirements:** Administrator role

**Workflow:**
1. Navigate to Link Checker page
2. Click "Start Link Check"
3. View real-time progress (batch processing)
4. Review detailed results:
   - Total links checked
   - Success count
   - Dead links
   - Errors
   - Redirects
   - Updated links

### Programmatic Usage

```csharp
public class MyService
{
    private readonly LinkCheckerService _linkChecker;
    
    public MyService(LinkCheckerService linkChecker)
    {
        _linkChecker = linkChecker;
    }
    
    public async Task CheckLinksAsync()
    {
        var results = await _linkChecker.CheckAllLinksAsync(
            batchSize: 10,
            CancellationToken.None
        );
        
        foreach (var result in results)
        {
            Console.WriteLine($"{result.LinkName}: {result.Status}");
        }
    }
}
```

## Configuration

### HttpClient Settings

**File:** `Program.cs`

```csharp
builder.Services.AddHttpClient("LinkChecker", client => {
    client.Timeout = TimeSpan.FromSeconds(15);  // Adjust timeout
    client.DefaultRequestHeaders.Add("User-Agent", "YourBotName/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});
```

### Batch Size

```csharp
// In CheckLinksModel.OnPostAsync()
Results = await _linkChecker.CheckAllLinksAsync(
    batchSize: 10,  // Adjust based on link count and network
    CancellationToken.None
);
```

**Recommendations:**
- Small link count (<50): batchSize = 10-20
- Medium link count (50-200): batchSize = 10
- Large link count (>200): batchSize = 5-10

### HTML Reading Limit

```csharp
// In LinkCheckerService.cs
private const int MaxHtmlBytesToRead = 51200; // 50KB
```

Increase if:
- Many sites have large `<head>` sections
- Description extraction frequently fails

## Logging

### Log Levels

| Level | When | Example |
|-------|------|---------|
| Information | Normal operation | "Starting link check for 50 links" |
| Warning | Non-critical issues | "Timeout checking link: https://example.com" |
| Error | Database update failure | "Failed to update link 123 in database" |

### Log Output

```
[12:34:56 INF] Starting link check for 50 links
[12:35:02 INF] Processed batch 1/5
[12:35:08 INF] Processed batch 2/5
[12:35:10 WRN] Timeout checking link: https://slow-site.com
[12:35:14 INF] Processed batch 3/5
[12:35:20 INF] Processed batch 4/5
[12:35:26 INF] Processed batch 5/5
[12:35:26 INF] Link check completed. Processed 50 links
```

## Security Considerations

### 1. SSRF Protection
? Only HTTP/HTTPS allowed  
? Private IPs blocked  
? Localhost blocked  
? Cloud metadata services blocked  

### 2. Resource Limits
? 15-second timeout per link  
? 50KB max HTML download  
? Connection pooling prevents socket exhaustion  

### 3. Input Validation
? URL format validation  
? Scheme validation  
? Host validation  

### 4. Authorization
? Admin-only access  
? Role-based authorization  

## Error Handling

### Network Errors
```csharp
catch (HttpRequestException ex) {
    link.Visible = false;
    link.Description = PrependTag("[DEAD LINK]", link.Description);
    result.Status = LinkCheckStatus.NetworkError;
    result.Message = $"Network error: {ex.Message}";
}
```

### Database Errors
```csharp
catch (Exception ex) {
    _logger.LogError(ex, "Failed to update link");
    result.Message += " (DB update failed)";
    // Continue with next link
}
```

### Timeout Handling
```csharp
catch (OperationCanceledException) {
    result.Status = LinkCheckStatus.Timeout;
    result.Message = "Request timed out (15s)";
    // Continue with next link
}
```

## Testing

### Unit Testing

```csharp
[Fact]
public async Task CheckLinkStatusAsync_ValidUrl_ReturnsSuccess()
{
    // Arrange
    var mockFactory = new Mock<IHttpClientFactory>();
    var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK);
    var client = new HttpClient(mockHandler);
    mockFactory.Setup(f => f.CreateClient("LinkChecker")).Returns(client);
    
    var service = new LinkCheckerService(
        Mock.Of<IServiceScopeFactory>(),
        mockFactory.Object,
        Mock.Of<ILogger<LinkCheckerService>>()
    );
    
    var link = new Link { 
        Id = 1, 
        Url = "https://example.com",
        Name = "Test"
    };
    
    // Act
    var result = await service.CheckLinkStatusAsync(link, CancellationToken.None);
    
    // Assert
    Assert.Equal(LinkCheckStatus.Success, result.Status);
}
```

### Integration Testing

```csharp
[Fact]
public async Task CheckAllLinksAsync_ProcessesAllLinks()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.PostAsync(
        "/Admin/Link/CheckLinks",
        new FormUrlEncodedContent(new Dictionary<string, string>())
    );
    
    // Assert
    response.EnsureSuccessStatusCode();
}
```

## Performance Metrics

### Typical Performance

| Metric | Value |
|--------|-------|
| Links per batch | 10 |
| Batch processing time | 2-5 seconds |
| Total time (100 links) | 20-50 seconds |
| Memory per batch | <5MB |
| Concurrent connections | 10-20 |

### Bottlenecks

1. **Network latency**: Varies by target site
2. **Slow servers**: Some may take full 15s timeout
3. **Database updates**: Sequential, ~50-100ms each

## Troubleshooting

### Issue: Links Taking Too Long

**Symptoms:** Batch processing exceeds expected time

**Solutions:**
1. Reduce batch size from 10 to 5
2. Increase HTTP timeout if legitimate slow sites
3. Check network connectivity

### Issue: High Memory Usage

**Symptoms:** Memory grows during processing

**Solutions:**
1. Reduce batch size
2. Check for links with very large HTML
3. Lower `MaxHtmlBytesToRead` constant

### Issue: Database Update Failures

**Symptoms:** "(DB update failed)" in results

**Solutions:**
1. Check database connectivity
2. Verify DbContext configuration
3. Check for long-running transactions blocking updates

### Issue: SSRF Validation False Positives

**Symptoms:** Legitimate URLs being blocked

**Solutions:**
1. Review URL validation logic
2. Check if URL uses IP address (may need whitelisting)
3. Verify URL scheme is http/https

## Future Enhancements

### Potential Improvements

1. **Retry Logic**
   - Retry failed links automatically
   - Exponential backoff

2. **Scheduling**
   - Background job for automatic checks
   - Configurable intervals (daily, weekly)

3. **Reporting**
   - Email notifications for dead links
   - Export results to CSV/Excel

4. **Link Monitoring**
   - Track link health over time
   - Alert when previously working link fails

5. **Advanced Validation**
   - DNS rebinding protection
   - Content-type validation
   - Certificate validation

6. **Performance Optimization**
   - Redis cache for recent checks
   - Skip recently checked links
   - Adaptive batch sizing

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial implementation with basic link checking |
| 2.0 | 2024 | Added SSRF protection, improved error handling |
| 3.0 | 2024 | Optimized memory usage, added metadata extraction |

## Contributing

When modifying the Link Checker:

1. **Maintain SSRF Protection**: Don't weaken URL validation
2. **Preserve Thread Safety**: Keep database updates sequential
3. **Test Resource Disposal**: Verify no leaks with profiler
4. **Update Tests**: Add tests for new functionality
5. **Document Changes**: Update this README

## License

Part of digioz Portal project - See main project LICENSE file.

## Support

For issues or questions:
- GitHub Issues: https://github.com/digioz/digioznetportal/issues
- Project Wiki: [Link to wiki]
- Email: [Support email]

---

**Last Updated:** 2024
**Maintainer:** digioz Portal Team
