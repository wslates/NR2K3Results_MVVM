using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace NR2K3Results_MVVM.Parsers
{
    class CarFileParser
    {
        /// <summary>
        /// Returns all drivers in the current roster file.
        /// 
        /// Only returns drivers that are enabled.
        /// 
        /// If multiple instances of a driver are enabled (i.e. different paint schemes), it will only read the first instance it encounters.
        /// </summary>
        /// 
        /// <param name="RosterFilePath">Path to the roster file.</param>
        /// <returns></returns>
        public static List<Driver> GetRosterDrivers(String NR2003, String RosterFilePath)
        {
            string[] lines = System.IO.File.ReadAllLines(RosterFilePath);
            List<Driver> drivers = new List<Driver>();
            String CarsFilePath = System.IO.Directory.GetParent(RosterFilePath).ToString();

            foreach (string line in lines)
            {
                //'+' indicates that the driver is enabled ('-') indicating they are disabled.
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
            drivers.AddRange(GetPlayerCars(CarsFilePath + "\\" , NR2003));
            return drivers;
        }
        
        /// <summary>
        /// Gets all the player's car data for each of their mods.
        /// </summary>
        /// <param name="modFolderPath">The path of the parent folder of the roster file.</param>
        /// <param name="NR2003">NR2003 directory</param>
        /// <returns></returns>
        private static List<Driver> GetPlayerCars(string modFolderPath, string NR2003)
        {
            //get all directories in the players folder
            string[] players = Directory.GetDirectories(NR2003 + "\\players");
            List<Driver> cars = new List<Driver>();
            foreach (string player in players)
            {
                List<Driver> playerCars = GetPlayerSingleMultiCars(modFolderPath, player);
                
                if (playerCars != null)
                {
                    using (StreamReader file = new StreamReader(player + "\\player.ini"))
                    {
                        string line;

                        //updates driver information with player's name
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] data = line.Split('=');
                            if (data[0].Trim().Equals("firstName"))
                            {
                                foreach (Driver driver in playerCars)
                                {
                                    
                                    driver.firstName = data[1].Split(';')[0].Trim();
                                }
                            }
                            else if (data[0].Trim().Equals("lastName"))
                            {
                                foreach (Driver driver in playerCars)
                                {
                                    driver.lastName = data[1].Split(';')[0].Trim();
                                    driver.isPlayer = true;
                                }
                                break;
                            }

                        }
                    }
                    cars.AddRange(playerCars);
                }
                
            }
            return cars;

        }

        /// <summary>
        /// Gets each of the player's car selections for each of their mods.
        /// </summary>
        /// <param name="modFolderPath">The path of the parent folder of the roster file.</param>
        /// <param name="NR2003">NR2003 directory</param>
        /// <returns></returns>
        private static List<Driver> GetPlayerSingleMultiCars(string modFolderPath, string player)
        {
            List<Driver> cars = new List<Driver>(2);

            using (StreamReader file = new StreamReader(player + "\\player.ini"))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    string[] data = line.Split('=');

                    if (data[0].Trim().Equals("SelectedCarFile") || data[0].Trim().Equals("SelectedCarFileMulti"))
                    {
                        try
                        {
                            cars.Add(OpenCarFile(modFolderPath + data[1].Split(';')[0].Trim()));
                        }
                        catch (IOException e)
                        {
                            //car of wrong mod type
                        }
                    }

                }

            }
            return cars;
        }

        /// <summary>
        /// Opens a car file and reads the relevant data.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
                    //team information is the last data point we need, so no reason to continue
                    break;
                }  
            }
            return driver;

        }
    }
}
