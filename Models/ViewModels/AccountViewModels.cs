using System.ComponentModel.DataAnnotations;

namespace UnivForm.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "İsim zorunludur.")]
        [Display(Name = "İsim")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Soyisim zorunludur.")]
        [Display(Name = "Soyisim")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Şifre (Tekrar)")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
        [Display(Name = "Kullanıcı Adı veya E-posta")]
        public string UsernameOrEmail { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = "";

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }

    // --- YENİ: Öğrenci Kaydı Modeli ---
    public class StudentRegisterViewModel
    {
        [Required(ErrorMessage = "Öğrenci tipi zorunludur.")]
        [Display(Name = "Öğrenci Tipi")]
        public string? StudentTypeSelect { get; set; } // "HighSchool" veya "University"

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        [StringLength(50)]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "İsim zorunludur.")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "TC Kimlik No zorunludur.")]
        [Display(Name = "TC Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        public string TCKimlikNo { get; set; } = "";

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Doğum yeri zorunludur.")]
        [Display(Name = "Doğum Yeri")]
        public string BirthPlace { get; set; } = "";

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "";

        // --- LİSE ÖĞRENCİSİ ALANLARI ---
        [Display(Name = "Lise")]
        public string? HighSchoolName { get; set; }

        [Range(100, 500, ErrorMessage = "Sınav puanı 100-500 arasında olmalıdır")]
        [Display(Name = "Sınav Puanı")]
        public decimal? ExamScore { get; set; }

        // --- ÜNİVERSİTE ÖĞRENCİSİ ALANLARI ---
        [Display(Name = "Üniversite")]
        public string? University { get; set; }

        [Display(Name = "Bölüm")]
        public string? Department { get; set; }

        [Range(1, 4, ErrorMessage = "Sınıf 1-4 arasında olmalıdır")]
        [Display(Name = "Sınıf")]
        public int? Grade { get; set; }

        [Display(Name = "Öğrenci Numarası")]
        public string? StudentId { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Şifre (Tekrar)")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = "";
    }

    // Admin - Kullanıcı Rol Yönetimi
    public class UserRolesViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public List<RoleSelection> AllRoles { get; set; } = new();
    }

    public class RoleSelection
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}