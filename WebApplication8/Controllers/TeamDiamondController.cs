using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication8.Controllers
{
    public class TeamDiamondController : Controller
    {
        private MutContext db = new MutContext();
        static bool lastPage = false;
        static int pageCount = 1;
        static string url = "https://www.muthead.com/19/players?page=";
        static List<Link> urlList = new List<Link>();
        // GET: /<controller>/

        
        public IActionResult Index()
        {
            List<TeamDiamondSet> teamDiamondSet = new List<TeamDiamondSet>();
            var teams = db.Diamonds.Select(u => u.team).Distinct();
            var uId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var team in teams)
            {
                if (team == "Free Agents") continue;
                var tds = new TeamDiamondSet(team, uId);
                teamDiamondSet.Add(tds);
            }

            var list = teamDiamondSet.OrderByDescending(u => u.profit).ToList();
            return View(list);
        }

        [Authorize]
        public IActionResult Modify(string team)
        {
            var uId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = db.Owned.FirstOrDefault(u => u.team == team && u.userId == uId);
            if (owned != null)
            {
                ViewBag.id = owned.id;
                ViewBag.owned = owned.owned;
            }
            else
            {
                ViewBag.id = null;
                ViewBag.owned = null;
            }
            ViewBag.Team = team;
            var players = db.Diamonds.Where(u => u.team == team || (team.Contains(u.column_10) && u.column_10 != "")).ToList();
            return View(players);
        }

        public IActionResult UpdatePlayerData()
        {
            GeneratePlayerList();

            GetPlayers();

            return View(null);
        }


        public void GeneratePlayerList()
        {
            var allLinks = db.Links.ToList();
            db.Links.RemoveRange(allLinks);

            db.SaveChanges();

            while (!lastPage)
            {
                ParseURL(url + pageCount);
            }

            db.Links.AddRange(urlList);
            db.SaveChanges();
        }


        public void GetPlayers()
        {
            var allPlayers = db.Players.ToList();
            db.Players.RemoveRange(allPlayers);

            db.SaveChanges();

            foreach (var player in db.Links.ToList())
            {
                GetPlayerData(player.url);
            }
        }

        [Authorize]
        public void ParseURL(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var test = doc.DocumentNode.SelectNodes("//*[@id='Player Cards']/tbody/tr/td[2]/div/a");
            foreach (var i in test)
            {
                string href = i.Attributes[0].Value;
                urlList.Add(new Link
                {
                    url = href
                });
            }

            pageCount++;

            if (doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/div/div/div[2]/div/ul/li[8]/a") == null)
            {
                lastPage = true;
            }
        }

        [HttpPost]
        public JsonResult AddOwned(string Team, string[] Owned)
        {
            var uId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (db.Owned.Any(u => u.userId == uId && u.team == Team))
            {
                var owned =  db.Owned.FirstOrDefault(u => u.userId == uId && u.team == Team);
                UpdateOwned(Owned, owned.id.ToString());

                return Json("'Success':'true'");
            }
            else
            {
                Owned o = new Owned
                {
                    userId = uId,
                    team = Team,
                    owned = String.Join(",", Owned)
                };

                db.Owned.Add(o);

                try
                {
                    db.SaveChanges();
                    return Json("'Success':'true'");
                }
                catch (Exception e)
                {
                    return Json("'Success':'false'");
                }
            }
            
        }

        [HttpPost]
        public JsonResult UpdateOwned(string[] owned, string id)
        {

            Owned o = db.Owned.Find(Convert.ToInt32(id));

            o.owned = String.Join(",", owned);

            if (o.owned == "")
            {
                db.Remove(o);
                db.SaveChanges();
                return Json("'Success':'true'");
            }

            db.Entry(o).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
                return Json("'Success':'true'");
            }
            catch (Exception e)
            {
                return Json("'Success':'false'");
            }
        }

        public void GetPlayerData(string href)
        {
            try
            {
                href = href.Replace("/players", "/players/prices");
                var url = "https://www.muthead.com" + href + "/playstation-4";
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var firstName = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[2]/div[1]/span[1]").InnerText;
                var lastName = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[2]/div[1]/span[2]").InnerText;
                var team = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[2]/div[1]/div/div/span[1]").InnerText;
                var overall = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[1]/div[1]/span[1]").InnerText;
                var position = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[1]/div[1]/span[2]").InnerText;
                var median = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[2]/div[2]/ul/li[2]/a/div/span[2]/text()").InnerText;
                var programName = doc.DocumentNode.SelectSingleNode("//*[@id='content']/section/header/div[1]/div[3]/div").InnerText;

                Players player = new Players
                {
                    firstName = firstName,
                    lastName = lastName,
                    team = team,
                    overall = Convert.ToInt32(overall),
                    position = position,
                    median = ConvertMedian(median),
                    programName = programName
                };

                db.Players.Add(player);
                db.SaveChanges();

                var diamond = db.Diamonds.FirstOrDefault(u =>
                    u.firstName == firstName && u.lastName == lastName && u.programName == programName);
                if (diamond != null)
                {
                    diamond.median = player.median;
                    db.Entry(diamond).State = EntityState.Modified;
                    db.SaveChanges();
                }


            }
            catch
            {
                Console.WriteLine(href + " failed.");
            }
        }

        public int ConvertMedian(string median)
        {
            int newMedian;
            if (median.Contains("M"))
            {
                newMedian = Convert.ToInt32((Convert.ToDouble(median.Replace("M", "")) * 1000000));
            }
            else if (median.Contains("K"))
            {
                newMedian = Convert.ToInt32((Convert.ToDouble(median.Replace("K", "")) * 1000));
            }
            else
            {
                newMedian = Convert.ToInt32(median.Replace(",", ""));
            }

            return newMedian;
        }
    }
}
