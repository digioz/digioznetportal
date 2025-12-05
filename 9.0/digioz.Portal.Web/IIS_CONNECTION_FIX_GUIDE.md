# IIS Connection Reset Fix - Deployment Guide

## Problem
When uploading videos through IIS on the dev server, you get:
- `Microsoft.AspNetCore.Connections.ConnectionResetException: The client has disconnected`
- `System.Runtime.InteropServices.COMException: The specified network name is no longer available`

This happens because **IIS has its own request buffering and timeout limits** that are separate from Kestrel/ASP.NET Core configuration.

## Root Cause
1. **IIS request buffering** - IIS buffers the entire request before passing to ASP.NET Core
2. **IIS connection timeouts** - Default 2-minute timeout for connections
3. **Request size limits** - IIS default is 30MB max request size
4. **Minimum data rate** - Kestrel requires minimum upload speed, disconnects slow connections

## Solution - Multi-Layered Configuration

### 1. ? web.config (IIS Configuration)

**File**: `digioz.Portal.Web/web.config`

This file has been created with:
- `maxAllowedContentLength="2147483648"` - Allow 2GB uploads in IIS
- `uploadReadAheadSize="2147483647"` - Increase read-ahead buffer
- `requestTimeout="00:20:00"` - 20-minute timeout for ASP.NET Core module
- `maxRequestLength="2097151"` - 2GB in KB for asp.net
- `executionTimeout="3600"` - 1-hour execution timeout

**?? CRITICAL**: This file must be deployed to the server alongside your application DLLs.

### 2. ? Program.cs Updates

Added three key configurations:

#### A. FormOptions Configuration
```csharp
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2147483648; // 2GB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});
```

#### B. Kestrel Configuration
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2147483648; // 2GB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.MinRequestBodyDataRate = null; // CRITICAL: Disable minimum data rate
});
```

**Key Setting**: `MinRequestBodyDataRate = null` - This allows slow connections to upload without being disconnected.

#### C. Chunked Upload Middleware Registration
```csharp
app.UseChunkedUploadMiddleware(); // Add BEFORE UseStaticFiles()
```

### 3. ? ChunkedUploadMiddleware.cs (New File)

**File**: `digioz.Portal.Web/Middleware/ChunkedUploadMiddleware.cs`

This middleware dynamically disables limits for `/API/ChunkedUpload*` endpoints:
- Disables max request body size
- Disables minimum data rate
- Logs each configuration change for debugging

### 4. ? Client-Side Improvements

**Already implemented in chunked-upload.js:**
- 5MB chunk size (faster individual uploads)
- 3 retry attempts per chunk with exponential backoff
- 500ms delay between chunks
- 60-second timeout per chunk
- Better error messages

### 5. ? appsettings.json

**Already configured:**
```json
"ChunkedUpload": {
    "ChunkSizeInMB": 5,
    "MaxChunks": 2000,
    "ChunkExpirationMinutes": 60,
    "MaxRetries": 3,
    "RetryDelayMs": 1000,
    "ChunkDelayMs": 500
}
```

## Deployment Checklist

### Before Deploying:
- [x] Build solution successfully
- [x] Verify web.config is included in publish output
- [x] Verify ChunkedUploadMiddleware.cs is compiled

### On Dev Server:

1. **Stop IIS Application Pool**
   ```powershell
   Stop-WebAppPool -Name "YourAppPoolName"
   ```

2. **Deploy Files**
   - Copy all files from publish output
   - **VERIFY web.config is present** in root directory

3. **Verify web.config Location**
   ```
   C:\inetpub\wwwroot\YourApp\web.config  <-- Must exist here
   ```

4. **Start IIS Application Pool**
   ```powershell
   Start-WebAppPool -Name "YourAppPoolName"
   ```

5. **Check Application Logs**
   - Look for: "ChunkedUpload request detected"
   - Look for: "Disabled max request body size limit"
   - Look for: "Disabled minimum data rate"

### Testing After Deployment:

1. **Test Small Video** (< 50MB)
   - Should use standard upload
   - Should complete quickly

2. **Test Medium Video** (50-200MB)
   - Should use chunked upload
   - Should show: "Total chunks: 10-40, Chunk size: 5 MB"
   - Watch for retries in console

3. **Monitor Server Logs**
   - Check for "ConnectionResetException" - should not appear anymore
   - Check for middleware log entries

## Expected Behavior After Fix

### Console Output:
```
File size (200MB) requires chunked upload
Starting chunked upload: video.mp4 (200 MB)
Total chunks: 40, Chunk size: 5 MB
Max retries per chunk: 3, Delay between chunks: 500ms
Uploading chunk 1/40 (attempt 1/3)
Chunk 1/40 uploaded successfully
[500ms delay]
Uploading chunk 2/40 (attempt 1/3)
Chunk 2/40 uploaded successfully
...
```

### Server Logs:
```
ChunkedUpload request detected: /api/chunkedupload
Disabled max request body size limit for chunked upload
Disabled minimum data rate for chunked upload
```

### If Chunk Fails:
```
Chunk 5 upload failed (attempt 1/3): Network error...
Retrying in 1000ms...
Uploading chunk 5/40 (attempt 2/3)
Chunk 5/40 uploaded successfully
```

## Troubleshooting

### Still Getting Connection Reset?

1. **Check web.config is deployed**
   ```powershell
   Test-Path "C:\inetpub\wwwroot\YourApp\web.config"
   ```

2. **Check IIS Application Pool Settings**
   - Open IIS Manager
   - Select your Application Pool
   - Advanced Settings ? Connection Limit: Set to 0 (unlimited)
   - Advanced Settings ? Queue Length: Set to 65535

3. **Check IIS Server-Level Settings**
   - Open IIS Manager ? Server node
   - Configuration Editor
   - Section: `system.applicationHost/webLimits`
   - Set: `connectionTimeout` to `00:20:00`

4. **Check Windows Event Log**
   - Open Event Viewer
   - Windows Logs ? Application
   - Look for IIS errors around the time of upload failure

5. **Enable Detailed IIS Logging**
   - IIS Manager ? Site ? Logging
   - Select Fields ? Check all fields
   - Review logs in `C:\inetpub\logs\LogFiles\`

### Cloudflare Still Timing Out?

If chunks are still failing even with these fixes, Cloudflare is likely the culprit. Options:

1. **Create Upload Subdomain** (Recommended)
   - DNS: `upload.yourdevdomain.com` ? Server IP (DNS Only, gray cloud)
   - Bypasses Cloudflare entirely for uploads

2. **Cloudflare Page Rule**
   - URL: `yourdevdomain.com/API/ChunkedUpload*`
   - Settings: Disable Performance, Disable Security
   - This may help but subdomain is better

3. **Reduce Chunk Size Further**
   - In appsettings.json: `"ChunkSizeInMB": 2`
   - Faster per-chunk uploads = less likely to timeout
   - More chunks = longer total upload time

## Performance Impact

### Upload Times (200MB video, 10 Mbps connection):

| Configuration | Chunks | Time per Chunk | Total Time | Retries |
|--------------|--------|----------------|------------|---------|
| 20MB chunks | 10 | ~16s | ~2.7 min | 0-2 |
| 10MB chunks | 20 | ~8s | ~2.7 min | 0-2 |
| **5MB chunks** | **40** | **~4s** | **~3 min** | **0-1** |
| 2MB chunks | 100 | ~1.6s | ~3.5 min | 0 |

**Optimal**: 5MB chunks balance speed and reliability.

## Additional Server Configuration (Optional)

### IIS Application Pool Advanced Settings:

1. **Connection Timeout**: 00:20:00 (20 minutes)
2. **Idle Timeout**: 00:20:00 (prevent pool shutdown during uploads)
3. **Ping Enabled**: False (prevent health check interference)
4. **Rapid Fail Protection**: Disabled (during testing)
5. **Request Queue Limit**: 5000 (handle multiple uploads)

### applicationHost.config (Server-Level):

Location: `C:\Windows\System32\inetsrv\config\applicationHost.config`

Add/modify:
```xml
<system.applicationHost>
  <webLimits 
    connectionTimeout="00:20:00" 
    dynamicIdleThreshold="150" 
    headerWaitTimeout="00:05:00" 
    minBytesPerSecond="0" />
</system.applicationHost>
```

**?? Warning**: Modifying applicationHost.config affects ALL sites on the server. Use with caution.

## Success Criteria

? **Upload Successful** when:
1. No `ConnectionResetException` in server logs
2. All 40 chunks upload successfully (maybe 1-2 retries)
3. Assembly completes without error
4. Video saved to database and plays correctly
5. Temporary upload directory cleaned up

? **Still Failing** if:
1. "The client has disconnected" still appears in logs
2. Consistent failures at same chunk number
3. Multiple retries still exhausted
4. Assembly fails with "missing chunks"

If still failing after all these changes, the issue is likely:
- **Network instability** between client and server
- **Cloudflare configuration** (requires subdomain solution)
- **Antivirus/Firewall** on server interfering with connections
- **Load balancer** (if any) has its own timeout settings

## Rollback Plan

If something breaks:

1. **Restore Previous Files**
   - Keep backup of working version
   - Revert to previous publish

2. **Remove Middleware**
   - Comment out `app.UseChunkedUploadMiddleware();` in Program.cs
   - Redeploy

3. **Reset IIS Config**
   - Remove web.config changes
   - Restart Application Pool

---

**Implementation Date**: December 2025  
**Version**: 2.0 - IIS Connection Fix  
**Status**: ? Ready for Deployment
