namespace GUIWebApi.Models
{
    public sealed class Category1
    {
        public int Category1Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product1> Products1 { get; set; }
    }
}
