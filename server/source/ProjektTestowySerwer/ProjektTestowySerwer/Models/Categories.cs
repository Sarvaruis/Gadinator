using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Categories")]
    public class Categories
    {
        [Key]
        [Column("CategoriesId")]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [Column("CategoriesName")]
        public string Name { get; set; }
    }
}
