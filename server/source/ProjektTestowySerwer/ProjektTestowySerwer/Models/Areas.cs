using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Areas")]
    public class Areas
    {
        [Key]
        [Column("AreasId")]
        public int Id { get; set; }
        [MaxLength(50)]
        [Column("AreasName")]
        public string Name { get; set; }
        [Required]
        [Column("AreasX")]
        public int X { get; set; }
        [Required]
        [Column("AreasY")]
        public int Y { get; set; }
        [Required]
        [Column("AreasWidth")]
        public int Width { get; set; }
        [Required]
        [Column("AreasHeight")]
        public int Height { get; set; }
        public int ParentAreaId { get; set; }
        [Required]
        [ForeignKey("ProjectsId")]
        public int ProjectId { get; set; }
    }
}
