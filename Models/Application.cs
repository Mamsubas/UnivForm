using System.ComponentModel.DataAnnotations;

namespace UnivForm.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string NationalId { get; set; } = null!;

        public DateTime BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Department { get; set; } = "";
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool AcceptedPolicy { get; set; }

        public ICollection<ApplicationFileModel> Files { get; set; } = new List<ApplicationFileModel>();
    }
}