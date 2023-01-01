namespace Document.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<Requester>? Employees { get; set; } = new HashSet<Requester>();
    }
}
