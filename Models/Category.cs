using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The Name is Require")]
        [MaxLength(50, ErrorMessage = "The Name must be less than 50 characters")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<Post>? Posts { get; set; }


    }
}
