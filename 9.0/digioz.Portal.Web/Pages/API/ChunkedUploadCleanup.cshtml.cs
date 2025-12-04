using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Pages.API
{
    [IgnoreAntiforgeryToken]
    public class ChunkedUploadCleanupModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ChunkedUploadCleanupModel> _logger;

        public ChunkedUploadCleanupModel(
            IWebHostEnvironment env,
            ILogger<ChunkedUploadCleanupModel> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Clean up uploaded chunks (called on error or cancellation)
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                string uploadId;
                
                // Read from request body (JSON)
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(body);
                    uploadId = data.GetProperty("uploadId").GetString() ?? "";
                }

                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest(new { error = "Upload ID is required" });
                }

                var webroot = _env.WebRootPath;
                var uploadDir = Path.Combine(webroot, "img", "uploads", uploadId);

                if (Directory.Exists(uploadDir))
                {
                    Directory.Delete(uploadDir, true);
                    _logger.LogInformation($"Cleaned up upload directory: {uploadId}");
                }

                return new JsonResult(new
                {
                    success = true,
                    uploadId = uploadId,
                    message = "Chunks cleaned up successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up chunks");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
