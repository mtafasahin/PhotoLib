using System;

namespace PhotoGallery.Entities;

public class ImageLabel
{
    public int ImageId { get; set; }
    public Image Image { get; set; }

    public int LabelId { get; set; }
    public Label Label { get; set; }
}
