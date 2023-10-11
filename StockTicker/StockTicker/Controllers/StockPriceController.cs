using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using StockRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockTicker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockPriceController : ControllerBase
    {
        //https://localhost:44327/api/StockPrice?symbol=MSFT
        //https://localhost:44327/api/StockPrice?symbol=INR=X
        StockYahooRepo stockRepo = new StockYahooRepo();
        [Route("GetGivenTicker")]
        [HttpGet]
        public async Task<List<Candle>> Get(string symbol)
        {
            //MSFT
            //INR=X
            /*
             SBI Bluechip Fund Direct Growth (0P0000XVJQ.BO)
            ASIANPAINT.NS asian paints
            https://finance.yahoo.com/quote/ASIANPAINT.NS/history?period1=1025481600&period2=1643155200&interval=1d&filter=history&frequency=1d&includeAdjustedClose=true
             */
            var data = await stockRepo.getStockData(symbol, new DateTime(2016, 01, 01), DateTime.Now.Date.AddDays(-2));
            return data;
        }

        //https://localhost:44327/api/StockPrice/api/StockPrice/GetAll
        [Route("GetAll")]
        [HttpGet]
        public async Task<List<Candle>> UpdateDBAll()
        {
            List<string> lstStockTickers = new List<string>();
            lstStockTickers.Add("MSFT");
            lstStockTickers.Add("INR=X");
            lstStockTickers.Add("ASIANPAINT.NS");

            List<Candle> lstCandle = new List<Candle>();
            //MSFT
            //INR=X
            /*
             SBI Bluechip Fund Direct Growth (0P0000XVJQ.BO)
            ASIANPAINT.NS asian paints
            https://finance.yahoo.com/quote/ASIANPAINT.NS/history?period1=1025481600&period2=1643155200&interval=1d&filter=history&frequency=1d&includeAdjustedClose=true
             */
            foreach (var symbol in lstStockTickers)
            {
                var s= await stockRepo.getStockData(symbol, new DateTime(2016, 01, 01), DateTime.Now.Date.AddDays(-2));
                lstCandle.AddRange(s);

            }
            return lstCandle;
        }

        [Route("UpdateMutualFundFromAMFI")]
        [HttpGet]
        public async Task UpdateMutualFundFromAMFI()
        {
            await (new MutualFundAMFIRepo()).UpdateMutualFundsDailyNAVFromAMFI();

        }
    }
}
