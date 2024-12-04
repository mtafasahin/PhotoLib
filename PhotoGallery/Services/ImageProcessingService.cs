using System.Text.Json;
using ImageMagick;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using PhotoGallery.Entities;

public class ImageProcessingService
{
    private readonly ApplicationDbContext _context;

    public ImageProcessingService(ApplicationDbContext context)
    {
        _context = context;
    }

    string AddPostfixToFileName(string originalPath, string postfix)
    {
        // Dosya adını ve uzantısını ayır
        string directory = Path.GetDirectoryName(originalPath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
        string extension = Path.GetExtension(originalPath);

        // Yeni dosya adı oluştur
        string newFileName = $"{fileNameWithoutExtension}{postfix}{extension}";

        // Yeni dosya yolu oluştur
        return Path.Combine(directory, newFileName);
    }

    private string CreateThumbnail(string url, uint width, uint height) 
    {
        var outputPath = Path.ChangeExtension(AddPostfixToFileName(url, "-th"),".jpg");
        try
        {                        
            using (var image = new MagickImage(url))
            {
                // Oranları koruyarak boyutlandır
                image.Resize(width, height);
                // Kaliteyi düşür (isteğe bağlı)
                image.Quality = 75;
                image.Strip(); // Gereksiz metadata'yı kaldır
                image.Format = MagickFormat.Jpg;
                
                // Thumbnail'i kaydet
                image.Write(outputPath);
            }
            Console.WriteLine($"Thumbnail oluşturuldu ve {outputPath} konumuna kaydedildi.");
            // Resmi yükle           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thumbnail oluşturulurken bir hata oluştu: {ex.Message}");
            return string.Empty;
        }
        return outputPath;
    }

    public void CreateThumbnails(int startId, int endId) {
        
        var unprocessedImages = _context.Images
            .Where(i => !i.IsDeleted)
            .Where(i => string.IsNullOrEmpty(i.ThumbUrl))
            .Where(i => i.Id >= startId && i.Id <= endId)
            .ToList();
        
        foreach (var unprocessedImage in unprocessedImages)
        {
            unprocessedImage.ThumbUrl =  CreateThumbnail(unprocessedImage.Url, 600, 400);
            if(!string.IsNullOrEmpty(unprocessedImage.ThumbUrl)) 
            {
                _context.Update(unprocessedImage);
                _context.SaveChanges();
            }
        }
        // Resmi yükle        
    }

    public void ProcessImages(int startId, int endId)
    {
        var unprocessedImages = _context.Images
            .Where(i => !i.IsProcessed)
            .Where(i => i.Id >= startId && i.Id <= endId)
            .ToList();

        int batchCounter = 0;
        const int batchSize = 1; // 10 kayıtta bir kaydet
        foreach (var image in unprocessedImages)
        {
            batchCounter = ProcessImage(batchCounter, image);

            if (batchCounter >= batchSize)
            {
                _context.SaveChanges();
                batchCounter = 0;
            }
        }

        // Kalan kayıtları kaydet
        if (batchCounter > 0)
        {
            _context.SaveChanges();
        }
       
    }

    private int ProcessImage(int batchCounter, Image? image)
    {
        try
        {
            ProcessImageMetadata(image);
            image.ThumbUrl = CreateThumbnail(image.Url, 600, 400);            
            image.IsProcessed = true;
            image.HashValue = GenerateImageHash(image.Url);
            _context.Images.Update(image);
            batchCounter++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing image {image.Url}: {ex.Message}");
            image.Brand = "Error";
        }

        return batchCounter;
    }

    private void ProcessStandardImage(Image image)
    {
        var metadataDict = new Dictionary<string, object>();
        var directories = ImageMetadataReader.ReadMetadata(image.Url);
        var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        if(exifSubIfd != null) 
        {
            foreach (var tag in exifSubIfd.Tags)
            {
                metadataDict[$"SubIfd_{tag.Name}"] = exifSubIfd.GetDescription(tag.Type);
            }
        }

        var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        if(exifIfd0 != null) 
        {
            foreach (var tag in exifIfd0.Tags)
            {
                metadataDict[$"Ifd0_{tag.Name}"] = exifIfd0.GetDescription(tag.Type);
            }
        }

        image.MetadataJson = JsonSerializer.Serialize(metadataDict);

        try {
            var parsedDate = exifSubIfd?.GetDateTime(ExifSubIfdDirectory.TagDateTimeOriginal);
            //image.TakenDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }catch(Exception ex) {
            Console.WriteLine($"Error processing ExifSubIfdDirectory.TagDateTimeOriginal {image.Url}: {ex.Message}");
        }
       
        
        image.Brand = exifIfd0?.GetDescription(ExifIfd0Directory.TagMake);
        image.Model = exifIfd0?.GetDescription(ExifIfd0Directory.TagModel);

        // Get resolution
        using (var magickImage = new MagickImage(image.Url))
        {
            image.Resolution = $"{magickImage.Width}x{magickImage.Height}";
        }
    }

    private string GenerateImageHash(string imagePath)
    {
        using (var magickImage = new MagickImage(imagePath))
        {
            return magickImage.PerceptualHash().ToString();
        }
    }


        private void ProcessImageMetadata(Image image)
        {
            using (var magickImage = new MagickImage(image.Url))
            {
                var metadataDict = new Dictionary<string, string>();

                // ExifProfile'dan metadata bilgilerini al
                var exifProfile = magickImage.GetExifProfile();
                if (exifProfile != null)
                {
                    foreach (var value in exifProfile.Values)
                    {
                        metadataDict[value.Tag.ToString()] = value.ToString();
                    }

                    // Örnek: DateTimeOriginal işle
                    var dateTaken = exifProfile.GetValue(ExifTag.DateTimeOriginal)?.Value;
                    if (!string.IsNullOrEmpty(dateTaken))
                    {
                        try
                        {
                            var parsedDate = DateTime.ParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null);
                            image.TakenDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing date: {ex.Message}");
                        }
                    }
                }

                image.Brand = exifProfile?.GetValue(ExifTag.Make)?.Value;
                image.Model = exifProfile?.GetValue(ExifTag.Model)?.Value;

                // Çözünürlük bilgisi ekle
                image.Resolution = $"{magickImage.Width}x{magickImage.Height}";
                metadataDict["Resolution"] = image.Resolution;

                // Hash değeri ekle
                image.HashValue = GenerateImageHash(image.Url);
                metadataDict["HashValue"] = image.HashValue;

                // Tüm bilgileri JSON olarak sakla
                image.MetadataJson = JsonSerializer.Serialize(metadataDict);
            }
        }


    private void ProcessHeicImage(Image image)
    {
        var metadataDict = new Dictionary<string, object>();
        using (var magickImage = new MagickImage(image.Url))
        {
            var exifProfile = magickImage.GetExifProfile();

            if (exifProfile != null)
            {
                foreach (var value in exifProfile.Values)
                {
                    metadataDict[value.Tag.ToString()] = value?.ToString();
                }
                image.MetadataJson = JsonSerializer.Serialize(metadataDict);

                var dateTaken = exifProfile.GetValue(ExifTag.DateTimeOriginal)?.Value;

                if (!string.IsNullOrEmpty(dateTaken))
                {
                    try
                    {
                        var parsedDate = DateTime.ParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null);
                        image.TakenDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Invalid date format for image {image.Url}: {dateTaken}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing date for image {image.Url}: {ex.Message}");
                    }
                }
                image.Brand = exifProfile.GetValue(ExifTag.Make)?.Value;
                image.Model = exifProfile.GetValue(ExifTag.Model)?.Value;
            }
            image.Resolution = $"{magickImage.Width}x{magickImage.Height}";
        }
    }
}
