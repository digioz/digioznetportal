-- Multi-Layer Login Protection Database Schema
-- Created by Migration: 20260128135227_MultiLayerLoginProtection
-- Purpose: Store banned IP addresses for rate limiting and bot protection

-- Table: BannedIp
-- Stores information about IP addresses that have been banned
-- Supports both temporary and permanent bans
CREATE TABLE [dbo].[BannedIp] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [IpAddress] NVARCHAR(64) NOT NULL,
    [BanExpiry] DATETIME2 NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [BanCount] INT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UserAgent] NVARCHAR(500) NULL,
    [AttemptedEmail] NVARCHAR(256) NULL,
    
    CONSTRAINT [PK_BannedIp] PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- Index for fast IP address lookups (used on every request)
CREATE INDEX [IX_BannedIp_IpAddress] 
ON [dbo].[BannedIp]([IpAddress]);

-- Index for cleanup queries (finding expired bans)
CREATE INDEX [IX_BannedIp_BanExpiry] 
ON [dbo].[BannedIp]([BanExpiry]);

-- Sample Queries

-- 1. Check if an IP is currently banned
SELECT TOP 1 * 
FROM BannedIp 
WHERE IpAddress = '192.168.1.1' 
  AND (BanExpiry > GETUTCDATE() OR BanExpiry = CAST('9999-12-31' AS DATETIME2))
ORDER BY CreatedDate DESC;

-- 2. Get all active bans
SELECT * 
FROM BannedIp 
WHERE BanExpiry > GETUTCDATE() OR BanExpiry = CAST('9999-12-31' AS DATETIME2)
ORDER BY CreatedDate DESC;

-- 3. Get all permanent bans
SELECT * 
FROM BannedIp 
WHERE BanExpiry = CAST('9999-12-31' AS DATETIME2)
ORDER BY CreatedDate DESC;

-- 4. Get bans that will expire in the next hour
SELECT * 
FROM BannedIp 
WHERE BanExpiry > GETUTCDATE() 
  AND BanExpiry < DATEADD(HOUR, 1, GETUTCDATE())
ORDER BY BanExpiry ASC;

-- 5. Get top 10 most banned IPs
SELECT TOP 10 
    IpAddress, 
    MAX(BanCount) as MaxBanCount,
    COUNT(*) as TotalBans,
    MAX(CreatedDate) as LastBanned
FROM BannedIp
GROUP BY IpAddress
ORDER BY MaxBanCount DESC, TotalBans DESC;

-- 6. Clean up expired bans (run periodically)
DELETE FROM BannedIp 
WHERE BanExpiry < GETUTCDATE() 
  AND BanExpiry != CAST('9999-12-31' AS DATETIME2);

-- 7. Get ban statistics
SELECT 
    COUNT(*) as TotalBans,
    SUM(CASE WHEN BanExpiry > GETUTCDATE() OR BanExpiry = CAST('9999-12-31' AS DATETIME2) THEN 1 ELSE 0 END) as ActiveBans,
    SUM(CASE WHEN BanExpiry = CAST('9999-12-31' AS DATETIME2) THEN 1 ELSE 0 END) as PermanentBans,
    SUM(CASE WHEN BanExpiry < GETUTCDATE() THEN 1 ELSE 0 END) as ExpiredBans,
    AVG(BanCount) as AverageBanCount
FROM BannedIp;

-- 8. Get recent ban activity (last 24 hours)
SELECT 
    IpAddress,
    Reason,
    BanCount,
    CreatedDate,
    CASE 
        WHEN BanExpiry = CAST('9999-12-31' AS DATETIME2) THEN 'Permanent'
        WHEN BanExpiry > GETUTCDATE() THEN 'Active'
        ELSE 'Expired'
    END as Status,
    UserAgent
FROM BannedIp
WHERE CreatedDate > DATEADD(DAY, -1, GETUTCDATE())
ORDER BY CreatedDate DESC;

-- 9. Get email enumeration attempts
SELECT 
    AttemptedEmail,
    IpAddress,
    COUNT(*) as AttemptCount,
    MAX(CreatedDate) as LastAttempt
FROM BannedIp
WHERE AttemptedEmail IS NOT NULL
GROUP BY AttemptedEmail, IpAddress
ORDER BY AttemptCount DESC;

-- 10. Manually ban an IP address
INSERT INTO BannedIp (IpAddress, BanExpiry, Reason, BanCount, CreatedDate)
VALUES ('192.168.1.100', DATEADD(HOUR, 24, GETUTCDATE()), 'Manual ban by admin', 1, GETUTCDATE());

-- 11. Manually unban an IP address (remove all bans)
DELETE FROM BannedIp WHERE IpAddress = '192.168.1.100';

-- 12. Extend a ban duration
UPDATE BannedIp 
SET BanExpiry = DATEADD(DAY, 7, GETUTCDATE())
WHERE IpAddress = '192.168.1.100'
  AND BanExpiry > GETUTCDATE();

-- 13. Make a ban permanent
UPDATE BannedIp 
SET BanExpiry = CAST('9999-12-31' AS DATETIME2)
WHERE IpAddress = '192.168.1.100';

-- Column Descriptions:
-- Id: Auto-incrementing primary key
-- IpAddress: The banned IP address (max 64 chars for IPv6)
-- BanExpiry: When the ban expires (DateTime.MaxValue = permanent)
-- Reason: Why the IP was banned
-- BanCount: How many times this IP has been banned (for escalation)
-- CreatedDate: When this ban record was created
-- UserAgent: Optional user agent string from the banned request
-- AttemptedEmail: Optional email that was being targeted (for enumeration tracking)

-- Performance Notes:
-- 1. IX_BannedIp_IpAddress index makes IP lookups very fast (< 1ms)
-- 2. IX_BannedIp_BanExpiry index speeds up cleanup queries
-- 3. Both indexes are non-clustered for optimal insert performance
-- 4. Table should be cleaned periodically to remove expired bans
-- 5. Expected size: ~200 bytes per record, 10,000 records = ~2 MB

-- Maintenance Schedule:
-- Daily: Run cleanup query (#6) to remove expired bans
-- Weekly: Analyze ban statistics (#7) for security monitoring
-- Monthly: Review top banned IPs (#5) and consider permanent bans
