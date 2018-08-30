using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [Authorize]
        public IActionResult Index()
        {
            List<TeamDiamondSet> teamDiamondSet = new List<TeamDiamondSet>();
            var teams = db.Diamonds.Select(u => u.team).Distinct();

            foreach (var team in teams)
            {
                var tds = new TeamDiamondSet(team);
                teamDiamondSet.Add(tds);
            }

            var list = teamDiamondSet.OrderByDescending(u => u.profit).ToList();
            return View(list);
        }

        [Authorize]
        public IActionResult GeneratePlayerList()
        {
            while (!lastPage)
            {
                ParseURL(url + pageCount);
            }

            db.Links.AddRange(urlList);
            db.SaveChanges();

            return View("Index");
        }

        [Authorize]
        public IActionResult GetPlayers()
        {
            foreach (var player in db.Links.ToList())
            {
                GetPlayerData(player.url);
            }

            return View("Index");
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
