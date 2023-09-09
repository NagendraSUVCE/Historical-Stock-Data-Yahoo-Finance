using CoreUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.DS;
using YahooFinanceApi;

namespace StockRepository
{
    public class StockYahooRepo
    {
        public async Task<List<Candle>> getStockData(string symbol, DateTime startDate, DateTime endDate)
        {
            StringBuilder sb = new StringBuilder();
            string companyName = "";
            List<StockHistoryYahoo> lstStockDetails = new List<StockHistoryYahoo>();
            try
            {
                startDate = GetLastUpdatedDBDate(symbol);
                startDate = startDate.Date.AddDays(1);
                if (startDate < endDate)
                {
                    var historic_data = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate);
                    var security = await Yahoo.Symbols(symbol).Fields(Field.LongName).QueryAsync();
                    var ticker = security[symbol];
                    try
                    {
                        companyName = ticker[Field.LongName];
                    }
                    catch (Exception ex)
                    {

                    }
                    for (int i = 0; i < historic_data.Count; i++)
                    {
                        StockHistoryYahoo stock = new StockHistoryYahoo();
                        stock.StockTicker = symbol;
                        stock.StockDateTime = historic_data[i].DateTime;
                        stock.Open = historic_data[i].Open;
                        stock.High = historic_data[i].High;
                        stock.Low = historic_data[i].Low;
                        stock.ClosePrice = historic_data[i].Close;
                        stock.Volume = historic_data[i].Volume;
                        stock.AdjustedClose = historic_data[i].AdjustedClose;
                        lstStockDetails.Add(stock);
                    }
                    List<Candle> cndList = historic_data.ToList();
                    DataTable dt = DataSetUtilities.ToDataTable<StockHistoryYahoo>(lstStockDetails);
                    dt.TableName = "StockHistoryYahoo";
                    DBUtilities.ConnectionString = GetDBConnectionString();
                    //DBUtilities.DeleteTableFromDatabase(dt);
                    DBUtilities.InsertDatatableToDatabase(dt);
                    return cndList;
                }
                return null;
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to get symbol: " + symbol);
                sb.AppendLine("exception " + ex.InnerException);

                throw ex;
            }
            return null;
        }

        private DateTime GetLastUpdatedDBDate(string symbol)
        {
            DateTime lastDataUpdatedTime = new DateTime(2016, 01, 01);
            object obj = null;
            string query = string.Format(@"SELECT MAX(StockDateTime) FROM StockHistoryYahoo WHERE StockTicker='{0}';", symbol);
            try
            {

                obj = DBUtilities.GetSingleValue(GetDBConnectionString(), query);
            }
            catch (Exception)
            {
            }
            if (obj != null && obj != DBNull.Value)
            {
                lastDataUpdatedTime = (DateTime)obj;
            }
            return lastDataUpdatedTime;
        }
        private string GetDBConnectionString()
        {
            return "Server =tcp:learningsqldb.database.windows.net,1433;Initial Catalog=Learning;Persist Security Info=False;User ID=nagendra;Password=AzureLearning#1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;";
        }
    }
}

