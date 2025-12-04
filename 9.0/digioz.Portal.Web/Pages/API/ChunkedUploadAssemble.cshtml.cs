using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Pages.API
{
    [IgnoreAntiforgeryToken]
    public class ChunkedUploadAssembleModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ChunkedUploadAssembleModel> _logger;

        public ChunkedUploadAssembleModel(
            IWebHostEnvironment env,
            ILogger<ChunkedUploadAssembleModel> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Assemble all chunks into the final file
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var uploadId = Request.Form["uploadId"].ToString();
                var fileName = Request.Form["fileName"].ToString();
                var totalChunks = int.Parse(Request.Form["totalChunks"]);
                var fileSize = long.Parse(Request.Form["fileSize"]);

                if (string.IsNullOrEmpty(uploadId) || string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { error = "Upload ID and file name are required" });
                }

                var webroot = _env.WebRootPath;
                var uploadDir = Path.Combine(webroot, "img", "uploads", uploadId);

                if (!Directory.Exists(uploadDir))
                {
                    return BadRequest(new { error = "Upload directory not found" });
                }

                // Verify all chunks are present
                var chunkFiles = Directory.GetFiles(uploadDir, "chunk_*")
                    .OrderBy(f => f)
                    .ToList();

                if (chunkFiles.Count != totalChunks)
                {
                    return BadRequest(new
                    {
                        error = $"Missing chunks. Expected {totalChunks}, found {chunkFiles.Count}"
                    });
                }

                // Generate unique filename for assembled file
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = Guid.NewGuid().ToString("N") + fileExtension;
                var assembledPath = Path.Combine(uploadDir, uniqueFileName);

                _logger.LogInformation($"Assembling {chunkFiles.Count} chunks into {uniqueFileName}");

                // Assemble chunks into final file
                using (var outputStream = System.IO.File.Create(assembledPath))
                {
                    foreach (var chunkFile in chunkFiles)
                    {
                        using (var inputStream = System.IO.File.OpenRead(chunkFile))
                        {
                            await inputStream.CopyToAsync(outputStream);
                        }
                    }
                }

                // Verify file size
                var assembledFileInfo = new FileInfo(assembledPath);
                if (assembledFileInfo.Length != fileSize)
                {
                    _logger.LogWarning($"File size mismatch. Expected {fileSize}, got {assembledFileInfo.Length}");
                }

                // Clean up chunk files
                foreach (var chunkFile in chunkFiles)
                {
                    try
                    {
                        System.IO.File.Delete(chunkFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to delete chunk file: {chunkFile}");
                    }
                }

                _logger.LogInformation($"File assembled successfully: {uniqueFileName}");

                return new JsonResult(new
                {
                    success = true,
                    uploadId = uploadId,
                    fileName = uniqueFileName,
                    originalFileName = fileName,
                    size = assembledFileInfo.Length,
                    relativePath = $"uploads/{uploadId}/{uniqueFileName}",
                    message = "File assembled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assembling chunks");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
