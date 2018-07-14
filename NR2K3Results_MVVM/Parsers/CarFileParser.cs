using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace NR2K3Results_MVVM.Parsers
{
    class CarFileParser
    {
        public static List<Driver> GetRosterDrivers(String RosterFilePath)
        {
            string[] lines = System.IO.File.ReadAllLines(RosterFilePath);
            List<Driver> drivers = new List<Driver>();
            String CarsFilePath = System.IO.Directory.GetParent(RosterFilePath).ToString();
            foreach (string line in lines)
            {
                if (line[0] == '+')
                {
                    //plus sign at beginning of every line, get rid of that and append to path
                    Driver driver = OpenCarFile(CarsFilePath + "\\" + line.Substring(1, line.Length - 1));

                    //won't get a driver multiple times if they have multiple paint schemes
                    if (!drivers.Contains(driver))
                    {
                        drivers.Add(driver);
                    }
                }               
            }
            return drivers;
        }
           

        private static Driver OpenCarFile(string path)
        {
            Driver driver = new Driver();

            StreamReader file = new StreamReader(path);
            string line;
            //parses data by splitting at equals sign, getting rid of variable name
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("car_number"))
                {
                    driver.number = line.Split('=')[1].Replace(" ", string.Empty);
                } else if (line.Contains("first_name"))
                {
                    driver.firstName = line.Split('=')[1];
                } else if (line.Contains("last_name"))
                {
                    driver.lastName = line.Split('=')[1];
                } else if (line.Contains("sponsor"))
                {
                    driver.sponsor = line.Split('=')[1]; 
                } else if (line.Contains("team"))
                {
                    driver.team = line.Split('=')[1];
                    break;
                }  
            }
            return driver;

        }
    }
}
