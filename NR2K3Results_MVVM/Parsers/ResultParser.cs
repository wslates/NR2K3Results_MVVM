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
                Console.WriteLine(driver);
            }
        }

        private static void ParsePracticeQual (ref List<Driver> drivers, ref List<string> finalResults, decimal length)
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




            }

            //in case some drivers were in the roster but not in the race, remove them
            drivers = drivers.Where(d => d.result != null).ToList();
        }

        private static void ParseRace(ref List<Driver> drivers, ref List<string> finalResults, decimal length)
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
    


