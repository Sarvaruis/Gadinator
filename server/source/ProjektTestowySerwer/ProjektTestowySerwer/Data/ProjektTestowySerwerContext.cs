using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjektTestowySerwer.Models;

namespace ProjektTestowySerwer.Models
{
    public class ProjektTestowySerwerContext : DbContext
    {
        public ProjektTestowySerwerContext (DbContextOptions<ProjektTestowySerwerContext> options)
            : base(options) {}

        public DbSet<ProjektTestowySerwer.Models.Users> Users { get; set; }
        public DbSet<ProjektTestowySerwer.Models.Projects> Projects { get; set; }
        public DbSet<ProjektTestowySerwer.Models.Categories> Categories { get; set; }
        public DbSet<ProjektTestowySerwer.Models.Objects> Objects { get; set; }
        public DbSet<ProjektTestowySerwer.Models.Areas> Areas { get; set; }
        public DbSet<ProjektTestowySerwer.Models.Instances> Instances { get; set; }
    }
}
