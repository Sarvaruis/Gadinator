using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Instances")]
    public class Instances
    {
        [Key]
        [Column("InstancesId")]
        public int Id { get; set; }
        [Required]
        [Column("InstancesX")]
        public int X { get; set; }
        [Required]
        [Column("InstancesY")]
        public int Y { get; set; }
        [Required]
        [ForeignKey("ObjectsId")]
        public int ObjectId { get; set; }
        [Required]
        [ForeignKey("AreasId")]
        public int AreaId { get; set; }
    }
}
