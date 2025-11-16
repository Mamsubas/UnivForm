public class Category
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    // Bir kategorinin birden çok başlığı olabilir
    // '?' ekleyerek bu özelliğin null olabileceğini belirtin.
    public virtual ICollection<ForumThread>? Threads { get; set; }
}