using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIsm2.Models
{
    public class Comment
    {

        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? UserId { get; set; }
        public string? UName { get; set; }
        public string? Content { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        public Product? Product { get; set; }
    }
}
