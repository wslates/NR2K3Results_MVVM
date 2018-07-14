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
        public static Race Parse(String NR2003Dir, String filepath)
        {
            Race retTrack = new Race();
            string line;
            StreamReader file = new StreamReader(filepath);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("Track: "))
                {
                    retTrack.name = line.Split(':')[1].Trim();
                    break;
                }
            }

            //gets all directories in that folder, i.e. the tracks
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
                            string trackName = new string(splitLine[1].Trim().Where(c => !Char.IsSymbol(c)).ToArray());
                            
                            //if this is not the track we want, move on to the next folder
                            if (!trackName.Equals(retTrack.name))
                            {
                                break;
                            }
                            else
                                trackFound = true;    
                        }
                        else if (splitLine[0].Trim().Equals("track_length"))
                        {
                            string length = splitLine[1];
                            length = Regex.Replace(length, "[^0-9.]", "");
                            retTrack.length = Convert.ToDecimal(length);
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
                    //just means we hit a folder without a track.ini file, such as the "shared" folder
                    continue;
                }

            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            retTrack.name = textInfo.ToTitleCase(retTrack.name.ToLower());

            return retTrack;
        }
    }
}
