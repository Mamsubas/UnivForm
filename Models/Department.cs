using System.ComponentModel.DataAnnotations;

public class Department
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    public int Quota { get; set; }
    public decimal MinScore { get; set; }

    public ICollection<Student> Students { get; set; } = null!;
}