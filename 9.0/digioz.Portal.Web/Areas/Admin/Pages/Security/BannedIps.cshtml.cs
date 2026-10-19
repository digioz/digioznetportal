using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Web.Data;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Pages.Security
{
    public class BannedIpsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BannedIpsModel> _logger;
        private readonly Services.BanManagementService _banService;

        public BannedIpsModel(
            ApplicationDbContext context, 
            ILogger<BannedIpsModel> logger,
            Services.BanManagementService banService)
        {
            _context = context;
            _logger = logger;
            _banService = banService;
        }

        public List<BannedIp> BannedIps { get; set; } = new();
        public int ActiveBansCount { get; set; }
        public int PermanentBansCount { get; set; }
        public int ExpiredBansCount { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            BannedIps = await _context.BannedIp
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            ActiveBansCount = BannedIps.Count(b => b.IsActive);
            PermanentBansCount = BannedIps.Count(b => b.IsPermanent);
            ExpiredBansCount = BannedIps.Count(b => !b.IsActive);
        }

        public async Task<IActionResult> OnPostUnbanAsync(int id)
        {
            try
            {
                var bannedIp = await _context.BannedIp.FindAsync(id);
                if (bannedIp == null)
                {
                    StatusMessage = "Error: Banned IP not found.";
                    return RedirectToPage();
                }

                // Use BanManagementService to remove from both cache and database
                await _banService.UnbanIpAsync(bannedIp.IpAddress);

                _logger.LogInformation("Manually unbanned IP: {IpAddress} by admin", bannedIp.IpAddress);
                StatusMessage = $"Successfully unbanned IP: {bannedIp.IpAddress}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning IP with ID: {Id}", id);
                StatusMessage = "Error: Failed to unban IP.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCleanupExpiredAsync()
        {
            try
            {
                var expiredBans = await _context.BannedIp
                    .Where(b => b.BanExpiry < DateTime.UtcNow && b.BanExpiry != DateTime.MaxValue)
                    .ToListAsync();

                if (expiredBans.Any())
                {
                    _context.BannedIp.RemoveRange(expiredBans);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} expired bans", expiredBans.Count);
                    StatusMessage = $"Successfully removed {expiredBans.Count} expired bans.";
                }
                else
                {
                    StatusMessage = "No expired bans found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired bans");
                StatusMessage = "Error: Failed to cleanup expired bans.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBanIpAsync(string ipAddress, string reason, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                StatusMessage = "Error: IP address is required.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "Manually banned by administrator";
            }

            try
            {
                var banExpiry = durationMinutes == -1 
                    ? DateTime.MaxValue // Permanent ban
                    : DateTime.UtcNow.AddMinutes(durationMinutes);

                // Use BanManagementService to add to both cache and database
                await _banService.BanIpAsync(
                    ipAddress,
                    reason,
                    banExpiry,
                    banCount: 1,
                    userAgent: string.Empty,
                    attemptedEmail: string.Empty);

                _logger.LogWarning("Manually banned IP: {IpAddress} by admin. Reason: {Reason}", ipAddress, reason);
                StatusMessage = $"Successfully banned IP: {ipAddress}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning IP: {IpAddress}", ipAddress);
                StatusMessage = "Error: Failed to ban IP.";
            }

            return RedirectToPage();
        }
    }
}
