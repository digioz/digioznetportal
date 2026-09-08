-- Verification Script for Rate Limiting Database Configuration
-- Run this after applying the migration to verify everything is set up correctly

PRINT '========================================';
PRINT 'Rate Limiting Configuration Verification';
PRINT '========================================';
PRINT '';

-- 1. Check BannedIp Table
PRINT '1. Checking BannedIp Table...';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BannedIp')
BEGIN
    PRINT '   ? BannedIp table exists';
    
    -- Check indexes
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BannedIp_IpAddress')
        PRINT '   ? IX_BannedIp_IpAddress index exists';
    ELSE
        PRINT '   ? IX_BannedIp_IpAddress index MISSING';
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BannedIp_BanExpiry')
        PRINT '   ? IX_BannedIp_BanExpiry index exists';
    ELSE
        PRINT '   ? IX_BannedIp_BanExpiry index MISSING';
    
    -- Show record count
    DECLARE @BannedCount INT;
    SELECT @BannedCount = COUNT(*) FROM BannedIp;
    PRINT '   ? Current banned IPs: ' + CAST(@BannedCount AS VARCHAR);
END
ELSE
BEGIN
    PRINT '   ? BannedIp table MISSING - Migration not applied!';
END
PRINT '';

-- 2. Check Config Entries
PRINT '2. Checking Config Table Entries...';
DECLARE @ConfigCount INT = 0;

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.MaxRequestsPerMinute')
BEGIN
    DECLARE @MaxReqPerMin VARCHAR(100);
    SELECT @MaxReqPerMin = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.MaxRequestsPerMinute';
    PRINT '   ? RateLimit.MaxRequestsPerMinute = ' + @MaxReqPerMin;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.MaxRequestsPerMinute MISSING';

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.MaxRequestsPer10Minutes')
BEGIN
    DECLARE @MaxReqPer10Min VARCHAR(100);
    SELECT @MaxReqPer10Min = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.MaxRequestsPer10Minutes';
    PRINT '   ? RateLimit.MaxRequestsPer10Minutes = ' + @MaxReqPer10Min;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.MaxRequestsPer10Minutes MISSING';

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.BanDurationMinutes')
BEGIN
    DECLARE @BanDuration VARCHAR(100);
    SELECT @BanDuration = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.BanDurationMinutes';
    PRINT '   ? RateLimit.BanDurationMinutes = ' + @BanDuration;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.BanDurationMinutes MISSING';

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.PermanentBanThreshold')
BEGIN
    DECLARE @PermanentThreshold VARCHAR(100);
    SELECT @PermanentThreshold = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.PermanentBanThreshold';
    PRINT '   ? RateLimit.PermanentBanThreshold = ' + @PermanentThreshold;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.PermanentBanThreshold MISSING';

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerIpPerHour')
BEGIN
    DECLARE @PwdResetIp VARCHAR(100);
    SELECT @PwdResetIp = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerIpPerHour';
    PRINT '   ? RateLimit.PasswordReset.MaxAttemptsPerIpPerHour = ' + @PwdResetIp;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.PasswordReset.MaxAttemptsPerIpPerHour MISSING';

IF EXISTS (SELECT * FROM Config WHERE ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour')
BEGIN
    DECLARE @PwdResetEmail VARCHAR(100);
    SELECT @PwdResetEmail = ConfigValue FROM Config WHERE ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour';
    PRINT '   ? RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour = ' + @PwdResetEmail;
    SET @ConfigCount = @ConfigCount + 1;
END
ELSE
    PRINT '   ? RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour MISSING';

PRINT '   ? Total config entries found: ' + CAST(@ConfigCount AS VARCHAR) + ' of 6';
PRINT '';

-- 3. Check Plugin Entry
PRINT '3. Checking Plugin Table Entry...';
IF EXISTS (SELECT * FROM Plugin WHERE Name = 'Rate Limiting & Bot Protection')
BEGIN
    DECLARE @IsEnabled BIT;
    SELECT @IsEnabled = IsEnabled FROM Plugin WHERE Name = 'Rate Limiting & Bot Protection';
    PRINT '   ? Rate Limiting & Bot Protection plugin exists';
    IF @IsEnabled = 1
        PRINT '   ? Plugin is ENABLED';
    ELSE
        PRINT '   ? Plugin is DISABLED';
END
ELSE
BEGIN
    PRINT '   ? Rate Limiting & Bot Protection plugin MISSING';
END
PRINT '';

-- 4. Summary
PRINT '========================================';
PRINT 'Summary';
PRINT '========================================';

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BannedIp')
   AND @ConfigCount = 6
   AND EXISTS (SELECT * FROM Plugin WHERE Name = 'Rate Limiting & Bot Protection')
BEGIN
    PRINT '? ALL CHECKS PASSED';
    PRINT '';
    PRINT 'Rate Limiting is ready to use!';
    PRINT '';
    PRINT 'Next Steps:';
    PRINT '  1. Navigate to /Admin/Security/BannedIps to view banned IPs';
    PRINT '  2. Navigate to /Admin/Config/Index to adjust settings';
    PRINT '  3. Navigate to /Admin/Plugin/Index to enable/disable feature';
    
    IF EXISTS (SELECT * FROM Plugin WHERE Name = 'Rate Limiting & Bot Protection' AND IsEnabled = 1)
        PRINT '  4. ? Rate limiting is currently ACTIVE';
    ELSE
        PRINT '  4. ? Rate limiting is currently DISABLED';
END
ELSE
BEGIN
    PRINT '? SOME CHECKS FAILED';
    PRINT '';
    PRINT 'Issues found:';
    
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BannedIp')
        PRINT '  - BannedIp table is missing';
    
    IF @ConfigCount < 6
        PRINT '  - Missing ' + CAST(6 - @ConfigCount AS VARCHAR) + ' config entries';
    
    IF NOT EXISTS (SELECT * FROM Plugin WHERE Name = 'Rate Limiting & Bot Protection')
        PRINT '  - Plugin entry is missing';
    
    PRINT '';
    PRINT 'Action Required:';
    PRINT '  Run: dotnet ef database update';
END

PRINT '';
PRINT '========================================';

-- 5. Show All Rate Limit Configs
PRINT '';
PRINT 'Current Rate Limiting Configuration:';
PRINT '========================================';

SELECT 
    ConfigKey,
    ConfigValue,
    CASE 
        WHEN ConfigKey = 'RateLimit.MaxRequestsPerMinute' THEN 'Per-minute request limit'
        WHEN ConfigKey = 'RateLimit.MaxRequestsPer10Minutes' THEN '10-minute request limit'
        WHEN ConfigKey = 'RateLimit.BanDurationMinutes' THEN 'Temporary ban duration (minutes)'
        WHEN ConfigKey = 'RateLimit.PermanentBanThreshold' THEN 'Bans before permanent'
        WHEN ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerIpPerHour' THEN 'Password reset IP limit per hour'
        WHEN ConfigKey = 'RateLimit.PasswordReset.MaxAttemptsPerEmailPerHour' THEN 'Password reset email limit per hour'
        ELSE 'Unknown'
    END AS Description
FROM Config
WHERE ConfigKey LIKE 'RateLimit.%'
ORDER BY ConfigKey;
