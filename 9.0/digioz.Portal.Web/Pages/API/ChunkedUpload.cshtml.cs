using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Pages.API
{
    [IgnoreAntiforgeryToken]
    public class ChunkedUploadModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChunkedUploadModel> _logger;

        public ChunkedUploadModel(
            IWebHostEnvironment env,
            IConfiguration configuration,
            ILogger<ChunkedUploadModel> logger)
        {
            _env = env;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Receive and save a single chunk
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var chunk = Request.Form.Files["chunk"];
                var uploadId = Request.Form["uploadId"].ToString();
                var chunkIndex = int.Parse(Request.Form["chunkIndex"]);
                var totalChunks = int.Parse(Request.Form["totalChunks"]);
                var fileName = Request.Form["fileName"].ToString();

                if (chunk == null || chunk.Length == 0)
                {
                    return BadRequest(new { error = "No chunk data received" });
                }

                if (string.IsNullOrEmpty(uploadId))
                {
                    return BadRequest(new { error = "Upload ID is required" });
                }

                // Get upload directory from wwwroot
                var webroot = _env.WebRootPath;
                var uploadDir = Path.Combine(webroot, "img", "uploads", uploadId);
                Directory.CreateDirectory(uploadDir);

                // Save chunk with index in filename
                var chunkPath = Path.Combine(uploadDir, $"chunk_{chunkIndex:D6}");
                using (var stream = System.IO.File.Create(chunkPath))
                {
                    await chunk.CopyToAsync(stream);
                }

                _logger.LogInformation($"Chunk {chunkIndex + 1}/{totalChunks} saved for upload {uploadId}");

                return new JsonResult(new
                {
                    success = true,
                    chunkIndex = chunkIndex,
                    uploadId = uploadId,
                    message = $"Chunk {chunkIndex + 1}/{totalChunks} uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading chunk");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
