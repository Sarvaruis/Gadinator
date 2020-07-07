using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Objects")]
    public class Objects
    {
        [Key]
        [Column("ObjectsId")]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [Column("ObjectsName")]
        public string Name { get; set; }
        [Required]
        [Column("ObjectsWidth")]
        public int Width { get; set; }
        [Required]
        [Column("ObjectsHeight")]
        public int Height { get; set; }
        [MaxLength(100)]
        [Column("ObjectsImagePath")]
        public string ImagePath { get; set; }
        [Required]
        [ForeignKey("CategoriesId")]
        public int CategoryId { get; set; }
    }
}
