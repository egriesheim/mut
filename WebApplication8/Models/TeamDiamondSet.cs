using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication8.Data;

namespace WebApplication8.Models
{
    public class TeamDiamondSet
    {
        private MutContext db = new MutContext();

        [Key]
        public int id { get; set; }
        public string team { get; set; }
        public int cost { get; set; }
        public List<Diamonds> players { get; set; }
        public int coinsBack { get; set; }
        public int diamondPlayerCost { get; set; }
        public int profit { get; set; }

        public TeamDiamondSet(string t)
        {
            team = t;
            players = GetPlayers();
            cost = players.Sum(u => u.median);
            coinsBack = CoinsBack();
            diamondPlayerCost = db.Players
                .Where(u => u.team == team && u.programName == "Team Diamonds" && u.lastName != "Lott")
                .Select(u => u.median).FirstOrDefault();
            profit = coinsBack + diamondPlayerCost - cost;
        }

        private List<Diamonds> GetPlayers()
        {
            List<Diamonds> players = new List<Diamonds>();

            players = db.Diamonds.Where(u => u.team == team || team.Contains(u.column_10)).ToList();

            return players;
        }

        public int CoinsBack()
        {
            return db.CoinsBack.Where(u => u.Team == team).Select(u => u.Coins).FirstOrDefault();
        }

        
    }
}
