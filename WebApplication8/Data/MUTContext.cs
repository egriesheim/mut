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
            "Server=localhost\\SQLEXPRESS;Initial Catalog=Fut;Integrated Security=SSPI;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";


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

        //public virtual DbSet<Link> Links { get; set; }
        //public virtual DbSet<Players> Players { get; set; }
        //public virtual DbSet<Diamonds> Diamonds { get; set; }
        //public virtual DbSet<CoinsBack> CoinsBack { get; set; }
        //public virtual DbSet<Owned> Owned { get; set; }
        public virtual DbSet<FutPlayers> FutPlayers { get; set; }
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

    public class Owned
    {
        public int id { get; set; }
        public string userId { get; set; }
        public string team { get; set; }
        public string owned { get; set; }
    }

    public class FutPlayers
    {
        public int id { get; set; }
        public string href { get; set; }
        public string name { get; set; }
        public int overall { get; set; }
        public string position { get; set; }
        public string club { get; set; }
        public string nation { get; set; }
        public string league { get; set; }
        public int acceleration { get; set; }
        public int sprintSpeed { get; set; }
        public int positioning { get; set; }
        public int finishing { get; set; }
        public int shotPower { get; set; }
        public int longShots { get; set; }
        public int volleys { get; set; }
        public int penalties { get; set; }
        public int vision { get; set; }
        public int crossing { get; set; }
        public int freekickaccuracy { get; set; }
        public int shortPassing { get; set; }
        public int longPassing { get; set; }
        public int curve { get; set; }
        public int agility { get; set; }
        public int balance { get; set; }
        public int reactions { get; set; }
        public int ballControl { get; set; }
        public int dribbling { get; set; }
        public int composure { get; set; }
        public int interceptions { get; set; }
        public int headingAccuracy { get; set; }
        public int marking { get; set; }
        public int standingTackle { get; set; }
        public int slidingTackle { get; set; }
        public int jumping { get; set; }
        public int stamina { get; set; }
        public int strength { get; set; }
        public int aggression { get; set; }
        public string ps4cost { get; set; }
        public string xboxcost { get; set; }
        public string pccost { get; set; }
    }
}
