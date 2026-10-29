namespace GardensBack.Domain.Entities
{
    public class BaseEntity
    {
        public bool Active { get; set; } = true;
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
