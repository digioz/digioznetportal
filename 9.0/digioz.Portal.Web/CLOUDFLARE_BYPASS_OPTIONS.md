# Cloudflare Bypass Configuration for Testing

## Option 1: Cloudflare Page Rule (Recommended for Testing)

Create a page rule to bypass Cloudflare for chunk uploads:

1. Log in to Cloudflare Dashboard
2. Select your domain
3. Go to "Rules" ? "Page Rules"
4. Create New Page Rule

**Settings:**
```
URL Match: yourdevdomain.com/API/ChunkedUpload*

Settings:
- Cache Level: Bypass
- Security Level: Essentially Off
- Browser Integrity Check: Off
- Disable Performance
- Disable Apps
```

This will make `/API/ChunkedUpload` requests go directly to your server without Cloudflare interference.

## Option 2: Firewall Rule (More Targeted)

1. Go to "Security" ? "WAF"
2. Create Firewall Rule

**Expression:**
```
(http.request.uri.path contains "/API/ChunkedUpload")
```

**Action:** Allow (bypasses rate limiting and security checks)

## Option 3: Direct IP Testing (Temporary)

For testing only, bypass Cloudflare completely:

1. Find your server's IP address
2. Add to your local hosts file:
   - Windows: `C:\Windows\System32\drivers\etc\hosts`
   - Add line: `123.45.67.89  yourdevdomain.com` (use actual IP)

?? This only works on your local machine for testing.

## Option 4: Development Subdomain (Permanent Solution)

If testing confirms it's Cloudflare causing the issue:

1. Create DNS record: `upload-dev.yourdevdomain.com`
2. Point to server IP
3. Set to "DNS Only" (gray cloud icon)
4. Update JavaScript to use this domain for uploads

**In chunked-upload.js:**
```javascript
getDefaultUploadUrl() {
    const hostname = window.location.hostname;
    
    if (hostname === 'yourdevdomain.com') {
        return 'https://upload-dev.yourdevdomain.com/API/ChunkedUpload';
    }
    
    return '/API/ChunkedUpload';
}
```

## Testing Without Cloudflare

To verify if Cloudflare is the issue:

1. Apply Page Rule (Option 1) above
2. Clear browser cache
3. Try upload again

**If upload works:** Cloudflare is the issue
**If upload still fails:** Issue is on your server or network

## Current Settings (Very Conservative)

With the latest changes:
- **2MB chunks** - Fastest possible individual uploads
- **1.5 second delay** between chunks - Prevents overwhelming connection
- **5 retries** per chunk with exponential backoff
- **90 second timeout** per chunk (plenty of time for 2MB)

For a 200MB video:
- **100 chunks** @ 2MB each
- **~2.5 seconds per chunk** (upload + delay)
- **~4 minutes total** (100 × 2.5s)

## Cloudflare Settings to Check

### 1. Timeout Settings
Default: 100 seconds for free plan
Your chunks should complete well under this.

### 2. Rate Limiting
Check if you have rate limiting rules that might block rapid sequential requests.

### 3. DDoS Protection
May interpret rapid chunk uploads as attack.

### 4. SSL/TLS Settings
- Full (Strict) SSL can cause issues
- Try "Full" instead

### 5. Upload Limits
Enterprise plans have different limits.

## Recommended Next Steps

1. **Apply Page Rule** (Option 1) - Takes 5 minutes
2. **Test upload** - See if it works
3. If works: Cloudflare is the issue, use subdomain solution
4. If fails: Issue is elsewhere (server/network)

## Alternative: Cloudflare Workers

If Page Rules don't work, create a Cloudflare Worker to handle uploads:

```javascript
addEventListener('fetch', event => {
  const url = new URL(event.request.url);
  
  // Bypass Cloudflare for chunk uploads
  if (url.pathname.startsWith('/API/ChunkedUpload')) {
    return event.respondWith(fetch(event.request, {
      cf: {
        cacheTtl: 0,
        cacheEverything: false
      }
    }));
  }
  
  return event.respondWith(fetch(event.request));
});
```

This gives you fine-grained control over how Cloudflare handles these requests.

---

**Bottom Line:** With 2MB chunks and 1.5s delays, if you're still getting SSL errors after 2 chunks, it's almost certainly Cloudflare terminating the connection. Apply the Page Rule to test this theory.
