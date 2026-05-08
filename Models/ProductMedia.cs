namespace BIsm2.Models
{
    public class ProductMedia
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        
        public byte[]? FileData { get; set; }


        public string? ContentType { get; set; }   // store relative path or URL
        public MediaType Type { get; set; }    // enum: Image or Video

        public Product? Product { get; set; }
    }
    public enum MediaType
    {
        Image,
        Video
    }
}

