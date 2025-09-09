using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="The Title is Require")]
        [MaxLength(100, ErrorMessage = "The Title must be less than 100 characters")]
        public string Title { get; set; }
        [Required(ErrorMessage = "The Content is Require")]

        public string Content { get; set; }
        [Required(ErrorMessage = "The Author is Require")]
        [MaxLength(50, ErrorMessage = "The Author must be less than 50 characters")]
        public string Author { get; set; }
        [ValidateNever]
        public string FeatureImagePath { get; set; }
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [ForeignKey("Category")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
