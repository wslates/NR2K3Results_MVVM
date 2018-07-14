using HtmlAgilityPack;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NR2K3Results_MVVM.Parsers
{
    class ResultParser
    {
        private static readonly List<String> sessions = new List<string> { "Practice", "Qualifying", "Happy Hour", "Race" };

        public static void GetSessions(String filePath, ObservableCollection<String> retSessions)
        {
            var doc = new HtmlDocument();
            doc.Load(@filePath);
            var tables = doc.DocumentNode.SelectNodes("//table");
            foreach (var table in tables)
            {
                if (table.ChildNodes.Where(d=>d.Name.ToLower().Equals("tr")).Count() > 1)
                {
                    retSessions.Add(sessions[tables.IndexOf(table)]);
                }                
            }
        }

        public static void Parse(ref List<Driver> drivers, string filePath, string session, ref Race track)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@filePath);

            var table = doc.DocumentNode.SelectNodes("//table").ElementAt(sessions.IndexOf(session)).ChildNodes.Where(d => d.Name.Equals("tr")).ToList();

            if (session.Equals("Race"))
            {
                ParseRace(ref drivers, table, ref track);
                GetRaceData(filePath, ref track);
            }
            else
            {
                ParsePracQual(ref drivers, table, track.length);
            }
        }

        private static void GetRaceData(string filePath, ref Race track)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            var headers = doc.DocumentNode.SelectNodes("//h3");
            foreach (var header in headers)
            {
                if (header.InnerText.Contains("Caution Flags"))
                {
                    var data = new string(header.InnerText.Trim().Where(c => char.IsDigit(c) || char.IsWhiteSpace(c)).ToArray()).Split(' ').Where(d=> !String.IsNullOrWhiteSpace(d)).ToArray();
                    track.cautions = Convert.ToInt16(data[0]);
                    track.cautionLaps = Convert.ToInt16(data[1]);
                }
                else if (header.InnerText.Contains("Lead Changes"))
                {
                    var data = new string(header.InnerText.Trim().Where(c => char.IsDigit(c) || char.IsWhiteSpace(c)).ToArray()).Split(' ').Where(d => !String.IsNullOrWhiteSpace(d)).ToArray();
                    track.leadChanges = Convert.ToInt16(data[0].Trim());
                    track.leaders = Convert.ToInt16(data[1].Trim());
                }
            }
        }

        private static void ParsePracQual (ref List<Driver> drivers, List<HtmlNode> table, decimal length)
        {
            //remove first row of table with column names
            table.RemoveAt(0);
            
            //fastest time is now last cell of first row
            TimeSpan fastTime = ParseTime(table[0].ChildNodes.Where(d => d.Name.Equals("td")).Last().InnerText.Trim());
            TimeSpan prevTime = fastTime;

            foreach (var row in table)
            {
                var cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t=>t.InnerText.Trim()).ToList();
                TimeSpan thisTime = (cells[3].Equals("--")) ? new TimeSpan() : ParseTime(cells[3]);
                //note: "--" for time means that driver did not make a lap in the session, thus time is set to 0
                DriverResult driverResult = new DriverResult
                {
                    finish = Convert.ToInt16(cells[0]),
                    time = thisTime,
                    timeOffLeader = (cells[3].Equals("--")) ? new TimeSpan(): thisTime.Subtract(fastTime),
                    timeOffNext = (cells[3].Equals("--")) ? new TimeSpan() : thisTime.Subtract(prevTime),
                    speed = (cells[3].Equals("--")) ? 0 : (length / (decimal)thisTime.TotalMilliseconds) * 1000 * 3600

                };



                Driver driver = new Driver
                {
                    number = cells[1],
                    firstName = cells[2][0].ToString(),
                    lastName = cells[2].Substring(2, cells[2].Length - 2),
                    result = driverResult
                };
                

                //set previous time to 0 
                prevTime = (cells[3].Equals("--")) ? new TimeSpan() : thisTime;
                
                if (drivers.Contains(driver))
                {
                    // simply update the result if a driver already exists in the list
                    drivers.ElementAt(drivers.IndexOf(driver)).result = driverResult;
                } else
                {
                    drivers.Add(driver);
                }

            }

            //in case some drivers were in the roster but not in the race, remove them
            drivers = drivers.Where(d => d.result != null).ToList();
        }

        private static void ParseRace (ref List<Driver> drivers, List<HtmlNode> table, ref Race track)
        {
            //remove first row of table with column names
            table.RemoveAt(0);
            var cells = table[0].ChildNodes.Where(d => d.Name.Equals("td")).Select(t => t.InnerText.Trim()).ToList();
            track.laps = Convert.ToInt16(cells[5]);
            track.speed = Convert.ToDecimal(cells[4]);
            foreach (var row in table)
            {
                cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t => t.InnerText.Trim()).ToList();
                DriverResult driverResult = new DriverResult
                {
                    finish = Convert.ToInt16(cells[0]),
                    start = Convert.ToInt16(cells[1]),
                    timeOffLeader = (cells[4].Contains('L')) ? new TimeSpan() : ParseTime(cells[4]+"R"),
                    lapsDown = (cells[4].Contains('L')) ? Convert.ToInt16(cells[4].Replace("L", String.Empty)):0,
                    laps = Convert.ToInt16(cells[5]),
                    lapsLed = (cells[6].Contains('*')) ? Convert.ToInt16(cells[6].Replace("*", String.Empty)) : Convert.ToInt16(cells[6]),
                    status = cells[8]
                };

                Driver driver = new Driver
                {
                    number = cells[2],
                    firstName = cells[3][0].ToString(),
                    lastName = cells[3].Substring(2, cells[3].Length - 2),
                    result = driverResult
                };
                Console.WriteLine(driver.result.timeOffLeader.TotalSeconds);

                if (drivers.Contains(driver))
                {
                    // simply update the result if a driver already exists in the list
                    drivers.ElementAt(drivers.IndexOf(driver)).result = driverResult;
                }
                else
                {
                    drivers.Add(driver);
                }
                
            }
            //in case some drivers were in the roster but not in the race, remove them
            drivers = drivers.Where(d => d.result != null).ToList();
            
        }

        private static TimeSpan ParseTime(string time)
        {
            int minutes = 0;
            bool race = time.Contains("R");
            time = time.Replace("-", String.Empty);
            time = time.Replace("R", String.Empty);
            var data = time.Split('.');

            if (time.Contains(":"))
            {
                minutes = Convert.ToInt16(time.Split(':')[0]);
                data = time.Split(':')[1].Split('.');
            }
            
            
            return new TimeSpan(0, 0, minutes, Convert.ToInt16(data[0]), (race) ? Convert.ToInt16(data[1]) * 10 : Convert.ToInt16(data[1]));
            
        }
    }
}
    


