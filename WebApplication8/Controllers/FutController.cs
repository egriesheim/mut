using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication8.Data;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    public class FutController : Controller
    {
        private MutContext db = new MutContext();
        private string url = "https://www.futbin.com/players?page=";
        public int pageCount = 1;
        public bool lastPage = false;
        List<FutPlayers> futPlayers = new List<FutPlayers>();
        public IActionResult Index()
        {
            //GetLinks();
            //GetPlayerData();
            
            //CalculateStrikerQuickTiers();
            //CalculateStrikerStrongTiers();
            //CalculateStrikerSkilledTiers();
            CalculateWingerQuickTiers();

            return View();
        }

        [Authorize]
        public void GetLinks()
        {
            while (!lastPage)
            {
                ParseURL(url + pageCount);
                pageCount++;
            }

            db.FutPlayers.AddRange(futPlayers);
            db.SaveChanges();
        }

        [Authorize]
        public void ParseURL(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            

            var test = doc.DocumentNode.SelectNodes("//*[@id=\"repTb\"]/tbody/tr");
            if (test.Count == 1 && test[0].InnerText == "No Results")
            {
                lastPage = true;
                return;
            }
            foreach (var i in test)
            {
                string href = i.Attributes[1].Value;
                futPlayers.Add(new FutPlayers
                {
                    href = href
                });
            }
            Debug.WriteLine(url);
        }

        [Authorize]
        public void GetPlayerData()
        {
            var toUpdate = db.FutPlayers.Where(u => u.name == null).ToList();
            foreach (var player in toUpdate)
            {
                ParsePlayerPage(player);
            }
        }

        public void ParsePlayerPage(FutPlayers player)
        {
            try
            {
                var url = "https://www.futbin.com" + player.href;
                var web = new HtmlWeb();
                var doc = web.Load(url);

                player.name = doc.DocumentNode.SelectSingleNode("//*[@id=\"Player-card\"]/div[2]").InnerText;
                player.overall =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"Player-card\"]/div[1]").InnerText);
                player.position = doc.DocumentNode.SelectSingleNode("//*[@id=\"Player-card\"]/div[3]").InnerText;
                //var club = doc.DocumentNode.SelectNodes("//*[@id=\"info_content\"]/table/tbody/tr");
                //player.club = doc.DocumentNode.SelectSingleNode("//*[@id=\"info_content\"]/table/tbody/tr[2]/td/a")
                //    .InnerText;
                //player.nation = doc.DocumentNode.SelectSingleNode("//*[@id=\"info_content\"]/table/tbody/tr[3]/td/a")
                //    .InnerText;
                //player.league = doc.DocumentNode.SelectSingleNode("//*[@id=\"info_content\"]/table/tbody/tr[4]/td/a")
                //    .InnerText;
                player.acceleration = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-acceleration-val-0\"]/div[3]").InnerText);
                player.sprintSpeed =
                    Convert.ToInt32(
                        doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-sprintspeed-val-0\"]/div[3]").InnerText);
                player.positioning =
                    Convert.ToInt32(
                        doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-positioning-val-0\"]/div[3]").InnerText);
                player.finishing =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-finishing-val-0\"]/div[3]").InnerText);
                player.shotPower =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-shotpower-val-0\"]/div[3]").InnerText);
                player.longShots = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-longshotsaccuracy-val-0\"]/div[3]").InnerText);
                player.volleys =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-volleys-val-0\"]/div[3]").InnerText);
                player.penalties =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-penalties-val-0\"]/div[3]").InnerText);
                player.vision =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-vision-val-0\"]/div[3]").InnerText);
                player.crossing =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-crossing-val-0\"]/div[3]").InnerText);
                player.freekickaccuracy = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-freekickaccuracy-val-0\"]/div[3]").InnerText);
                player.shortPassing = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-shortpassing-val-0\"]/div[3]").InnerText);
                player.longPassing =
                    Convert.ToInt32(
                        doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-longpassing-val-0\"]/div[3]").InnerText);
                player.curve =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-curve-val-0\"]/div[3]").InnerText);
                player.agility =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-agility-val-0\"]/div[3]").InnerText);
                player.balance =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-balance-val-0\"]/div[3]").InnerText);
                player.reactions =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-reactions-val-0\"]/div[3]").InnerText);
                player.ballControl =
                    Convert.ToInt32(
                        doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-ballcontrol-val-0\"]/div[3]").InnerText);
                player.dribbling =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-dribbling-val-0\"]/div[3]").InnerText);
                player.composure =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-composure-val-0\"]/div[3]").InnerText);
                player.interceptions = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-interceptions-val-0\"]/div[3]").InnerText);
                player.headingAccuracy = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-headingaccuracy-val-0\"]/div[3]").InnerText);
                player.marking =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-marking-val-0\"]/div[3]").InnerText);
                player.standingTackle = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-standingtackle-val-0\"]/div[3]").InnerText);
                player.slidingTackle = Convert.ToInt32(doc.DocumentNode
                    .SelectSingleNode("//*[@id=\"sub-slidingtackle-val-0\"]/div[3]").InnerText);
                player.jumping =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-jumping-val-0\"]/div[3]").InnerText);
                player.stamina =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-stamina-val-0\"]/div[3]").InnerText);
                player.strength =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-strength-val-0\"]/div[3]").InnerText);
                player.aggression =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//*[@id=\"sub-aggression-val-0\"]/div[3]")
                        .InnerText);
                //player.ps4cost = doc.DocumentNode.SelectSingleNode("//*[@id=\"ps-lowest-1\"]/text()").InnerText;
                //player.xboxcost = doc.DocumentNode.SelectSingleNode("//*[@id=\"xbox-lowest-1\"]/text()").InnerText;
                //player.pccost = doc.DocumentNode.SelectSingleNode("//*[@id=\"pc-lowest-1\"]/text()").InnerText;

                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void CalculateTier(List<StatPair> statPairs)
        {
            var standardDeviation = CalculateStandardDeviation(statPairs);
            var avg = statPairs.Select(u => u.statTotal).Average();

            var tiera = avg + 3 * standardDeviation;
            var tierb = avg + 2 * standardDeviation;
            var tierc = avg + standardDeviation;
            var tierd = avg - standardDeviation;
            var tiere = avg - 2 * standardDeviation;
            var tierf = avg - 3 * standardDeviation;

            foreach (var statPair in statPairs)
            {
                if (statPair.statTotal > tiera)
                {
                    statPair.tier = "A";
                }
                else if (statPair.statTotal > tierb)
                {
                    statPair.tier = "B";
                }
                else if (statPair.statTotal > tierc)
                {
                    statPair.tier = "C";
                }
                else if (statPair.statTotal > tierd)
                {
                    statPair.tier = "D";
                }
                else if (statPair.statTotal > tiere)
                {
                    statPair.tier = "E";
                }
                else if (statPair.statTotal > tierf)
                {
                    statPair.tier = "F";
                }
            }
        }

        public double CalculateStandardDeviation(List<StatPair> stats)
        {
            var DataToCalculate = stats.Select(u => u.statTotal).ToList();

            int count = DataToCalculate.Count;



            double average = DataToCalculate.Average();
            double sum = DataToCalculate.Sum(d => Math.Pow(d - average, 2));
            double deviation = Math.Sqrt(sum / (count - 1));

            return deviation;
        }

        public void CalculateStrikerQuickTiers()
        {
            List<FutPlayers> strikers = db.FutPlayers.Where(u => u.position == "ST" || u.position == "CF" || u.position == "LF" || u.position == "RF").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var striker in strikers)
            {
                statPairs.Add(new StatPair
                {
                    player = striker,
                    statTotal = r.CalculateRole(striker, r.StrikerQuick)
                });
            }

            CalculateTiers(statPairs);
        }

        public void CalculateStrikerStrongTiers()
        {
            List<FutPlayers> strikers = db.FutPlayers.Where(u => u.position == "ST" || u.position == "CF" || u.position == "LF" || u.position == "RF").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var striker in strikers)
            {
                statPairs.Add(new StatPair
                {
                    player = striker,
                    statTotal = r.CalculateRole(striker, r.StrikerStrong)
                });
            }

            CalculateTiers(statPairs);
        }

        public void CalculateStrikerSkilledTiers()
        {
            List<FutPlayers> strikers = db.FutPlayers.Where(u => u.position == "ST" || u.position == "CF" || u.position == "LF" || u.position == "RF").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var striker in strikers)
            {
                statPairs.Add(new StatPair
                {
                    player = striker,
                    statTotal = r.CalculateRole(striker, r.StrikerSkilled)
                });
            }

            CalculateTiers(statPairs);
        }

        public void CalculateWingerQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "LW" || u.position == "RW" || u.position == "LM" || u.position == "RM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.WingerQuick)
                });
            }

            CalculateTiers(statPairs);
        }

        public void CalculateTiers(List<StatPair> statPairs)
        {
            statPairs = statPairs.OrderByDescending(u => u.statTotal).ToList();

            var standardDeviation = CalculateStandardDeviation(statPairs);

            var avg = statPairs.Select(u => u.statTotal).Average();

            CalculateTier(statPairs);

            var tiera = statPairs.Where(u => u.tier == "A").ToList();
            var tierb = statPairs.Where(u => u.tier == "B").ToList();
            var tierc = statPairs.Where(u => u.tier == "C").ToList();
            var tierd = statPairs.Where(u => u.tier == "D").ToList();
            var tiere = statPairs.Where(u => u.tier == "E").ToList();
            var tierf = statPairs.Where(u => u.tier == "F").ToList();
        }
    }

    
}