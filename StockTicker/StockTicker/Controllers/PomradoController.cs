using CoreUtility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Utility.DS;

namespace StockTicker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PomradoController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [Route("Ping")]
        // https://stocktickergithubnag.azurewebsites.net/api/Pomrado/Ping
        public string Ping()
        {
            return "Pomrado Connected";
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("CreatePomradoSingleRecord")]
        // https://localhost:44327/api/Pomrado/CreatePomradoSingleRecord
        // https://stocktickergithubnag.azurewebsites.net/api/Pomrado/CreatePomradoSingleRecord
        public string CreatePomradoSingleRecord(PomradoDetail pomradoObj)
        {
            string serializedObject = JsonConvert.SerializeObject(pomradoObj);
            PomradoDetailRepo repoObj = new PomradoDetailRepo();
            var x = repoObj.CreateSinglePomradoDetail(pomradoObj);
            return "Pomrado " + serializedObject;
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("GetPomradoDetails")]
        // https://localhost:44327/api/Pomrado/GetPomradoDetails
        // https://stocktickergithubnag.azurewebsites.net/api/Pomrado/GetPomradoDetails
        [EnableCors("AllowAllOrigins")] // Enable CORS for this method
        public List<PomradoDetail> GetPomradoDetails()
        {

            PomradoDetailRepo repoObj = new PomradoDetailRepo();
            var x = repoObj.GetPomradoDetails();
            return x;
        }
    }

    public class PomradoDetail
    {
        public string PersonName { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class PomradoDetailRepo
    {
        private string GetDBConnectionString()
        {
            return "Server =tcp:learningsqldb.database.windows.net,1433;Initial Catalog=Learning;Persist Security Info=False;User ID=nagendra;Password=AzureLearning#1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=300;";

        }
        public List<PomradoDetail> GetPomradoDetails()
        {
            List<PomradoDetail> lstPomradoDetails = new List<PomradoDetail>();
            string dbConnectionstring
               = GetDBConnectionString(); ;
            string query = $"SELECT Top 20 StartTime,EndTime,ShortDesc,'' LongDesc, PersonName FROM PomradoDetail ORDER BY EndTime DESC";
            int count = 0;
            DBUtilities.ConnectionString = dbConnectionstring;
            DataSet dsSeries = DBUtilities.ExecuteDataSet(query, "PomradoDetail");
            lstPomradoDetails = DataSetUtilities.BindList<PomradoDetail>(dsSeries.Tables[0]);

            return lstPomradoDetails;
        }
        public List<PomradoDetail> CreateSinglePomradoDetail(PomradoDetail pomradoInput)
        {
            List<PomradoDetail> lstPomradoDetails = new List<PomradoDetail>();
            lstPomradoDetails.Add(pomradoInput);
            DataTable dt = DataSetUtilities.ToDataTable<PomradoDetail>(lstPomradoDetails);
            dt.TableName = "PomradoDetail";
            DBUtilities.ConnectionString = GetDBConnectionString();
            //DBUtilities.DeleteTableFromDatabase(dt);
            DBUtilities.InsertDatatableToDatabase(dt);
            return lstPomradoDetails;
        }
    }
}
