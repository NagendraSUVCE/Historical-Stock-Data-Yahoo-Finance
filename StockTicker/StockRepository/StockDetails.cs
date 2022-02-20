using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockRepository
{
    public class StockHistoryYahoo
    {
        public string StockTicker { get; set; }
        public DateTime StockDateTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal ClosePrice { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
    }
}
