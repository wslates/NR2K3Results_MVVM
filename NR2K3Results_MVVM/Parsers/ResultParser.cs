using HtmlAgilityPack;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public static void Parse(ref List<Driver> drivers, string filePath, string session, decimal length)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@filePath);

            var table = doc.DocumentNode.SelectNodes("//table").ElementAt(sessions.IndexOf(session)).ChildNodes.Where(d => d.Name.Equals("tr")).ToList();

            if (session.Equals("Race"))
            {
                ParseRace(ref drivers, table, length);
            }
            else
            {
                ParsePracQual(ref drivers, table, length);
            }
        }

        private static void ParsePracQual (ref List<Driver> drivers, List<HtmlNode> table, decimal length)
        {
            //remove first row of table with column names
            table.RemoveAt(0);

            //fastest time is now last cell of first row
            decimal fastTime = Convert.ToDecimal(table[0].ChildNodes.Where(d => d.Name.Equals("td")).Last().InnerText.Trim());
            decimal prevTime = fastTime;

            foreach (var row in table)
            {
                var cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t=>t.InnerText.Trim()).ToList();
             
                //note: "--" for time means that driver did not make a lap in the session, thus time is set to 0
                DriverResult driverResult = new DriverResult
                {
                    finish = Convert.ToInt16(cells[0]),
                    time = (cells[3].Equals("--")) ? 0 : Convert.ToDecimal(cells[3]),
                    timeOffLeader = (cells[3].Equals("--")) ? 0 : fastTime - Convert.ToDecimal(cells[3]),
                    timeOffNext = (cells[3].Equals("--")) ? 0 : prevTime - Convert.ToDecimal(cells[3]),
                    speed = (cells[3].Equals("--")) ? 0 : (length / Convert.ToDecimal(cells[3])) * 3600

                };



                Driver driver = new Driver
                {
                    number = cells[1],
                    firstName = cells[2][0].ToString(),
                    lastName = cells[2].Substring(2, cells[2].Length - 2),
                    result = driverResult
                };

                //set previous time to 0 
                prevTime = (cells[3].Equals("--")) ? 0 : Convert.ToDecimal(cells[3]);
                
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

        private static void ParseRace (ref List<Driver> drivers, List<HtmlNode> table, decimal length)
        {
            //remove first row of table with column names
            table.RemoveAt(0);

            foreach (var row in table)
            {
                var cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t => t.InnerText.Trim()).ToList();

                DriverResult driverResult = new DriverResult
                {
                    finish = Convert.ToInt16(cells[0]),
                    start = Convert.ToInt16(cells[1]),
                    timeOffLeader = (cells[4].Contains('L')) ? 0 : Convert.ToDecimal(cells[4]),
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
    }
}
    


