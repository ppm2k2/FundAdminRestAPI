using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FundAdminRestAPI.Common.Extentions;
using FundAdminRestAPI.Interfaces.BusinessLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FundAdminRestAPI.DataLayer.Repositories
{
    public class FundRepository : IFundRepository
    {
        public FundRepository() { }

        public List<FundDTO> GetFundPL()
        {
            var result = new RepetedResponse<FundListDTO>();
            List<FundListDTO> fundListResponse = new List<FundListDTO>();


            Console.WriteLine(string.Format("********************Crypto Portfolio : {0} *****************", DateTime.Now.ToString("h:mm:ss tt")));

            result = new RepetedResponse<FundListDTO>();
            //List<FundListDTO> currencyRates = new List<FundListDTO>();

            var url = "https://api.coinbase.com/v2/exchange-rates?currency=USD";
            PricingDTO currencyRates = GetRates<PricingDTO>(url);

            RatesDTO currencyRatesDTO = currencyRates.data;
            Dictionary<string, decimal> rates = currencyRatesDTO.Rates;
            //StreamReader reader = new(@"C:\Source\GIT\Repos\FundAdminRestAPI\FundAdminRestAPI.Console\Data\Portfolio.json");
            StreamReader reader = new(Constants.JSONURL);
            var json = reader.ReadToEnd();
            var jarray = JArray.Parse(json);
            List<FundDTO> Portfolios = new();
            decimal initialMV = 0;
            decimal currentMV = 0;
            decimal pl = 0;
            foreach (var item in jarray)
            {
                FundDTO portfolio = item.ToObject<FundDTO>();

                foreach (var secirity in portfolio.Securities)
                {
                    // Beginning of Portfolio Market Value..
                    initialMV += secirity.Quantity * secirity.AvgPrice;

                    switch (secirity.Name)
                    {
                        // (Quantity * (CurrentPrice - AvgPrice))
                        case "BTC":
                            currentMV += secirity.Quantity * (1 / rates[Constants.BTC_TICKER]); // Current BTC MV
                            pl += secirity.Quantity * (1 / rates["BTC"] - secirity.AvgPrice); // Current BTC PL
                            break;
                        case "ETH":
                            currentMV += secirity.Quantity * (1 / rates[Constants.ETH_TICKER]); // Current ETH MV
                            pl += secirity.Quantity * (1 / rates["ETH"] - secirity.AvgPrice);
                            break;
                        case "AAVE":
                            currentMV += secirity.Quantity * (1 / rates[Constants.AAVE_TICKER]); // Current AAVE MV
                            pl += secirity.Quantity * (1 / rates["AAVE"] - secirity.AvgPrice);
                            break;
                        case "SOL":
                            currentMV += secirity.Quantity * (1 / rates[Constants.SOL_TICKER]); // Current SOL MV
                            pl += secirity.Quantity * (1 / rates["AAVE"] - secirity.AvgPrice);
                            break;
                    }
                }
                // Beginning Portfolio MV
                portfolio.InitialMV = initialMV;
                portfolio.CurrentMV = currentMV;
                portfolio.PL = pl;
                portfolio.PLPercent = (currentMV - initialMV) / initialMV * 100;

                //Console.WriteLine(string.Format("Current DateTime : {0}", DateTime.Now.ToString("h:mm:ss tt")));
                Console.WriteLine(string.Format(" {0} Initial Market Value : ${1}, Current Market Value : ${2}, P/L : ${3}, P/L %: {4}%",
                    portfolio.Name, Math.Round(portfolio.InitialMV, 8), Math.Round(portfolio.CurrentMV, 8), Math.Round(portfolio.PL, 8), Math.Round(portfolio.PLPercent, 8)));

                Portfolios.Add(portfolio);
            }

            return Portfolios;
            /*result.Result = fundListResponse;
            result.ServiceReponse.IsSuccessful = true;
            return result;*/
        }

        private static T GetRates<T>(string url) where T : new()
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString(url);
                }
                catch (Exception) { }
                // if string with JSON data is not empty, deserialize it to class and return its instance 
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
        }
    }
}
