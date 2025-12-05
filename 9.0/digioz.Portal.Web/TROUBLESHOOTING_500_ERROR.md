# 500 Error Troubleshooting Guide

## Where to Find Logs

### 1. ASP.NET Core stdout Logs

**Location on Server:**
```
C:\inetpub\wwwroot\YourAppName\logs\stdout_*.log
```

Or wherever your application is deployed:
```
[Application Root]\logs\stdout_*.log
```

**File Naming Pattern:**
```
stdout_20241204_153045_12345.log
```

**If logs folder doesn't exist:**
- The application may not have permission to create it
- Create the folder manually and give IIS_IUSRS full control

**PowerShell to check:**
```powershell
# Find your application root
cd C:\inetpub\wwwroot\YourAppName

# List log files
Get-ChildItem -Path .\logs -Filter "stdout_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 5

# View latest log
Get-Content (Get-ChildItem -Path .\logs -Filter "stdout_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
```

### 2. Windows Event Viewer

**Application Logs:**
1. Open Event Viewer (eventvwr.msc)
2. Windows Logs ? Application
3. Filter by Source:
   - "IIS AspNetCore Module V2"
   - "ASP.NET Core"
   - ".NET Runtime"

**System Logs:**
1. Windows Logs ? System
2. Look for IIS-related errors

### 3. IIS Failed Request Tracing

If you still can't find the error, enable Failed Request Tracing:

**Enable in IIS Manager:**
1. Select your site
2. Failed Request Tracing Rules
3. Add Rule ? All Content (*)
4. Status Code: 500-599
5. Finish

**View traces:**
```
C:\inetpub\logs\FailedReqLogFiles\W3SVC[SiteID]\
```

### 4. Enable Developer Exception Page

**Temporarily in web.config:**
```xml
<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
```

?? This will show detailed errors in the browser, but **REMOVE AFTER TROUBLESHOOTING** for security.

## Common 500 Error Causes with web.config

### 1. Invalid XML Syntax
**Check for:**
- Missing closing tags
- Duplicate sections
- Invalid attributes

**Validate XML:**
```powershell
[xml]$xml = Get-Content "C:\path\to\web.config"
# If this errors, XML is invalid
```

### 2. Invalid Configuration Values

**Check uploadReadAheadSize:**
```xml
<!-- Maximum value is 2GB (2147483647) -->
<serverRuntime uploadReadAheadSize="2147483647" />
```

If still errors, try removing it:
```xml
<!-- Remove this line temporarily -->
<!-- <serverRuntime uploadReadAheadSize="2147483647" /> -->
```

### 3. IIS Module Not Installed

**Check ASP.NET Core Module is installed:**
```powershell
Get-WindowsFeature -Name Web-Server, Web-Asp-Net45, Web-ISAPI-Ext, Web-ISAPI-Filter
```

**Install if missing:**
- Download: ASP.NET Core Hosting Bundle
- URL: https://dotnet.microsoft.com/download/dotnet/9.0
- Select "Hosting Bundle" for Windows

### 4. Application Pool Issues

**Check Application Pool:**
1. IIS Manager ? Application Pools
2. Find your pool
3. Right-click ? Advanced Settings
4. **.NET CLR Version: No Managed Code** (must be this for .NET Core)
5. **Managed Pipeline Mode: Integrated**

**Recycle the pool:**
```powershell
Restart-WebAppPool -Name "YourAppPoolName"
```

### 5. Permission Issues

**Check folder permissions:**
```powershell
$path = "C:\inetpub\wwwroot\YourAppName"

# Check current permissions
Get-Acl $path | Format-List

# Grant IIS_IUSRS full control
$acl = Get-Acl $path
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS","FullControl","ContainerInherit,ObjectInherit","None","Allow")
$acl.SetAccessRule($rule)
Set-Acl $path $acl
```

## Step-by-Step Troubleshooting

### Step 1: Minimal web.config

Replace your web.config with this minimal version:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\digioz.Portal.Web.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

If this works, add settings back one at a time:
1. Add `requestTimeout`
2. Add `maxAllowedContentLength`
3. Add `uploadReadAheadSize`

### Step 2: Check Application Files

**Verify DLL exists:**
```powershell
Test-Path "C:\inetpub\wwwroot\YourAppName\digioz.Portal.Web.dll"
```

**Check dependencies:**
```powershell
Get-ChildItem "C:\inetpub\wwwroot\YourAppName\*.dll" | Select-Object Name, Length, LastWriteTime
```

### Step 3: Check appsettings.json

**Verify connection string:**
- SQL Server is accessible from web server
- Credentials are correct
- Database exists

**Test SQL connection from server:**
```powershell
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=localhost;Database=digiozPortal9;Trusted_Connection=True;"
try {
    $conn.Open()
    Write-Host "Connection successful" -ForegroundColor Green
    $conn.Close()
} catch {
    Write-Host "Connection failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Step 4: Check .NET Runtime

**Verify .NET 9 is installed:**
```powershell
dotnet --list-runtimes
```

Should show:
```
Microsoft.AspNetCore.App 9.0.x
Microsoft.NETCore.App 9.0.x
```

**If missing, install:**
- ASP.NET Core 9.0 Runtime (Hosting Bundle)
- Download from: https://dotnet.microsoft.com/download/dotnet/9.0

## Likely Issue: uploadReadAheadSize

The `uploadReadAheadSize` setting can cause issues on some IIS configurations. Try this version without it:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      
      <aspNetCore processPath="dotnet" 
                  arguments=".\digioz.Portal.Web.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess"
                  requestTimeout="00:20:00">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
      
      <security>
        <requestFiltering>
          <requestLimits maxAllowedContentLength="2147483648" />
        </requestFiltering>
      </security>
    </system.webServer>
  </location>
</configuration>
```

## Getting Help

If none of this works, provide these details:

1. **Exact log content** from `.\logs\stdout_*.log`
2. **Event Viewer errors** (screenshot or text)
3. **IIS version**: Run `iisver.vbs` on server
4. **Application Pool settings** (screenshot)
5. **.NET runtimes installed**: Output of `dotnet --list-runtimes`
6. **Web.config being used** (sanitized, remove sensitive data)

## Quick Fixes to Try

### Fix 1: Remove uploadReadAheadSize
This setting can cause issues. Remove it from web.config.

### Fix 2: Restore Original web.config
If you had a working web.config before, restore it and only add:
```xml
<requestLimits maxAllowedContentLength="2147483648" />
```

### Fix 3: Use OutOfProcess Hosting
Change to out-of-process hosting model:
```xml
<aspNetCore ... hostingModel="outofprocess" />
```

### Fix 4: Check for Locked Configuration
Some settings can't be overridden if locked at server level:
```powershell
C:\Windows\System32\inetsrv\appcmd.exe list config -section:system.webServer/serverRuntime
```

If locked, need to unlock at server level or remove from web.config.

---

**Most Common Solution:**
The `uploadReadAheadSize` setting is likely causing the issue. Remove it and use the "without uploadReadAheadSize" version above.
