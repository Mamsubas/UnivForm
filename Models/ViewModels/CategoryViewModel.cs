using System.ComponentModel.DataAnnotations;

namespace UnivForm.Models.ViewModels
{
    public class CategoryViewModel
    {
        [Required(ErrorMessage = "Kategori başlığı zorunludur.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Başlık 3-100 karakter arasında olmalıdır.")]
        [Display(Name = "Kategori Başlığı")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = "";
    }
}