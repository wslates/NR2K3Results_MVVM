using HtmlAgilityPack;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NR2K3Results_MVVM.Parsers
{
    class ResultParser
    {
        private static Dictionary<string, int> sessionTypes = new Dictionary<string, int>()
            {
                {"Practice", 0},
                {"Qualifying", 1},
                {"Happy Hour", 2},
                {"Race", 3}
            };

        public static void Parse(ref List<Driver> drivers, string FilePath, ref string session, ref decimal length)
        {
            bool[] ranSession = new bool[4];

            //C:\Papyrus\NASCAR Racing 2003 Season\exports_imports\HappyHour.Html

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@"C:\Papyrus\NASCAR Racing 2003 Season\exports_imports\Charlotte.html");

            var tables = doc.DocumentNode.SelectNodes("//table");
            var sessions = doc.DocumentNode.SelectNodes("//h3").Where(x => x.InnerText.Contains("Session"));
            for (int i = 0; i < sessions.Count(); i++)
            {
                if (tables[i].SelectNodes("tr").Count() > 1)
                {
                    string sess = sessions.ElementAt(i).InnerText.Split(':')[1].Trim();
                    ranSession[sessionTypes[sess]] = true;
                }
            }


            //parse the results
            List<string> finalResults = new List<string>();
          
            if (ranSession[sessionTypes[session]])
            {
                foreach (HtmlNode row in tables.ElementAt(sessionTypes[session]).SelectNodes("tr").Skip(1))
                {
                    foreach (HtmlNode cell in row.SelectNodes("td"))
                    {
                        finalResults.Add(cell.InnerText.Trim());
                    }
                }

                if (session.Equals("Race"))
                {
                    ParseRace(ref drivers, ref finalResults, ref length);
                } else
                {
                    ParsePracticeQual(ref drivers, ref finalResults, ref length);
                }
            }
           
            


    
        }

        private static void ParsePracticeQual (ref List<Driver> drivers, ref List<string> finalResults, ref decimal length)
        {
            decimal fastTime = Convert.ToDecimal(finalResults.GetRange(0, 4)[3]);
            decimal prevTime = fastTime;
            for (int i = 0; i < finalResults.Count - 3; i += 4)
            {

                string[] result = finalResults.GetRange(i, 4).ToArray();
                DriverResult driverRes = new DriverResult
                {
                    finish = Convert.ToInt16(result[0]),
                    time = (result[3].Equals("--")) ? 0 : Convert.ToDecimal(result[3]),
                    timeOffLeader = (result[3].Equals("--")) ? 0 : fastTime - Convert.ToDecimal(result[3]),
                    timeOffNext = (result[3].Equals("--")) ? 0 : prevTime - Convert.ToDecimal(result[3]),
                    speed = (result[3].Equals("--")) ? 0 : (length / Convert.ToDecimal(result[3])) * 3600
                };


                string[] name = result[2].Split(' ');

                Driver driver = new Driver
                {
                    number = result[1],
                    firstName = result[2][0].ToString(),
                    lastName = result[2].Substring(2, result[2].Length - 2),
                    result = driverRes
                };

                prevTime = (result[3].Equals("--")) ? 0 : Convert.ToDecimal(result[3]);


                if (drivers.Contains(driver))
                {
                    drivers[drivers.IndexOf(driver)].result = driverRes;
                } else
                {
                    
                }


            }

            //in case some drivers were in the roster but not in the race, remove them
            drivers = drivers.Where(d => d.result != null).ToList();
        }

        private static void ParseRace(ref List<Driver> drivers, ref List<string> finalResults, ref decimal length)
        {
            //decimal fastTime = Convert.ToDecimal(finalResults.GetRange(0, 9)[4]);
            for (int i = 0; i < finalResults.Count - 8; i += 9)
            {

                string[] result = finalResults.GetRange(i, 9).ToArray();

                if(result[6].Contains('*'))
                {
                    result[6] = result[6].Replace("*", String.Empty);
                }
                DriverResult driverRes = new DriverResult
                {
                    finish = Convert.ToInt16(result[0]),
                    start = Convert.ToInt16(result[1]),
                    timeOffLeader = (result[4].Contains('L')) ? .000001m : Convert.ToDecimal(result[4]),
                    lapsDown = (result[4].Contains('L')) ? result[4].Replace("L", String.Empty) : String.Empty,
                    laps = Convert.ToInt16(result[5]),
                    lapsLed = Convert.ToInt16(result[6]),
                    status = result[8]
                };

                string[] name = result[2].Split(' ');

                Driver driver = new Driver
                {
                    number = result[2],
                    firstName = result[3][0].ToString(),
                    lastName = result[3].Substring(2, result[3].Length - 2),
                    result = driverRes
                };

                if (drivers.Contains(driver))
                {
                    drivers[drivers.IndexOf(driver)].result = driverRes;
                }

            }

            //in case some drivers were in the roster but not in the race, remove them
            drivers = drivers.Where(d => d.result != null).ToList();
        }


    }
}
    


