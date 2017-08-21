using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml.Linq;

namespace RateLoader
{
    public class RateFile
    {
        public DateTime Date { get; set; }
        public List<Rate> Rates { get; set; }
        public RateFile(string filepath, bool xml = true)
        {
            Config.debug("Parsing file " + filepath + ".");
            XDocument doc = XDocument.Load(filepath);
            XElement root = doc.Element("tabela_kursow");
            string numerTabeli = root.Element("numer_tabeli").Value.ToString();
            string dataPublikacji = root.Element("data_publikacji").Value.ToString();
            //Date = DateTime.ParseExact(dataPublikacji,"yyyy-MM-dd", CultureInfo.InvariantCulture);
            //Date = Date.AddDays(1);
            Date = DateTime.Now;

            Rates = new List<Rate>();
            foreach (var item in root.Elements("pozycja"))
            {
                Rates.Add(new Rate { Currency = item.Element("kod_waluty").Value.ToString(), Value = double.Parse(item.Element("kurs_sredni").Value.ToString()) });
            }
            Config.debug("File " + filepath + " parsed.");
        }
        public RateFile(string filepath)
        {
            Config.debug("Parsing file " + filepath + ".");
            int counter = 0;
            string line;
            System.IO.StreamReader file =
               new System.IO.StreamReader(filepath);

            file.ReadLine(); //[KURSY_WALUT]
            line = file.ReadLine(); //DataKursu
            string[] split = line.Split('=');
            //Date = DateTime.ParseExact(split[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            //Date = Date.AddDays(1);
            Date = DateTime.Now;

            Rates = new List<Rate>();
            double newrate = 0;
            string newcurr = "";
            while ((line = file.ReadLine()) != null)
            {
                split = line.Split('=');

                newcurr = split[0];
                newrate = Double.Parse(split[1]);
                if (newcurr.Equals("HUF") || newcurr.Equals("JPY"))
                    newrate = newrate / 100;

                Rates.Add(
                    new Rate()
                    {
                        Currency = newcurr,
                        Value = newrate
                    }
                    );

                counter++;
            }

            file.Close();

            Config.debug("File " + filepath + " parsed.");
        }
    }
}
