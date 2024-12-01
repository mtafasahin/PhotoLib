using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        [HttpGet("{*filePath}")]
        public IActionResult GetImage(string filePath)
        {
            var fullPath = Path.Combine(@"D:/resimler", filePath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileStream = System.IO.File.OpenRead(fullPath);
            return File(fileStream, "image/jpeg");
        }

        [HttpGet("{id}/similar")]
        public IActionResult GetSimilarImages(int id)
        {
            // Resmin benzerliklerini bul
                var similarImages = _context.ImageSimilarities
                    .Where(sim => sim.ImageId == id)
                    .Select(sim => sim.SimilarImageId)
                    .ToList();

                if (!similarImages.Any())
                {
                    return NotFound(new { Message = "No similar images found for the given ID." });
                }

                // Benzer resimleri alın
                var images = _context.Images
                    .Where(img => similarImages.Contains(img.Id))
                    .Select(i => new
                    {
                        i.Id,
                        i.Url,
                        i.TakenDate,
                        i.Brand,
                        i.Model,  
                        SimilarCount = _context.ImageSimilarities.Count(s => s.ImageId == i.Id)
                    })
                    .ToList();

                return Ok(new
                {
                    Data = images,
                    TotalItems = images.Count,
                    PageNumber = 1,
                    PageSize = 100,
                    TotalPages = 1
                });
        }


        [HttpPost("bulk-delete")]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                // Verilen ID'lere sahip resimleri al
                var imagesToDelete = _context.Images.Where(i => ids.Contains(i.Id)).ToList();

                if (!imagesToDelete.Any())
                {
                    return NotFound(new { Message = "No images found for the given IDs." });
                }

                // isDeleted alanını true yap
                foreach (var image in imagesToDelete)
                {
                    image.IsDeleted = true;
                }

                // Veritabanına kaydet
                _context.SaveChanges();

                return Ok(new { Message = "Images deleted successfully.", DeletedCount = imagesToDelete.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting images.", Error = ex.Message });
            }
        }


        // GET: api/Image
        [HttpGet]
        public IActionResult GetImages([FromQuery] int pageSize, [FromQuery] int pageNumber, [FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var query = _context.Images.AsQueryable();
                // Yıl ve Ay Filtresi
                if (year.HasValue)
                {
                    query = query.Where(i => i.TakenDate.HasValue && i.TakenDate.Value.Year == year.Value);
                }

                if (month.HasValue)
                {
                    query = query.Where(i => i.TakenDate.HasValue && i.TakenDate.Value.Month == month.Value);
                }

                query = query.Where(i => !i.IsDeleted);

                // Benzerlik sayısına göre sıralama
                var imagesWithSimilarity = query
                    .Select(i => new
                    {
                        i.Id,
                        i.Url,
                        i.TakenDate,
                        i.Brand,
                        i.Model,
                        SimilarCount = _context.ImageSimilarities.Count(s => s.ImageId == i.Id && !s.SimilarImage.IsDeleted)
                    })
                    
                    .OrderByDescending(i => i.SimilarCount) // Benzerlik sayısına göre sıralama     
                    .OrderByDescending(i => i.TakenDate);            

                // Toplam Kayıt Sayısı
                var totalRecords = imagesWithSimilarity.Count();

                // Sayfalama
                var images = imagesWithSimilarity
                    .Skip((pageNumber - 1) * pageSize) // Skip items from previous pages
                    .Take(pageSize) // Take items for the current page
                    .ToList();

                // Yanıt
                return Ok(new
                {
                    Data = images,
                    TotalItems = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the images.", error = ex.Message });
            }
        }
    }
}
