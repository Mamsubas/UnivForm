using System.ComponentModel.DataAnnotations.Schema;

public class ForumThread
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // İlişkiler
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public int AuthorId { get; set; } // BURAYI INT YAP
    [ForeignKey("AuthorId")]
    public virtual UnivForm.Data.AppUser Author { get; set; } = null!; // BURAYI AppUser YAP

    public virtual ICollection<Post> Posts { get; set; } = null!;
}