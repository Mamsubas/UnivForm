using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnivForm.Models
{
    public class ApplicationFileModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public string FileName { get; set; } = "";

        [Required]
        public string StoredPath { get; set; } = "";

        public long Size { get; set; }

        public string ContentType { get; set; } = "";

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; } = null!;
    }
}