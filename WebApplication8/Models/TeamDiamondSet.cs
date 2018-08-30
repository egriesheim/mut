using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
        public List<Diamonds> owned { get; set; }
        public int ownedCount { get; set; }

        public TeamDiamondSet(string t, string uId)
        {
            team = t;
            var ownedItems = db.Owned.FirstOrDefault(u => u.userId == uId && u.team == team);
            if (ownedItems != null)
            {
                var ownedArray = ownedItems.owned.Split(",").ToList();
                owned = new List<Diamonds>();
                foreach (var item in ownedArray)
                {
                    owned.Add(db.Diamonds.FirstOrDefault(u => u.id == Convert.ToInt32(item)));
                }
            }
            players = GetPlayers();
            cost = players.Sum(u => u.median);
            coinsBack = CoinsBack();
            diamondPlayerCost = db.Players
                .Where(u => u.team == team && u.programName == "Team Diamonds" && u.lastName != "Lott")
                .Select(u => u.median).FirstOrDefault();
            profit = coinsBack + diamondPlayerCost - cost;
            if (owned != null)
            {
                ownedCount = owned.Count();
            }
            else
            {
                ownedCount = 0;
            }
        }

        private List<Diamonds> GetPlayers()
        {
            List<Diamonds> players = new List<Diamonds>();
            
            if (owned != null)
            {
                players = db.Diamonds.Where(u => u.team == team || (team.Contains(u.column_10) && u.column_10 != "")).ToList();
                foreach (var own in owned)
                {
                    players.Remove(own);
                }
            }
            else
            {
                players = db.Diamonds.Where(u => u.team == team || (team.Contains(u.column_10) && u.column_10 != "")).ToList();
            }
            
            

            return players;
        }

        public int CoinsBack()
        {
            return db.CoinsBack.Where(u => u.Team == team).Select(u => u.Coins).FirstOrDefault();
        }

        
    }
}
