using System.Text.Json;
using ImageMagick;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
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

    public void ProcessImages()
    {
        var unprocessedImages = _context.Images
            .Where(i => !i.IsProcessed)
            .ToList();

        int batchCounter = 0;
        const int batchSize = 5; // 10 kayıtta bir kaydet
        foreach (var image in unprocessedImages)
        {
            try
            {
                // if (Path.GetExtension(image.Url).ToLower() == ".heic")
                // {
                //     ProcessHeicImage(image);
                // }
                // else
                // {
                //     ProcessStandardImage(image);
                // }
                ProcessImageMetadata(image);

                image.IsProcessed = true;
                image.HashValue = GenerateImageHash(image.Url);
                _context.Images.Update(image);
                batchCounter++;

                if (batchCounter >= batchSize)
                {
                    _context.SaveChanges();
                    batchCounter = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {image.Url}: {ex.Message}");
            }
        }


        // Kalan kayıtları kaydet
        if (batchCounter > 0)
        {
            _context.SaveChanges();
        }
       
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

                image.Brand = exifProfile.GetValue(ExifTag.Make)?.Value;
                image.Model = exifProfile.GetValue(ExifTag.Model)?.Value;

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
