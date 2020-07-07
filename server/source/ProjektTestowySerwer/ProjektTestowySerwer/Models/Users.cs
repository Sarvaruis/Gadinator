using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjektTestowySerwer.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [Column("UsersId")]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [Column("UsersLogin")]
        public string Login { get; set; }
        [Required]
        [MaxLength(50)]
        [Column("UsersPassword")]
        public string Password { get; set; }
    }
}
