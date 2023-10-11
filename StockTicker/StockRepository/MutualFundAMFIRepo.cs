using CoreUtility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility.DS;

namespace StockRepository
{
    public class MutualFundAMFIRepo
    {
        // get data from AMFI API. Genarate c# class and store all values.  save it to database
        public async Task UpdateMutualFundsDailyNAVFromAMFI()
        {
            /*
              WHEN StockName ='HDFC Hybrid Equity Fund-Dir-Growth-(formerly HDFC Premier Multi-Cap Fund, erstwhile HDFC Balanced Fund merged)'	 THEN 	119062
               WHEN StockName ='HDFC Hybrid Equity Fund-Growth-(formerly HDFC Premier Multi-Cap Fund, erstwhile HDFC Balanced Fund merged)'	 THEN 	102948
               WHEN StockName ='HDFC Liquid Fund-Growth'	 THEN 	100868
               WHEN StockName ='HDFC Mid-Cap Opportunities Fund-DG'	 THEN 	118989
               WHEN StockName ='Bluechip Fund - Direct Plan Growth-(formerly ICICI Prudential Focused Bluechip Equity Fund)'	 THEN 	120586
               WHEN StockName ='Bluechip Fund - Growth-(formerly ICICI Prudential Focused Bluechip Equity Fund)'	 THEN 	108466
               WHEN StockName ='Liquid Fund - Growth-(formerly ICICI Prudential Liquid Plan)'	 THEN 	103340
               WHEN StockName ='Value Discovery Fund - DP Growth'	 THEN 	120323
               WHEN StockName ='Value Discovery Fund - Growth'	 THEN 	102594
               WHEN StockName ='SBI Blue Chip Fund Dir Plan-G'	 THEN 	119598
               WHEN StockName ='SBI Blue Chip Fund Reg Plan-G'	 THEN 	103504
               WHEN StockName ='HDFC Index Fund-NIFTY 50 Plan-Dir-(formerly HDFC Index Fund  - Nifty Plan)' THEN 119063
               WHEN StockName ='HDFC Liquid-DP-Growth Option' THEN 119091
             */
            //string fundidstring = "100868";
            List<String> findidsstring = new List<string>() { "100868","102594",
                "102948",
                "103340",
                "103504",
                "108466",
                "118989",
                "119062",
                "119598",
                "120323",
                "120586","119063","119091"

            };

            bool bDeleteFirst = true;
            List<RootObject> lstRootObject = new List<RootObject>();
            List<FundMetaData> lstFundMetaData = new List<FundMetaData>();
            List<NAVData> lstNAVData = new List<NAVData>();

            CultureInfo provider = CultureInfo.InvariantCulture;
            DataSet ds = new DataSet();
            foreach (var fundidstring in findidsstring)
            {
                RootObject navDataOfFund = GetNAVGivenFundId(fundidstring);

                lstRootObject.Add(navDataOfFund);
                lstFundMetaData.Add(navDataOfFund.fundData);
                foreach (var item in navDataOfFund.navListData)
                {
                    item.fundid = fundidstring;
                    item.dateNav = DateTime.ParseExact(item.date, "dd-MM-yyyy", provider);
                }
                lstNAVData.AddRange(navDataOfFund.navListData);
            }
            DBUtilities.ConnectionString = GetDBConnectionString();
            ds.Tables.Add(DataSetUtilities.ToDataTable<FundMetaData>(lstFundMetaData));
            ds.Tables.Add(DataSetUtilities.ToDataTable<NAVData>(lstNAVData));
            DBUtilities.DeleteTableFromDatabase(ds.Tables[0]);

            DBUtilities.DeleteTableFromDatabase(ds.Tables[1]); ;
            DBUtilities.InsertDatatableToDatabase(ds.Tables[0]);
            DBUtilities.InsertDatatableToDatabase(ds.Tables[1]);
        }

        public RootObject GetNAVGivenFundId(string fundId)
        {
            string url = "https://www.amfiindia.com/spages/NAVAll.txt";
            url = $"https://api.mfapi.in/mf/{fundId}";
            string contents = GetDataFromWeb(url);
            RootObject valueSet = JsonConvert.DeserializeObject<RootObject>(contents);

            return valueSet;
        }

        public static string GetDataFromWeb(string urlToGetData)
        {
            string s = "";
            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(urlToGetData);
            }
            return s;
        }

        private string GetDBConnectionString()
        {
            return "Server =tcp:learningsqldb.database.windows.net,1433;Initial Catalog=Learning;Persist Security Info=False;User ID=nagendra;Password=AzureLearning#1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;";
        }
    }

    public class FundMetaData
    {
        public string fund_house { get; set; }
        public string scheme_type { get; set; }
        public string scheme_category { get; set; }
        public int scheme_code { get; set; }
        public string scheme_name { get; set; }
    }

    public class NAVData
    {
        public string fundid { get; set; }
        public string date { get; set; }
        public DateTime dateNav { get; set; }
        public string nav { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("meta")]
        public FundMetaData fundData { get; set; }

        [JsonProperty("data")]
        public List<NAVData> navListData { get; set; }
        public string status { get; set; }
    }
}
