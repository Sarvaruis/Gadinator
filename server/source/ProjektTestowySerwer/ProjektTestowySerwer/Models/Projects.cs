using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Projects")]
    public class Projects
    {
        [Key]
        [Column("ProjectsId")]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [Column("ProjectsName")]
        public string Name { get; set; }
        [MaxLength(100)]
        [Column("ProjectsBackgroundFilePath")]
        public string BackgroundFilePath { get; set; }
        [Required]
        [Column("ProjectsGridWidth")]
        public int GridWidth { get; set; }
        [Required]
        [Column("ProjectsGridHeight")]
        public int GridHeight { get; set; }
        [Required]
        [ForeignKey("UsersId")]
        public int UserId { get; set; }
    }
}
