using System.ComponentModel.DataAnnotations.Schema;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // İlişkiler
    public int ForumThreadId { get; set; }
    public virtual ForumThread ForumThread { get; set; } = null!;

    public int AuthorId { get; set; } // BURAYI INT YAP
    [ForeignKey("AuthorId")]
    public virtual UnivForm.Data.AppUser Author { get; set; } = null!; // BURAYI AppUser YAP
    public int? ParentPostId { get; set; }

    [ForeignKey("ParentPostId")]
    public virtual Post ParentPost { get; set; } = null!; // Üst yorum
    public bool IsDeleted { get; set; } = false;
    public DateTime? EditedAt { get; set; } = null;

    // Bu yoruma verilen cevapların listesi
    public virtual ICollection<Post> Replies { get; set; } = new List<Post>();
}