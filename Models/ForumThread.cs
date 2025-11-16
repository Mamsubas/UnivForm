using System.ComponentModel.DataAnnotations.Schema;

public class ForumThread
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    public int ViewCount { get; set; } = 0;  // Popülerlik ölçüsü
    public DateTime? LastActivity { get; set; }  // Son aktivite zamanı

    // Emoji ve Gif desteği
    public string? SelectedEmoji { get; set; }  // 🎉, 😀, ❓ gibi emojiler
    public string? GifUrl { get; set; }  // Gif URL'si

    // İlişkiler
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public int AuthorId { get; set; }
    [ForeignKey("AuthorId")]
    public virtual UnivForm.Data.AppUser Author { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = null!;
}