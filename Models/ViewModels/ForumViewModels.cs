using System.ComponentModel.DataAnnotations;

namespace UnivForm.Models.ViewModels
{
    public class CreateThreadViewModel
    {
        [Required(ErrorMessage = "Konu başlığı zorunludur.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Başlık 5-150 karakter arasında olmalıdır.")]
        [Display(Name = "Konu Başlığı")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Konu içeriği zorunludur.")]
        [Display(Name = "Konu İçeriği (İlk Mesaj)")]
        public string Content { get; set; } = "";

        [Required(ErrorMessage = "Bir kategori seçmelisiniz.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
    }
    public class CreatePostViewModel
    {
        [Required]
        public int ThreadId { get; set; } // Hangi konuya yazıldığını bilmek için

        [Required(ErrorMessage = "Cevap boş olamaz.")]
        [Display(Name = "Cevabınız")]
        public string Content { get; set; } = "";
        public int? ParentPostId { get; set; }
    }

    // KONU DETAY SAYFASINI GÖSTERMEK İÇİN
    public class ThreadDetailViewModel
    {
        public ForumThread Thread { get; set; } = null!;
        public IEnumerable<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public CreatePostViewModel NewPost { get; set; } = null!; // Yeni post formu için
    }
    public class PostViewModel
    {
        public Post Post { get; set; } = null!;
        public int LikeCount { get; set; } = 0;
        public bool UserHasLiked { get; set; } = false;
        public ICollection<PostViewModel> Replies { get; set; } = new List<PostViewModel>();
    }


    // KONU DETAY SAYFASINI GÜNCELLE

}