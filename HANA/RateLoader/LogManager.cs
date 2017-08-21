using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace RateLoader
{
    public class LogManager
    {
        static LogManager()
        {
            var confFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if (System.IO.File.Exists(confFile))
            {
                var fi = new System.IO.FileInfo(confFile);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(fi.OpenRead());
                XmlNode node = xdoc.SelectSingleNode("/configuration/log4net");
                if (node != null)
                {
                    log4net.Config.XmlConfigurator.Configure(fi);
                }
            }

            log4net.Repository.Hierarchy.Hierarchy hier =
              log4net.LogManager.GetLoggerRepository() as log4net.Repository.Hierarchy.Hierarchy;
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <returns></returns>
        public static log4net.ILog GetLog()
        {
            Type t = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            log4net.ILog l = log4net.LogManager.GetLogger(t);
            return l;
        }
    }
}
