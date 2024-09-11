using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.IO;
using System.Threading.Tasks;
using System;
using Azure.Identity;
using ClosedXML.Excel;
using System.Data;
using Microsoft.Graph.Models;
using System.Diagnostics;
using ExcelDataReader;
using Microsoft.Graph.Models.Security;
using DataSet = System.Data.DataSet;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using StockTicker.Utility;
using CoreUtility;
using Utility.DS;

namespace StockTicker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphExcelController : ControllerBase
    {

        [HttpGet]
        [AllowAnonymous]
        [Route("Ping")]
        // https://stocktickergithubnag.azurewebsites.net/api/GraphExcel/Ping
        // https://localhost:44327/api/GraphExcel/Ping

        public string Ping()
        {
            return "GraphExcel Connected";
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("GetExcelDailyLog")]
        [EnableCors("AllowAllOrigins")] // Enable CORS for this method
        // https://stocktickergithubnag.azurewebsites.net/api/GraphExcel/GetExcelDailyLog
        // https://localhost:44327/api/GraphExcel/GetExcelDailyLog

        public async Task<List<DailyLog15Min>> GetExcelDailyLog()
        {
            ExcelFileFromOneDrive excelFileFromOneDrive = new ExcelFileFromOneDrive();
            var d = await excelFileFromOneDrive.GetDaily15MinLogFromGraphExcel();
            var top100 = d.OrderByDescending(d => d.dtActivity).Take(100).ToList();

            return top100;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetExcelDataTableForCAMS")]
        [EnableCors("AllowAllOrigins")] // Enable CORS for this method
        // https://stocktickergithubnag.azurewebsites.net/api/GraphExcel/GetExcelDataTableForCAMS
        // https://localhost:44327/api/GraphExcel/GetExcelDataTableForCAMS
        public async Task<string> GetExcelDataTableForCAMS(bool saveToDB = false)
        {
            string returnResult = "";
            ExcelFileFromOneDrive excelFileFromOneDrive = new ExcelFileFromOneDrive();
            var d = await excelFileFromOneDrive.GetCAMSDataset();
            DataTable dtSaveToDB = d.Tables[0];
            returnResult = returnResult + " MF Transactions returned Successfully";
            if (saveToDB)
            {
                string connectionString = KeyVaultUtility.KeyVaultUtilityGetSecret("learningdbconnectionstring");
                DBUtilities.ConnectionString = connectionString;
                dtSaveToDB.TableName = "MFTransactionFromCAMS";
                DBUtilities.DeleteTableFromDatabase(dtSaveToDB);
                DBUtilities.InsertDatatableToDatabase(dtSaveToDB);
                returnResult = returnResult + " Saved To DB Successfully";
            }
            return returnResult;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetExcelForPayslipsSummarized")]
        [EnableCors("AllowAllOrigins")] // Enable CORS for this method
        // https://stocktickergithubnag.azurewebsites.net/api/GraphExcel/GetExcelForPayslipsSummarized
        // https://localhost:44327/api/GraphExcel/GetExcelForPayslipsSummarized
        public async Task<List<MSFTSharesSummary>> GetExcelForPayslipsSummarized(bool saveToDB = false)
        {
            string returnResult = "";
            ExcelFileFromOneDrive excelFileFromOneDrive = new ExcelFileFromOneDrive();
            var d = await excelFileFromOneDrive.GetPayslipsSummarized();
            DataTable specificWorkSheet = d.Tables["MSFTSharesSummary"];
            specificWorkSheet.TableName = "MSFTSharesSummary";
            List<MSFTSharesSummary> lstMSFTSharesSummary = DataSetUtilities.BindList<MSFTSharesSummary>(specificWorkSheet);
            returnResult = returnResult + " MSFTSharesSummary returned Successfully";
            if (saveToDB)
            {
                string connectionString = KeyVaultUtility.KeyVaultUtilityGetSecret("learningdbconnectionstring");
                DBUtilities.ConnectionString = connectionString;
               // dtSaveToDB.TableName = "MFTransactionFromCAMS";
                // DBUtilities.DeleteTableFromDatabase(dtSaveToDB);
                // DBUtilities.InsertDatatableToDatabase(dtSaveToDB);
                returnResult = returnResult + " MSFTSharesSummary Saved To DB Successfully";
            }
            var top100 = lstMSFTSharesSummary.OrderByDescending(d => d.TxnDate).Take(100).ToList();

            return top100;
        }
    }

    // C:\Users\nagendrs\OneDrive - Krishna\Nagendra\all Salary\Mutual funds Demat Acc Share\MutualFunds CAMS\20240708_CAMSDetail.xls
    // C:\Users\nagendrs\OneDrive - Krishna\Nagendra\000 Frequent\15-Min-Timesheet-168-Hours v2.xlsx

    public class ExcelFileFromOneDrive
    {
        public async Task<System.Data.DataSet> GetCAMSDataset()
        {
            var tempFilePath = "cams.xlsx";
            var fileName = "20240708_CAMSDetail.xls";
            var folderPath = @"Nagendra/all Salary/Mutual funds Demat Acc Share/MutualFunds CAMS";
            System.Data.DataSet ds = null;
            try
            {
                await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, tempFilePath);
                ds = GraphFileUtility.GetDataFromExcelNewWay(tempFilePath);

            }
            catch (Exception ex)
            {
                throw;
            }
            return ds;
        }
        public async Task<System.Data.DataSet> GetPayslipsSummarized()
        {
            var tempFilePath = "payslips.xlsx";
            var fileName = "all Payslips Summarized.xlsx";
            var folderPath = @"Nagendra/all Salary/all Payslips";
            System.Data.DataSet ds = null;
            try
            {
                await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, tempFilePath);
                ds = GraphFileUtility.GetDataFromExcelNewWayUseHeader(tempFilePath);

            }
            catch (Exception ex)
            {
                throw;
            }
            return ds;
        }
        public async Task<List<DailyLog15Min>> GetDaily15MinLogFromGraphExcel()
        {
            var tempFilePath = "timesheetbyte.xlsx";
            var fileName = "15-Min-Timesheet-168-Hours v2.xlsx";
            var folderPath = "Nagendra/000 Frequent";
            System.Data.DataSet ds = null;
            List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
            try
            {
                await GraphFileUtility.CreateTemporaryFileInLocal(folderPath, fileName, tempFilePath);
                ds = GraphFileUtility.GetDataFromExcelNewWay(tempFilePath);
                lstDailyLog15Min = await GetAll15MinLogsFromDataset(ds);

            }
            catch (Exception ex)
            {
                // https://learn.microsoft.com/en-us/answers/questions/1191723/problem-extract-data-using-microsoft-graph-c-net
                throw;
            }
            return lstDailyLog15Min;
        }
        public async Task<List<DailyLog15Min>> GetAll15MinLogsFromDataset(DataSet dataset15minActivity)
        {
            List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
            var temp = await GetAll15MinLogs(dataset15minActivity.Tables[0]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;
            int sheetIndex = 0;
            temp = await GetAll15MinLogs(dataset15minActivity.Tables[1]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;

            temp = await GetAll15MinLogs(dataset15minActivity.Tables[2]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;


            temp = await GetAll15MinLogs(dataset15minActivity.Tables[3]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;


            temp = await GetAll15MinLogs(dataset15minActivity.Tables[4]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;


            temp = await GetAll15MinLogs(dataset15minActivity.Tables[5]);
            lstDailyLog15Min.AddRange(temp.ToList());
            temp = null;
            return lstDailyLog15Min;
        }
        public async Task<List<DailyLog15Min>> GetAll15MinLogs(DataTable dt15minActivity)
        {
            DateTime dtDateOfActivity = new DateTime(2000, 01, 01);
            List<DailyLog15Min> lstDailyLog15Min = new List<DailyLog15Min>();
            for (int j = 0; j < dt15minActivity.Columns.Count; j++)
            {
                for (int i = 0; i < dt15minActivity.Rows.Count; i++)
                {
                    object dataCell = dt15minActivity.Rows[i][j];
                    DailyLog15Min dailyLog15MinObj = new DailyLog15Min();
                    if (dataCell != null && !String.IsNullOrWhiteSpace(dataCell.ToString()))
                    {
                        DateTime dtTempForParse;
                        if (DateTime.TryParse(dataCell.ToString(), out dtTempForParse))
                        {
                            if (dtTempForParse.Year > 2018)
                            {
                                dtDateOfActivity = dtTempForParse;
                                dtDateOfActivity = dtDateOfActivity.AddHours(5);
                            }

                        }
                        else
                        {
                            if (i > 2)
                            {
                                string activityDesc = dataCell.ToString();
                                dailyLog15MinObj.dtActivity = dtDateOfActivity;
                                dailyLog15MinObj.activityDesc = activityDesc;
                                dailyLog15MinObj.rowIndex = i;
                                dailyLog15MinObj.colIndex = j;
                                // dailyLog15MinObj.category = SetAllFieldsForDailyActivity(dailyLog15MinObj);
                                dailyLog15MinObj.Hrs = 0.25m;
                                if (dailyLog15MinObj.activityDesc != null && dailyLog15MinObj.activityDesc.Trim() != "")
                                {
                                    string[] splitDesc = dailyLog15MinObj.activityDesc.Split('-');
                                    if (splitDesc != null && splitDesc.Count() > 0)
                                    {

                                        dailyLog15MinObj.category = splitDesc[0];
                                    }

                                    if (splitDesc != null && splitDesc.Count() > 1)
                                    {

                                        dailyLog15MinObj.activityGroup = splitDesc[1];
                                    }

                                    if (splitDesc != null && splitDesc.Count() > 2)
                                    {

                                        dailyLog15MinObj.activityName = splitDesc[2];
                                    }

                                    if (splitDesc != null && splitDesc.Count() > 3)
                                    {

                                        dailyLog15MinObj.activityIndex = splitDesc[3];
                                    }
                                }
                                lstDailyLog15Min.Add(dailyLog15MinObj);
                                dtDateOfActivity = dtDateOfActivity.AddMinutes(15);
                            }
                        }
                    }
                }
            }
            return lstDailyLog15Min;
        }

    }

    public class DailyLog15Min
    {
        public DateTime dtActivity { get; set; }
        public string activityDesc { get; set; }

        public int colIndex { get; set; }
        public int rowIndex { get; set; }
        public string category { get; set; }
        public string activityGroup { get; set; }
        public string activityName { get; set; }
        public string activityIndex { get; set; }
        public decimal Hrs { get; set; }
    }

    public class MSFTSharesSummary
    {
        public int EmpId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime TxnDate { get; set; }
        public decimal TxnAmt { get; set; }
        public decimal FairMarketValue { get; set; }
        public decimal SharesForTaxes { get; set; }
        public decimal NetShares { get; set; }
        public string SharesType { get; set; }
    }
}
