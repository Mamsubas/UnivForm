using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UnivForm.Data;

public class HighSchoolStudent
{
    public int Id { get; set; }

    [Required(ErrorMessage = "TC Kimlik No zorunludur")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
    public string TCKimlikNo { get; set; } = null!;

    [Required(ErrorMessage = "Ad alanı zorunludur")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Telefon numarası zorunludur")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public string BirthPlace { get; set; } = null!;

    [Required]
    public string Gender { get; set; } = null!;

    [Required]
    public string HighSchoolName { get; set; } = null!;

    [Required(ErrorMessage = "LYS puanı zorunludur")]
    [Range(100, 500, ErrorMessage = "LYS puanı 100-500 arasında olmalıdır")]
    public decimal ExamScore { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    public string Address { get; set; } = null!;

    public string ParentName { get; set; } = null!;
    public string ParentPhone { get; set; } = null!;

    // --- AppUser ile ilişki ---
    [ForeignKey("AppUser")]
    public int? AppUserId { get; set; }

    public AppUser? AppUser { get; set; }
}
