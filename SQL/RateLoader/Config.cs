using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace RateLoader
{
    public class Config
    {
        /// <summary>
        /// Klasa reprezentująca dane połączenia z serwerem bazy danych SAP BO.
        /// </summary>
        [Serializable]
        public class Polaczenie
        {
            public string ServerAddress { get; set; } //adres serwera
            public string DBName { get; set; } // nazwa bazy danych
            public string UseTrusted { get; set; }
            public string DBUserName { get; set; } //uzytkownik bazy danych
            public string DBPassword { get; set; } //haslo bazy danych
            public string SAPUserName { get; set; } //login do sapa
            public string SAPUserPassword { get; set; } // haslo do sapa
            public string LicenseServer { get; set; } // adres serwera licencji
            public string DBServerType { get; set; }
            public string RateFileLocation { get; set; }
            public string RateFileStorage { get; set; }
        }

        public static Polaczenie Configuration {get; set; }
        public static Company Company { get; set; }
        public static Dictionary<string, string> Currencies { get; set; }

        public static void InitConfiguration()
        {
            string sciezkaPlikuKonfiguracyjnego;
            if ((sciezkaPlikuKonfiguracyjnego = System.Reflection.Assembly.GetExecutingAssembly().Location + ".config")
                    != null)
            {
                ConfigXmlDocument conf;
                conf = new ConfigXmlDocument();
                conf.Load(sciezkaPlikuKonfiguracyjnego);

                XmlNode xmlPolaczenie = conf.SelectSingleNode("configuration/Polaczenie");
                XmlSerializer serializer = new XmlSerializer(typeof(Polaczenie));
                Configuration = (Polaczenie)serializer.Deserialize(new StringReader(xmlPolaczenie.OuterXml));
                debug("Configuration initialized.");
            }
        }

        public static bool InitSBOConnection()
        {
            try
            {
                if (Configuration != null)
                {
                    if (Company != null && Company.Connected)
                    {
                        return true;
                    }
                    //rodzic.status = false;
                    //rodzic.info += "Próbuję utworzyć połączenie z bazą..." + Environment.NewLine;
                    debug("Connecting to " + Configuration.DBName + " SAP BO database...");
                    Company = new SAPbobsCOM.Company();
                    Company.CompanyDB = Configuration.DBName;
                    Company.language = BoSuppLangs.ln_Polish;
                    Company.UseTrusted = Configuration.UseTrusted.ToLower() == "true";
                    if (!Company.UseTrusted)
                    {
                        Company.DbUserName = Configuration.DBUserName;
                        Company.DbPassword = Configuration.DBPassword;
                    }
                    //HANA
                    //Company.UseTrusted = false;
                    //Company.DbUserName = Configuration.DBUserName;
                    //Company.DbPassword = Configuration.DBPassword;
                    //KONIEC HANA
                    Company.Server = Configuration.ServerAddress;
                    Company.UserName = Configuration.SAPUserName;
                    Company.Password = Configuration.SAPUserPassword;
                    Company.LicenseServer = Configuration.LicenseServer;
                    Company.language = BoSuppLangs.ln_English;
                    switch (Configuration.DBServerType)
                    {
                        case "2005": Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                            break;
                        case "2008": Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                            break;
                        case "2012": Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                            break;
                        case "2014": Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                            break;
                        case "HANA": Company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
                            break;
                    }
                    if (Company.Connect() == 0)
                    {
                        debug("Connected to " + Configuration.DBName);

                        Recordset rs = (Recordset)Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                        rs.DoQuery("select ISOCurrCod, CurrCode from OCRN");
                        //rs.DoQuery("SELECT \"ISOCurrCod\", \"CurrCode\" FROM OCRN");

                        Currencies = new Dictionary<string, string>();
                        for (int i = 0; i < rs.RecordCount; i++)
                        {
                            Currencies.Add(rs.Fields.Item(0).Value, rs.Fields.Item(1).Value);
                            rs.MoveNext();
                        }

                        Marshal.FinalReleaseComObject(rs);
                        debug("Got currencies from SBO.");

                        return true;
                    }
                    error(Configuration.DBName + ": " + Company.GetLastErrorDescription());
                }
                return false;
            }
            catch (Exception e)
            {
                error(e.Message);
                throw e;
                //return false;
            }
        }


        public static void log(object message)
        {
            LogManager.GetLog().Info(message);
        }

        public static void error(object message)
        {
            LogManager.GetLog().Error(message);
        }

        public static void debug(object message)
        {
            LogManager.GetLog().Debug(message);
        }
    }
}
