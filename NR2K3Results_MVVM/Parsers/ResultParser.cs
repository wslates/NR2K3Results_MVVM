using HtmlAgilityPack;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace NR2K3Results_MVVM.Parsers
{
    /// <summary>
    /// Contains methods necessary to parse a result file.
    /// </summary>
    class ResultParser
    {
        /// <summary>
        /// 4 possible sessions that can be run.
        /// </summary>
        private static readonly List<String> sessions = new List<string> { "Practice", "Qualifying", "Happy Hour", "Race" };
        
        /// <summary>
        /// Gets the sessions and updates "sessions" collection with the sessions that were ran in this result file.
        /// </summary>
        /// <param name="filePath">Filepath to result files.</param>
        /// <param name="sessios">Sessions that were ran in this result file.</param>
        public static void GetSessions(String filePath, ObservableCollection<String> session)
        {
            var doc = new HtmlDocument();
            doc.Load(@filePath);

            //selects all tables in the document
            var tables = doc.DocumentNode.SelectNodes("//table");

            foreach (var table in tables)
            {
                //checks to see if this table has more than one row, which would indicate there are results.
                if (table.ChildNodes.Where(d=>d.Name.ToLower().Equals("tr")).Count() > 1)
                {
                    session.Add(sessions[tables.IndexOf(table)]);
                }                
            }
        }

        /// <summary>
        /// Parses result file and updates drivers list accordingly.
        /// </summary>
        /// <param name="drivers">List of current drivers. Will be updated with each driver's result from this session.</param>
        /// <param name="filePath">Filepath pointing towards result file.</param>
        /// <param name="session">Session type, i.e. Practice, Qualifying, Happy Hour, Race</param>
        /// <param name="race">Race data. Will be updated with caution and leader data for the race.</param>
        public static void Parse(ref List<Driver> drivers, string filePath, string session, ref Race race)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@filePath);

            var table = doc.DocumentNode.SelectNodes("//table").ElementAt(sessions.IndexOf(session)).ChildNodes.Where(d => d.Name.Equals("tr")).ToList();

            if (session.Equals("Race"))
            {
                ParseRace(ref drivers, table, ref race);
                GetRaceData(filePath, ref race);
            }
            else
            {
                ParsePracQual(ref drivers, table, race.length);
            }
        }

        /// <summary>
        /// Gets race data such as the number of leaders/time and the number of cautions/laps.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="track">Reference to instance of track. </param>
        private static void GetRaceData(string filePath, ref Race track)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            //each of the race data is in a h3 header
            var headers = doc.DocumentNode.SelectNodes("//h3");

            foreach (var header in headers)
            {
                if (header.InnerText.Contains("Caution Flags"))
                {
                    /* Long linq query, but...
                     * 
                     */
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

        /// <summary>
        /// Parses practice or qualifying results from a result file and updates drivers with a driver result for the session.
        /// </summary>
        /// <param name="drivers">Reference to the current list of drivers.</param>
        /// <param name="table">The table containing the race results.</param>
        /// <param name="race">Reference to the current race.</param>
        private static void ParsePracQual (ref List<Driver> drivers, List<HtmlNode> table, decimal length)
        {
            //remove first row of table with column names
            table.RemoveAt(0);
            
            //fastest time is now last cell of first row
            TimeSpan fastTime = ParseTime(table[0].ChildNodes.Where(d => d.Name.Equals("td")).Last().InnerText.Trim(), false);
            TimeSpan prevTime = fastTime;

            foreach (var row in table)
            {
                /* get the cells in this row, which can be defined as:
                 *      cells[0] = finishing position
                 *      cells[1] = driver number
                 *      cells[2] = driver name
                 *      cells[3] = lap time
                 */ 
                var cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t=>t.InnerText.Trim()).ToList();

                //note: "--" for time means that driver did not make a lap in the session, thus time is set to 0

                TimeSpan thisTime = (cells[3].Equals("--")) ? new TimeSpan() : ParseTime(cells[3], false);
                
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

        /// <summary>
        /// Parses race results from a result file and updates drivers with a driver result for the session.
        /// </summary>
        /// <param name="drivers">Reference to the current list of drivers.</param>
        /// <param name="table">The table containing the race results.</param>
        /// <param name="race">Reference to the current race.</param>
        private static void ParseRace (ref List<Driver> drivers, List<HtmlNode> table, ref Race race)
        {
            //remove first row of table with column names
            table.RemoveAt(0);

            //gets leader's cell to get their laps and avg. speed
            var cells = table[0].ChildNodes.Where(d => d.Name.Equals("td")).Select(t => t.InnerText.Trim()).ToList();
            race.laps = Convert.ToInt16(cells[5]);
            race.speed = Convert.ToDecimal(cells[4]);

            foreach (var row in table)
            {
                /* get the cells in this row, which can be defined as:
                 *      cells[0] = finishing position
                 *      cells[1] = starting position
                 *      cells[2] = driver number
                 *      cells[3] = driver name
                 *      cells[4] = interval
                 *      cells[5] = laps completed
                 *      cells[6] = laps led
                 *      cells[7] = IGNORE
                 *      cells[8] = status
                */
                cells = row.ChildNodes.Where(d => d.Name.Equals("td")).Select(t => t.InnerText.Trim()).ToList();

                DriverResult driverResult = new DriverResult
                {
                    finish = Convert.ToInt16(cells[0]),
                    start = Convert.ToInt16(cells[1]),
                    timeOffLeader = (cells[4].Contains('L')) ? new TimeSpan() : ParseTime(cells[4], true),
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

        /// <summary>
        /// Converts a string containing a time to a TimeSpan object.
        /// </summary>
        /// <param name="time">String containg the time.</param>
        /// <param name="isRace">True if this time is from a race, false if it is from a practice or qualifying session.</param>
        /// <returns></returns>
        private static TimeSpan ParseTime(string time, bool isRace)
        {
            int minutes = 0;
            time = time.Replace("-", String.Empty);

            /* split the time across the decimal point, this will result in one of two ways:
             * The time has minutes, then data[0] will be minutes:seconds and data[1] will be milliseconds
             * Time does not have minutes, then data[0] will be seconds and data[1] will be milliseconds
             */ 
            var data = time.Split('.');

            
            if (time.Contains(":"))
            {
                //get the number of minutes
                minutes = Convert.ToInt16(data[0].Split(':')[0]);
                
                //set data[0] to the number of seconds
                data[0] = data[0].Split(':')[1];
            }

            /*
            * Race times only have a precision of two decimal places rather than three, so when setting the milliseconds, you must multiply them by 10
            *      Ex: 24.56 (race) vs 24.056 (practice) - parsing the data will read these as the same values
            */
            return new TimeSpan(0, 0, minutes, Convert.ToInt16(data[0]), (isRace) ? Convert.ToInt16(data[1]) * 10 : Convert.ToInt16(data[1]));
            
        }
    }
}
    


