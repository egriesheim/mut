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
        public List<FutPlayers> existingPlayers = new List<FutPlayers>();
        public int pageCount = 1;
        public bool lastPage = false;
        List<FutPlayers> futPlayers = new List<FutPlayers>();
        public IActionResult Index()
        {
            //GetLinks();
            //GetPlayerData();
            GetPrices();
            
            //CalculateStrikerQuickTiers();
            //CalculateStrikerStrongTiers();
            //CalculateStrikerSkilledTiers();
            //CalculateWingerQuickTiers();

            return View();
        }

        public ActionResult StrikerQuick()
        {
            return View(CalculateStrikerQuickTiers());
        }

        public ActionResult StrikerStrong()
        {
            return View(CalculateStrikerStrongTiers());
        }

        public ActionResult StrikerSkilled()
        {
            return View(CalculateStrikerSkilledTiers());
        }

        public ActionResult WingerQuick()
        {
            return View(CalculateWingerQuickTiers());
        }

        public ActionResult WingerSkilled()
        {
            return View(CalculateWingerSkilledTiers());
        }

        public ActionResult AttackingMidQuick()
        {
            return View(CalculateAttackingMidQuickTiers());
        }

        public ActionResult AttackingMidSkilled()
        {
            return View(CalculateAttackingMidSkilledTiers());
        }

        public ActionResult MidQuick()
        {
            return View(CalculateMidQuickTiers());
        }

        public ActionResult MidSkilled()
        {
            return View(CalculateMidSkilledTiers());
        }

        public ActionResult MidDefender()
        {
            return View(CalculateMidDefenderTiers());
        }

        public ActionResult DefendingMidQuick()
        {
            return View(CalculateDefendingMidQuickTiers());
        }

        public ActionResult DefendingMidSkilled()
        {
            return View(CalculateDefendingMidSkilledTiers());
        }

        public ActionResult DefendingMidDefender()
        {
            return View(CalculateDefendingMidDefenderTiers());
        }

        public ActionResult BackQuick()
        {
            return View(CalculateBackQuickTiers());
        }

        public ActionResult BackSkilled()
        {
            return View(CalculateBackSkilledTiers());
        }

        public ActionResult BackStrong()
        {
            return View(CalculateBackStrongTiers());
        }

        public ActionResult DefenderQuick()
        {
            return View(CalculateDefenderQuickTiers());
        }

        public ActionResult DefenderSkilled()
        {
            return View(CalculateDefenderSkilledTiers());
        }

        public ActionResult DefenderStrong()
        {
            return View(CalculateDefenderStrongTiers());
        }

        [Authorize]
        public void GetLinks()
        {
            existingPlayers = db.FutPlayers.ToList();
            while (!lastPage)
            {
                ParseURL(url + pageCount);
                pageCount++;
            }

            db.FutPlayers.AddRange(futPlayers);
            db.SaveChanges();
        }

        [Authorize]
        public void GetPrices()
        {
            existingPlayers = db.FutPlayers.ToList();
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
                if(existingPlayers.Any(u => u.href == href))
                {
                    var player = existingPlayers.FirstOrDefault(u => u.href == href);
                    player.ps4cost = i.SelectSingleNode("td[5]/span[1]").InnerText;
                    db.Entry(player).State = EntityState.Modified;
                    db.SaveChanges();
                }
                
            }
            
        }

        [Authorize]
        public async void GetPlayerData()
        {
            var toUpdate = db.FutPlayers.Where(u => u.ps4cost == null).OrderByDescending(u => u.overall).ToList();
            List<Task> TaskList = new List<Task>();
            foreach (var player in toUpdate)
            {
                var LastTask = ParsePlayerPage(player);
                TaskList.Add(LastTask);
            }

            await Task.WhenAll(TaskList.ToArray());
        }

        public async Task ParsePlayerPage(FutPlayers player)
        {
            try
            {
                var url = "https://www.futbin.com" + player.href;
                var web = new HtmlWeb();
                var doc = web.Load(url);
                /*
                player.name = doc.DocumentNode.SelectSingleNode("//div[@id='Player-card']//div[@class='pcdisplay-name']").InnerText;
                player.overall =
                    Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//div[@id='Player-card']//div[@class='pcdisplay-rat']").InnerText);
                player.position = doc.DocumentNode.SelectSingleNode("//div[@id='Player-card']//div[@class='pcdisplay-pos']").InnerText;
                
                player.club = doc.DocumentNode.SelectSingleNode("//div[@class='container p-xs-0']//ul[contains(@class,'list-unstyled list-inline')]//li[2]//a").InnerText;
                player.nation = doc.DocumentNode.SelectSingleNode("//div[@class='container p-xs-0']//ul[contains(@class,'list-unstyled list-inline')]//li[1]//a").InnerText;
                player.league = doc.DocumentNode.SelectSingleNode("//div[@class='container p-xs-0']//ul[contains(@class,'list-unstyled list-inline')]//li[3]//a").InnerText;
                
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
                */

                db.Entry(player).State = EntityState.Modified;
                await db.SaveChangesAsync();
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

            var tier1 = avg + 3 * standardDeviation;
            var tier2 = avg + 2.5 * standardDeviation;
            var tier3 = avg + 2 * standardDeviation;
            var tier4 = avg + 1.5 * standardDeviation;
            var tier5 = avg + standardDeviation;
            var tier6 = avg + 0.5 * standardDeviation;
            var tier7 = avg - 0.5 * standardDeviation;
            var tier8 = avg - standardDeviation;
            var tier9 = avg - 1.5 * standardDeviation;
            var tier10 = avg - 2 * standardDeviation;
            var tier11 = avg - 2.5 * standardDeviation;
            var tier12 = avg - 3 * standardDeviation;

            foreach (var statPair in statPairs)
            {
                if (statPair.statTotal > tier1)
                {
                    statPair.tier = "1";
                }
                else if (statPair.statTotal > tier2)
                {
                    statPair.tier = "2";
                }
                else if (statPair.statTotal > tier3)
                {
                    statPair.tier = "3";
                }
                else if (statPair.statTotal > tier4)
                {
                    statPair.tier = "4";
                }
                else if (statPair.statTotal > tier5)
                {
                    statPair.tier = "5";
                }
                else if (statPair.statTotal > tier6)
                {
                    statPair.tier = "6";
                }
                else if (statPair.statTotal > tier7)
                {
                    statPair.tier = "7";
                }
                else if (statPair.statTotal > tier8)
                {
                    statPair.tier = "8";
                }
                else if (statPair.statTotal > tier9)
                {
                    statPair.tier = "9";
                }
                else if (statPair.statTotal > tier10)
                {
                    statPair.tier = "10";
                }
                else if (statPair.statTotal > tier11)
                {
                    statPair.tier = "11";
                }
                else
                {
                    statPair.tier = "12";
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

        public List<StatPair> CalculateStrikerQuickTiers()
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

            return statPairs;
        }

        public List<StatPair> CalculateStrikerStrongTiers()
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

            return statPairs;
        }

        public List<StatPair> CalculateStrikerSkilledTiers()
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

            return statPairs;
        }

        public List<StatPair> CalculateWingerQuickTiers()
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

            return statPairs;
        }

        public List<StatPair> CalculateWingerSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "LW" || u.position == "RW" || u.position == "LM" || u.position == "RM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.WingerSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateAttackingMidQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CAM" || u.position == "CM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.AttackingMidQuick)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateAttackingMidSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CAM" || u.position == "CM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.AttackingMidSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateMidQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CM" || u.position == "CAM" || u.position == "CDM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.MidQuick)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateMidSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CM" || u.position == "CAM" || u.position == "CDM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.MidSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateMidDefenderTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CM" || u.position == "CAM" || u.position == "CDM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.MidDefender)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefendingMidQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CDM" || u.position == "CM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefendingMidQuick)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefendingMidSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CDM" || u.position == "CM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefendingMidSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefendingMidDefenderTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CDM" || u.position == "CM").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefendingMidDefender)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateBackQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "LB" || u.position == "LWB" || u.position == "RB" || u.position == "RWB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.BackQuick)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateBackSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "LB" || u.position == "LWB" || u.position == "RB" || u.position == "RWB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.BackSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateBackStrongTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "LB" || u.position == "LWB" || u.position == "RB" || u.position == "RWB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.BackStrong)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefenderQuickTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefenderQuick)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefenderSkilledTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefenderSkilled)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
        }

        public List<StatPair> CalculateDefenderStrongTiers()
        {
            List<FutPlayers> players = db.FutPlayers.Where(u => u.position == "CB").ToList();

            List<StatPair> statPairs = new List<StatPair>();

            Roles r = new Roles();

            foreach (var player in players)
            {
                statPairs.Add(new StatPair
                {
                    player = player,
                    statTotal = r.CalculateRole(player, r.DefenderStrong)
                });
            }

            CalculateTiers(statPairs);

            return statPairs;
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
            var tierg = statPairs.Where(u => u.tier == "G").ToList();
        }
    }

    
}