using BIsm2.Models;
namespace BIsm2.ViewModels
{
    public class DetailsViewModel
    {
        public Product? Product { get; set; }
        public Comment? Comment { get; set; }
        public IEnumerable<Comment>? Comments { get; set; }

    }
}
