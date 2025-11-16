using System;
using System.ComponentModel.DataAnnotations;
using UnivForm.Data; // AppUser için

namespace UnivForm.Models
{
    public class PostLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public virtual AppUser User { get; set; } = null!;

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}