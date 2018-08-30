using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApplication8.Data
{
    public class MutContext : ApplicationDbContext
    {
        public static string ConnectionString =
            "Server=localhost\\SQLEXPRESS;Database=MUT;Trusted_Connection=True;MultipleActiveResultSets=true";


        public MutContext()
        {

        }

        public MutContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<Players> Players { get; set; }
        public virtual DbSet<Diamonds> Diamonds { get; set; }
        public virtual DbSet<CoinsBack> CoinsBack { get; set; }
    }



    public class Players
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string team { get; set; }
        public int overall { get; set; }
        public string position { get; set; }
        public int median { get; set; }
        public string programName { get; set; }
    }

    public class Link
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Diamonds
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string team { get; set; }
        public int overall { get; set; }
        public string position { get; set; }
        public int median { get; set; }
        public string programName { get; set; }
        public string Team_Diamond { get; set; }
        public string column_10 { get; set; }
    }

    public class CoinsBack
    {
        public int id { get; set; }
        public string Team { get; set; }
        public int Coins { get; set; }
    }
}
