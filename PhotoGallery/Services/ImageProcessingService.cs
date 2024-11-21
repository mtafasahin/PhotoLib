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

        foreach (var image in unprocessedImages)
        {
            try
            {
                if (Path.GetExtension(image.Url).ToLower() == ".heic")
                {
                    ProcessHeicImage(image);
                }
                else
                {
                    ProcessStandardImage(image);
                }

                image.IsProcessed = true;
                _context.Images.Update(image);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image {image.Url}: {ex.Message}");
            }
        }

        _context.SaveChanges();
    }

    private void ProcessStandardImage(Image image)
    {
        var directories = ImageMetadataReader.ReadMetadata(image.Url);

        var exifSubIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

        try {
            var parsedDate = exifSubIfd?.GetDateTime(ExifSubIfdDirectory.TagDateTimeOriginal);
            //image.TakenDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }catch(Exception ex) {
            Console.WriteLine($"Error processing ExifSubIfdDirectory.TagDateTimeOriginal {image.Url}: {ex.Message}");
        }
       
        
        image.Brand = exifIfd0?.GetDescription(ExifIfd0Directory.TagMake);
        image.Model = exifIfd0?.GetDescription(ExifIfd0Directory.TagModel);
    }

    private void ProcessHeicImage(Image image)
    {
        using (var magickImage = new MagickImage(image.Url))
        {
            var exifProfile = magickImage.GetExifProfile();

            if (exifProfile != null)
            {
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
        }
    }
}
