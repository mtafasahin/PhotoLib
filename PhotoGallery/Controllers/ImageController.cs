using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Dtos;

namespace PhotoGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("import-images")]
        public IActionResult ImportImages([FromBody] ImageImportRequestDto request)
        {
            if (!Directory.Exists(request.Path))
            {
                return BadRequest("The specified directory does not exist.");
            }

            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".heic" };

            // Dosyaları bulmak için seçilen yöntem
            var files = request.UseNestedFolders
                ? Directory.GetFiles(request.Path, "*.*", SearchOption.AllDirectories)
                : Directory.GetFiles(request.Path, "*.*", SearchOption.TopDirectoryOnly);

            files = files.Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower())).ToArray();

            if (!files.Any())
            {
                return NotFound("No supported image files found in the specified directory.");
            }

            foreach (var file in files)
            {
                if (!_context.Images.Any(i => i.Url == file)) // Check for duplicates
                {
                    _context.Images.Add(new Entities.Image
                    {
                        Url = file,
                        IsProcessed = false
                    });
                }
            }

            _context.SaveChanges();
            return Ok($"{files.Length} image(s) imported successfully.");
        }
    }
}
