using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;

namespace RateLoader
{
    class RateManager
    {
        public static void ReadRateFile(RateFile ratefile, Company Company)
        {
            SBObob bob = (SBObob)Company.GetBusinessObject(BoObjectTypes.BoBridge);
            foreach (Rate rate in ratefile.Rates.Where(r => Config.Currencies.Keys.Contains(r.Currency)))
            {
                bob.SetCurrencyRate(Config.Currencies[rate.Currency], ratefile.Date, rate.Value, true);
                Config.log(ratefile.Date.ToString("yyyy-MM-dd") + " " + rate.Currency + " = " + rate.Value);
            }
        }
    }
}
