using System;

namespace PhotoGallery.Dtos;

public class ImageImportRequestDto
{
    public string Path { get; set; } = null!; // Path zorunlu
    public bool UseNestedFolders { get; set; } = false; // Varsayılan false
}
