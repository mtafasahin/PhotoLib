using System;

namespace PhotoGallery.Entities;

public class Image
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string? Brand { get; set; } // Kamera markası
    public string? Model { get; set; } // Kamera modeli
    public DateTime? TakenDate { get; set; } // Çekim tarihi
    public string? Resolution { get; set; } // Örn: "1920x1080"
    public string? HashValue { get; set; } // Resim hash değeri
    public bool IsProcessed { get; set; } = false;

    public string? MetadataJson { get; set; } // Tüm metadata bilgilerini JSON formatında saklar

    // Navigation Properties
    public ICollection<ImageLabel> ImageLabels { get; set; }
}
