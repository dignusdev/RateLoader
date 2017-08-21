using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace RateLoader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Config.InitConfiguration();
                if (!Config.InitSBOConnection())
                {
                    throw new Exception("No SBO connection!");
                }

                if (!Directory.Exists(Config.Configuration.RateFileLocation))
                {
                    throw new Exception("Directory " + Config.Configuration.RateFileLocation + " not exists!");
                }
                Config.debug("Reading files from " + Config.Configuration.RateFileLocation);

                WebClient webClient = new WebClient();
                webClient.DownloadFile("http://www.nbp.pl/kursy/xml/LastA.xml", Config.Configuration.RateFileLocation + string.Format("KursyNBP{0}.xml",DateTime.Now.ToString("yyyyMMdd")));

                foreach (string filename in Directory.EnumerateFiles(Config.Configuration.RateFileLocation))
                {
                    RateFile ratefile = new RateFile(filename,true);
                    RateManager.ReadRateFile(ratefile, Config.Company);
                    if (!Directory.Exists(Config.Configuration.RateFileStorage))
                    {
                        Directory.CreateDirectory(Config.Configuration.RateFileStorage);
                    }
                    //File.Move(filename, Config.Configuration.RateFileStorage + Path.GetFileName(filename));
                    //Config.debug("File " + filename + " moved to " + Config.Configuration.RateFileStorage + Path.GetFileName(filename));
                }
                List<String> MyMusicFiles = Directory.GetFiles(Config.Configuration.RateFileLocation, "*.*", SearchOption.TopDirectoryOnly).ToList();
                foreach (string file in MyMusicFiles)
                {
                    File.Move(file, Config.Configuration.RateFileStorage + Path.GetFileName(file));
                }
                
            }
            catch (Exception ex)
            {
                Config.error("Error: " + ex.Message + " - " + ex.StackTrace);
                //throw ex;
            }
        }
    }
}
