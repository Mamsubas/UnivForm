using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace UnivForm.Data
{
    public enum StudentType
    {
        HighSchool,    // Lise son sınıf
        University     // Üniversite
    }

    public class AppUser : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        // --- Profil Alanları ---
        [MaxLength(500)]
        public string? Biography { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        // --- Öğrenci tipi (Lise veya Üniversite) ---
        public StudentType? StudentType { get; set; }

        // --- YENİ: Eski Student ilişkisi (ileride kaldırılabilir) ---
        public Student? Student { get; set; }

        // --- YENİ: HighSchoolStudent ilişkisi ---
        public HighSchoolStudent? HighSchoolStudent { get; set; }

        // --- YENİ: UniversityStudent ilişkisi ---
        public UniversityStudent? UniversityStudent { get; set; }

        // --- Ban ve Uyarı Sistemi ---
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedAt { get; set; }
        public string? BanReason { get; set; }
        public int WarningCount { get; set; } = 0;
        public DateTime? LastWarning { get; set; }
    }
}