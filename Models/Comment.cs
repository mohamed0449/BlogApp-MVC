using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "The UserName is Require")]
        [MaxLength(50, ErrorMessage = "The UserName must be less than 50 characters")]
        public string UserName { get; set; }
        [DataType(DataType.Date)]
        public DateTime CommentDate { get; set; }
        [Required(ErrorMessage = "The Content is Require")]

        public string Content { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        [ValidateNever]
        public Post Post { get; set; }

    }
}
