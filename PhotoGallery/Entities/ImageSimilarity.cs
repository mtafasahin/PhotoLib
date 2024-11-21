using PhotoGallery.Entities;

public class ImageSimilarity
{
    public int Id { get; set; }

    // Ana resim
    public int ImageId { get; set; }
    public Image Image { get; set; } = null!; // Navigation property

    // Benzer resim
    public int SimilarImageId { get; set; }
    public Image SimilarImage { get; set; } = null!; // Navigation property

    // Benzerlik skoru
    public int SimilarityScore { get; set; }
}
