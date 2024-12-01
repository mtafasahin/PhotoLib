public class ImageSimilarityService
{
    private readonly ApplicationDbContext _context;

    public ImageSimilarityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void CalculateSimilarities()
    {
        var images = _context.Images.Where(i => i.HashValue != null).ToList();

        foreach (var image in images)
        {
            var otherImages = images.Where(i => i.Brand == image.Brand || i.Brand == "Error" || i.Brand is null ).ToList();
            if(image.TakenDate is not null) {
                otherImages = otherImages.Where(i => i.TakenDate is not null).ToList();
                otherImages = otherImages.Where(i => Math.Abs((i.TakenDate.Value - image.TakenDate.Value).TotalHours) < 1 ).ToList();
            }
            foreach (var otherImage in otherImages)
            {
                // Aynı resimle karşılaştırma yapma
                if (image.Id == otherImage.Id) continue;

                // Hamming mesafesini hesapla
                var hammingDistance = CalculateHammingDistance(image.HashValue, otherImage.HashValue);

                // Eşik değer kontrolü (10 altında ise benzer kabul et)
                if (hammingDistance <= 10)
                {
                    // Veritabanına benzerlik kaydet
                    var similarity = new ImageSimilarity
                    {
                        ImageId = image.Id,
                        SimilarImageId = otherImage.Id,
                        SimilarityScore = hammingDistance
                    };

                    _context.ImageSimilarities.Add(similarity);
                }
            }

            _context.SaveChanges();
        }

        
    }

    private int CalculateHammingDistance(string hash1, string hash2)
    {
        int distance = 0;

        // Her iki hash'in aynı uzunlukta olması bekleniyor
        for (int i = 0; i < hash1.Length; i++)
        {
            if (hash1[i] != hash2[i])
                distance++;
        }

        return distance;
    }
}
