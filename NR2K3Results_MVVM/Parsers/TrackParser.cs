using NR2K3Results_MVVM.Model;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
namespace NR2K3Results_MVVM.Parsers
{
    class TrackParser
    {
        /// <summary>
        /// Using the result file, gets the all the track's data.
        /// </summary>
        /// <param name="NR2003Dir">NR2003 root directory.</param>
        /// <param name="filepath">File Path to result file.</param>
        /// <returns></returns>
        public static Race Parse(String NR2003Dir, String filepath)
        {
            Race retTrack = new Race();
            string line;

            StreamReader file = new StreamReader(filepath);

            while ((line = file.ReadLine()) != null)
            {
                //get the track name from the result file
                if (line.Contains("Track: "))
                {
                    retTrack.name = line.Split(':')[1].Trim();
                    break;
                }
            }

            //gets all directories in the track folder
            string[] tracks = Directory.GetDirectories(NR2003Dir + "\\tracks");
            bool trackFound = false;
          
            foreach (string track in tracks)
            {
                try
                {
                    file = new StreamReader(track + "\\track.ini");

                    while ((line = file.ReadLine()) != null)
                    {
                        string[] splitLine = line.Split('=');
                        if (splitLine[0].Trim().Equals("track_name"))
                        {
                            string trackName = splitLine[1].Trim();

                            //if this is not the track we want, move on to the next folder
                            if (!trackName.Equals(retTrack.name))   
                                break;
                            else
                                trackFound = true;    
                        }
                        else if (splitLine[0].Trim().Equals("track_length"))
                        {
                            string length = splitLine[1];
                            //contains an m and sometimes a space, so just remove anything that is not a number or decimal point
                            length = Regex.Replace(length, "[^0-9.]", "");
                            retTrack.length = Convert.ToDecimal(length);

                            //this is the last data point we'll need, so let's stop reading the file
                            break;
                        }
                        else if (splitLine[0].Trim().Equals("track_length_n_type"))
                        {
                            retTrack.description = splitLine[1];
                        }
                        else if (splitLine[0].Trim().Equals("track_city"))
                        {
                            retTrack.city = splitLine[1];
                        }
                        else if (splitLine[0].Trim().Equals("track_state"))
                        {
                            retTrack.state += splitLine[1];
                        }
                    }
                    //if we found the track, we should stop there
                    if (trackFound)
                        break;
                }
                catch (IOException e)
                {
                    //most likely means we hit a folder without a track.ini file, such as the "shared" folder
                    continue;
                }

            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            retTrack.name = textInfo.ToTitleCase(retTrack.name.ToLower());

            return retTrack;
        }
    }
}
