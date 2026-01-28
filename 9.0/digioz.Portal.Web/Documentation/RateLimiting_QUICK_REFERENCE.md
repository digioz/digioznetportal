# Rate Limiting Quick Reference

## ?? Quick Start

### 1. Apply Database Migration
```bash
cd digioz.Portal.Web
dotnet ef database update
```

This will:
- Create the `BannedIp` table
- Insert rate limiting configuration into the `Config` table
- Add the "Rate Limiting & Bot Protection" plugin record (enabled by default)

### 2. Restart Application
The middleware is already registered in `Program.cs` and will start working immediately.

### 3. Access Admin Panel
Navigate to: `/Admin/Security/BannedIps`

### 4. Configure Settings (Optional)
Navigate to: `/Admin/Config/Index`

Rate limiting settings you can adjust:
- `RateLimit.MaxRequestsPerMinute` (default: 20)
- `RateLimit.MaxRequestsPer10Minutes` (default: 60)
- `RateLimit.BanDurationMinutes` (default: 60)
- `RateLimit.PermanentBanThreshold` (default: 5)
- `RateLimit.PasswordReset.MaxAttemptsPerIpPerHour` (default: 10)
- `RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour` (default: 3)

### 5. Enable/Disable Feature
Navigate to: `/Admin/Plugin/Index`

Find "Rate Limiting & Bot Protection" and toggle the `IsEnabled` flag to enable or disable the entire feature.

---

## ?? Current Configuration

Configuration is now stored in the **Config** table and can be changed without redeploying code.

| Setting | Config Key | Default Value | Description |
|---------|------------|---------------|-------------|
| Max Requests/Minute | `RateLimit.MaxRequestsPerMinute` | 20 | Per IP address |
| Max Requests/10 Minutes | `RateLimit.MaxRequestsPer10Minutes` | 60 | Per IP address |
| Ban Duration | `RateLimit.BanDurationMinutes` | 60 min | Temporary ban length |
| Permanent Ban Threshold | `RateLimit.PermanentBanThreshold` | 5 | Bans before permanent |
| Password Reset Limit (IP) | `RateLimit.PasswordReset.MaxAttemptsPerIpPerHour` | 10/hour | Per IP address |
| Password Reset Limit (Email) | `RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour` | 3/hour | Per email address |

**Note**: After changing config values, the new values take effect immediately on the next request.

---

## ?? Common Tasks

### Enable/Disable Rate Limiting
1. Go to `/Admin/Plugin/Index`
2. Find "Rate Limiting & Bot Protection"
3. Click "Edit"
4. Toggle the "Enabled" checkbox
5. Save

### Adjust Rate Limits
1. Go to `/Admin/Config/Index`
2. Find the rate limit config key you want to change
3. Click "Edit"
4. Change the value
5. Save (changes take effect immediately)

### View Banned IPs
```
URL: /Admin/Security/BannedIps
```

### Check Logs for Attacks
```
URL: /Admin/Logs
Filter: Level = Warning
Search: "Password reset requested for non-existent email"
```

### Manually Ban an IP
1. Go to `/Admin/Security/BannedIps`
2. Fill in the "Manually Ban IP Address" form
3. Choose duration (1 hour to Permanent)
4. Click "Ban IP Address"

### Unban an IP
1. Go to `/Admin/Security/BannedIps`
2. Find the IP in the list
3. Click "Unban" button
4. Confirm

### Cleanup Expired Bans
1. Go to `/Admin/Security/BannedIps`
2. Click "Cleanup Expired Bans" button
3. Confirm

---

## ?? Monitoring

### Key Log Messages

**Warning** - Suspicious Activity:
```
- "Password reset requested for non-existent email: {Email} from IP: {IpAddress}"
- "Blocked request from suspicious bot: {BotName} from IP: {IpAddress}"
- "Rate limit exceeded (1-minute window) for IP: {IpAddress}"
- "TEMPORARY BAN: IP {IpAddress} banned until {Expiry}"
```

**Error** - Critical Events:
```
- "PERMANENT BAN: IP {IpAddress} banned permanently"
```

### Admin Dashboard Statistics

The admin page shows:
- ?? **Active Bans**: Currently enforced bans
- ?? **Permanent Bans**: IPs banned forever
- ?? **Expired Bans**: Bans that are no longer active

---

## ??? Adjust Rate Limits

### For More Restrictive Limits
Edit `RateLimitingMiddleware.cs`:
```csharp
private const int MaxRequestsPerMinute = 10;       // Decrease from 20
private const int MaxRequestsPer10Minutes = 30;    // Decrease from 60
```

### For More Lenient Limits
Edit `RateLimitingMiddleware.cs`:
```csharp
private const int MaxRequestsPerMinute = 30;       // Increase from 20
private const int MaxRequestsPer10Minutes = 100;   // Increase from 60
```

### Change Ban Duration
Edit `RateLimitingMiddleware.cs`:
```csharp
private const int BanDurationMinutes = 120;        // Increase to 2 hours
// or
private const int BanDurationMinutes = 30;         // Decrease to 30 minutes
```

### Change Permanent Ban Threshold
Edit `RateLimitingMiddleware.cs`:
```csharp
private const int PermanentBanThreshold = 3;       // Permanent after 3 bans
// or
private const int PermanentBanThreshold = 10;      // Permanent after 10 bans
```

---

## ?? Troubleshooting

### Issue: Legitimate users getting banned
**Solution 1**: Adjust rate limits in `/Admin/Config/Index` (no code changes needed!)  
**Solution 2**: Add IP to whitelist (feature needs to be added)  
**Solution 3**: Manually unban the IP in `/Admin/Security/BannedIps`  
**Solution 4**: Temporarily disable rate limiting in `/Admin/Plugin/Index`

### Issue: Bots still getting through
**Solution 1**: Decrease rate limits in `/Admin/Config/Index`  
**Solution 2**: Add bot patterns to `BotHelper.cs`  
**Solution 3**: Consider adding CAPTCHA

### Issue: Need to disable rate limiting temporarily
**Solution**: Go to `/Admin/Plugin/Index`, find "Rate Limiting & Bot Protection", and set IsEnabled to false

### Issue: Rate limit changes not taking effect
**Solution**: Changes are immediate - verify you saved the config in `/Admin/Config/Index`

### Issue: Too many expired bans in database
**Solution 1**: Run cleanup more frequently in `/Admin/Security/BannedIps`  
**Solution 2**: Decrease ban duration in Config: `RateLimit.BanDurationMinutes`  
**Solution 3**: Set up automated cleanup job

### Issue: Bans not persisting after restart
**Solution**: Check database migration was applied:
```bash
dotnet ef migrations list
# Should show: 20260128150000_AddBannedIpAndRateLimitConfig (Applied)
```

### Issue: Banned myself on localhost during development
**Problem**: The rate limiting now applies to ALL IPs including localhost (127.0.0.1 and ::1).  
**Solution 1**: Unban your IP at `/Admin/Security/BannedIps`  
**Solution 2**: Temporarily disable rate limiting at `/Admin/Plugin/Index`  
**Solution 3**: Clear the ban from the database:
```sql
DELETE FROM BannedIp WHERE IpAddress = '::1' OR IpAddress = '127.0.0.1';
```

**Note**: Unlike earlier versions, localhost IPs are NO LONGER bypassed. This allows you to test ban functionality on your development machine. Use the Plugin disable feature if you need to work without rate limiting during development.

### Issue: Performance impact from rate limiting

Rate limiting is designed to have minimal performance impact. However, to ensure optimal performance:

- **Use efficient data queries**: Rate limiting stores data in the `Config` table. Ensure your database is indexed and queries are optimized.

- **Adjust rates as necessary**: If you have legitimate traffic spikes, consider adjusting your rate limits to accommodate valid users while still protecting against abuse.

- **Monitor performance**: Keep an eye on your application's performance metrics. If you notice degradation, review your rate limiting settings and adjust as needed.

---

## ?? Where to Get Help

1. **Documentation**: `digioz.Portal.Web/Documentation/RateLimitingReadMe.md`
2. **Database Schema**: `digioz.Portal.Web/Documentation/RateLimiting_DATABASE_SCHEMA.sql`
3. **Admin Panel**: `/Admin/Security/BannedIps`
4. **Logs**: `/Admin/Logs` (filter by Warning/Error)

---

## ?? What's Protected

? **Global Protection**
- All pages and endpoints
- Automatic bot detection
- Rate limiting per IP

? **Password Reset Specific**
- Email enumeration prevention
- Per-email rate limiting
- Attack logging

? **Admin Management**
- View all bans
- Manual ban/unban
- Statistics dashboard

---

## ?? Alert Thresholds

Consider setting up alerts for:

| Event | Threshold | Action |
|-------|-----------|--------|
| Bans per hour | > 10 | Investigate attack |
| Permanent bans | Any | Review immediately |
| Password reset attempts (non-existent) | > 50/hour | Consider CAPTCHA |
| Rate limit hits | > 100/hour | Review limits |

---

## ?? Quick SQL Queries

### Check if IP is banned
```sql
SELECT * FROM BannedIp 
WHERE IpAddress = '192.168.1.1' 
  AND BanExpiry > GETUTCDATE()
ORDER BY CreatedDate DESC;
```

### Get all active bans
```sql
SELECT IpAddress, Reason, BanExpiry, BanCount 
FROM BannedIp 
WHERE BanExpiry > GETUTCDATE()
ORDER BY CreatedDate DESC;
```

### Manually unban IP
```sql
DELETE FROM BannedIp WHERE IpAddress = '192.168.1.1';
```

### Clean expired bans
```sql
DELETE FROM BannedIp 
WHERE BanExpiry < GETUTCDATE() 
  AND BanExpiry != CAST('9999-12-31' AS DATETIME2);
```

---

## ? Future Enhancements to Consider

1. **CAPTCHA Integration**: Add reCAPTCHA v3 for suspicious requests
2. **IP Whitelist**: Never ban certain IPs (monitoring services, etc.)
3. **GeoIP Blocking**: Block entire countries if needed
4. **Email Alerts**: Notify admins of attacks via email/Slack
5. **Metrics Dashboard**: Graphs and charts of attack patterns
6. **Export Bans**: Share ban lists between environments

---

**Last Updated**: 2026-01-28  
**Version**: 1.0
