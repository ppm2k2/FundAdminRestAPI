using FundAdminRestAPI.BusinessLogic;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MyApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10)); 

            while (await timer.WaitForNextTickAsync())
            {
                FundAdminBL fileAdminBL  = new FundAdminBL();
                _ = fileAdminBL.GetFundPL();
            }
        }
    }
}