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
        private string url = "https://www.futbin.com/players?version=gold_nr&page=";
        public List<FutPlayers> existingPlayers = new List<FutPlayers>();
        public int pageCount = 1;
        public bool lastPage = false;
        List<FutPlayers> futPlayers = new List<FutPlayers>();
        private List<Team> leagueHighs = new List<Team>();
        public IActionResult Index()
        {
            
            GetLinks();
            //GetPlayerData();
            //GetPrices();
            
            
            //var values = GetClubValues();

            //CalculateStrikerQuickTiers();
            //CalculateStrikerStrongTiers();
            //CalculateStrikerSkilledTiers();
            //CalculateWingerQuickTiers();

            return View();
        }

        public class ClubValues
        {
            public string clubName { get; set; }
            public double averageCost { get; set; }
        }

        public List<ClubValues> GetClubValues()
        {
            List<ClubValues> clubValues = new List<ClubValues>();
            var players = db.FutPlayers.ToList();
            var clubs = players.Select(u => u.club).Distinct().ToList();
            foreach (var club in clubs)
            {
                var costs = players.Where(u => u.club == club).Select(u => u.ps4cost).ToList();
                if(costs.Count < 5)
                {
                    continue;
                }
                var sum = 0;
                var convertedCosts = new List<int>();
                foreach (var cost in costs)
                {
                    convertedCosts.Add(GetCost(cost));
                }

                convertedCosts = convertedCosts.OrderBy(p => p).Take(3).ToList();

                sum = convertedCosts.Sum() / 3;


                clubValues.Add(new ClubValues
                {
                    clubName = club,
                    averageCost = sum
                });
            }

            clubValues = clubValues.OrderByDescending(u => u.averageCost).ToList();

            return clubValues;
        }

        public ActionResult FutStats(string type)
        {
            ViewBag.Type = type;
            if (type == "StrikerQuick")
            {
                return View(CalculateStrikerQuickTiers());
            }
            else if (type == "StrikerStrong")
            {
                return View(CalculateStrikerStrongTiers());
            }
            else if (type == "StrikerSkilled")
            {
                return View(CalculateStrikerSkilledTiers());
            }
            else if (type == "WingerQuick")
            {
                return View(CalculateWingerQuickTiers());
            }
            else if (type == "WingerSkilled")
            {
                return View(CalculateWingerSkilledTiers());
            }
            else if (type == "AttackingMidQuick")
            {
                return View(CalculateAttackingMidQuickTiers());
            }
            else if (type == "AttackingMidSkilled")
            {
                return View(CalculateAttackingMidSkilledTiers());
            }
            else if (type == "MidQuick")
            {
                return View(CalculateMidQuickTiers());
            }
            else if (type == "MidSkilled")
            {
                return View(CalculateMidSkilledTiers());
            }
            else if (type == "MidDefender")
            {
                return View(CalculateMidDefenderTiers());
            }
            else if (type == "DefendingMidQuick")
            {
                return View(CalculateDefendingMidQuickTiers());
            }
            else if (type == "DefendingMidSkilled")
            {
                return View(CalculateDefendingMidSkilledTiers());
            }
            else if (type == "DefendingMidDefender")
            {
                return View(CalculateDefendingMidDefenderTiers());
            }
            else if (type == "BackQuick")
            {
                return View(CalculateBackQuickTiers());
            }
            else if (type == "BackSkilled")
            {
                return View(CalculateBackSkilledTiers());
            }
            else if (type == "BackStrong")
            {
                return View(CalculateBackStrongTiers());
            }
            else if (type == "DefenderQuick")
            {
                return View(CalculateDefenderQuickTiers());
            }
            else if (type == "DefenderSkilled")
            {
                return View(CalculateDefenderSkilledTiers());
            }
            else if (type == "DefenderStrong")
            {
                return View(CalculateDefenderStrongTiers());
            }
            else
            {
                return View();
            }
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
                else
                {
                    var player = new FutPlayers
                    {
                        href = href,
                        ps4cost = i.SelectSingleNode("td[5]/span[1]").InnerText
                    };

                    db.FutPlayers.Add(player);
                    db.SaveChanges();
                }
            }
            
        }

        [Authorize]
        public async void GetPlayerData()
        {
            var toUpdate = db.FutPlayers.Where(u => u.name == null).ToList();
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

            foreach (var statPair in statPairs)
            {
                statPair.player.ps4cost = GetCost(statPair.player.ps4cost).ToString();
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

        public void GetBestValue(string league, int totalCost)
        {
            int combinations = 1000;
            List<Team> teams = new List<Team>();
            for (int i = 0; i < combinations; i++)
            {
                var pool = db.FutPlayers.Where(u => u.league == league && u.ps4cost != "0" && u.overall >= 75).ToList();
                Random r = new Random();
                var st = pool.Where(u => u.position == "ST" || u.position == "CF").ElementAt(r.Next(1, pool.Where(u => u.position == "ST" || u.position == "CF").Count()));
                var lw = pool.Where(u => u.position == "LW" || u.position == "LF" || u.position == "LM").ElementAt(r.Next(1, pool.Where(u => u.position == "LW" || u.position == "LF" || u.position == "LM").Count()));
                var rw = pool.Where(u => u.position == "RW" || u.position == "RF" || u.position == "RM").ElementAt(r.Next(1, pool.Where(u => u.position == "RW" || u.position == "RF" || u.position == "RM").Count()));
                var cm = pool.Where(u => u.position == "CM" || u.position == "CDM" || u.position == "CAM").ElementAt(r.Next(1, pool.Where(u => u.position == "CM" || u.position == "CDM" || u.position == "CAM").Count()));
                var cm2 = pool.Where(u => u.position == "CM" || u.position == "CDM" || u.position == "CAM").ElementAt(r.Next(1, pool.Where(u => u.position == "CM" || u.position == "CDM" || u.position == "CAM").Count()));
                var cdm = pool.Where(u => u.position == "CM" || u.position == "CDM").ElementAt(r.Next(1, pool.Where(u => u.position == "CM" || u.position == "CDM").Count()));
                var lb = pool.Where(u => u.position == "LB" || u.position == "LWB").ElementAt(r.Next(1, pool.Where(u => u.position == "LB" || u.position == "LWB").Count()));
                var cb = pool.Where(u => u.position == "CB").ElementAt(r.Next(1, pool.Where(u => u.position == "CB").Count()));
                var cb2 = pool.Where(u => u.position == "CB").ElementAt(r.Next(1, pool.Where(u => u.position == "CB").Count()));
                var rb = pool.Where(u => u.position == "RB" || u.position == "RWB").ElementAt(r.Next(1, pool.Where(u => u.position == "RB" || u.position == "RWB").Count()));

                Team t = new Team();
                t.players = new List<FutPlayers>();
                t.players.Add(st);
                t.players.Add(lw);
                t.players.Add(rw);
                t.players.Add(cm);
                t.players.Add(cm2);
                t.players.Add(cdm);
                t.players.Add(lb);
                t.players.Add(cb);
                t.players.Add(cb2);
                t.players.Add(rb);

                int cost = GetCost(st.ps4cost);
                cost += GetCost(lw.ps4cost);
                cost += GetCost(rw.ps4cost);
                cost += GetCost(cm.ps4cost);
                cost += GetCost(cm2.ps4cost);
                cost += GetCost(cdm.ps4cost);
                cost += GetCost(lb.ps4cost);
                cost += GetCost(cb.ps4cost);
                cost += GetCost(cb.ps4cost);
                cost += GetCost(rb.ps4cost);
                t.cost = cost;

                Roles role = new Roles();

                double rating = role.CalculateRole(st, role.StrikerQuick);
                rating += role.CalculateRole(lw, role.WingerQuick);
                rating += role.CalculateRole(rw, role.WingerQuick);
                rating += role.CalculateRole(cm, role.MidSkilled);
                rating += role.CalculateRole(cm2, role.MidSkilled);
                rating += role.CalculateRole(cdm, role.DefendingMidDefender);
                rating += role.CalculateRole(lb, role.BackQuick);
                rating += role.CalculateRole(cb, role.DefenderStrong);
                rating += role.CalculateRole(cb2, role.DefenderStrong);
                rating += role.CalculateRole(rb, role.BackQuick);
                t.rating = rating;

                if (cost < totalCost)
                {
                    teams.Add(t);
                }
                
            }

            teams = teams.OrderByDescending(u => u.rating).ToList();

            var bestTeam = teams.FirstOrDefault();
            leagueHighs.Add(bestTeam);
        }

        public class Team
        {
            public List<FutPlayers> players { get; set; }
            public int cost { get; set; }
            public double rating { get; set; }
        }

        public int GetCost(string cost)
        {
            int newCost;
            if (cost == null)
            {
                return 1000000000;
            }
            if (cost.Contains("M"))
            {
                cost = cost.Replace("M", "");
                newCost = Convert.ToInt32(Convert.ToDouble(cost) * 1000000);
            }
            else if (cost.Contains("K"))
            {
                cost = cost.Replace("K", "");
                newCost = Convert.ToInt32(Convert.ToDouble(cost) * 1000);
            }
            else
            {
                newCost = Convert.ToInt32(cost);
            }

            return newCost;
        }
    }

    
}