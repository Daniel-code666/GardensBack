using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    [Table("Role", Schema = "Data")]
    public class Role : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        // relaciones
        public ICollection<User> Users { get; set; } = [];
    }
}
