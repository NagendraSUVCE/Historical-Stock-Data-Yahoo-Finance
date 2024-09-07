using Azure.Identity;
using ExcelDataReader;
using Microsoft.Graph;
using StockTicker.Controllers;
using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Azure.Core;
namespace StockTicker.Utility
{
    public static class GraphFileUtility
    {
        static SecretClient client = null;
        static SecretClientOptions options = null;
        public static string GraphFileUtilityGetSecret(string key)
        {
            if (options == null)
            {
                options = new SecretClientOptions()
                {
                    Retry =
                        {
                            Delay= TimeSpan.FromSeconds(2),
                            MaxDelay = TimeSpan.FromSeconds(16),
                            MaxRetries = 5,
                            Mode = RetryMode.Exponential
                         }
                };
            }
            if (client == null)
            {
                client = new SecretClient(new Uri("https://nagkeyvault.vault.azure.net/"), new DefaultAzureCredential(), options);
            }
            KeyVaultSecret secret = client.GetSecret(key);

            string secretValue = secret.Value;
            return secretValue;
        }
        public static GraphServiceClient GetGraphClientWithClientSecretCredential()
        {
            string clientId = GraphFileUtilityGetSecret("clientId");
            string tenantId = GraphFileUtilityGetSecret("tenantId"); 
            string clientSecret = GraphFileUtilityGetSecret("clientSecret"); 
            string[] scopes = { "https://graph.microsoft.com/.default" };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            return new GraphServiceClient(clientSecretCredential, scopes);
        }

        public static async Task CreateTemporaryFileInLocal(string folderPathInput, string fileNameInput, string tempFileName)
        {
            System.Data.DataSet ds = null;
            var filePath = tempFileName; // "timesheetbyte.xlsx";
            var fileName = fileNameInput; //  "15-Min-Timesheet-168-Hours v2.xlsx";
            var folderPath = folderPathInput;// "Nagendra/000 Frequent";

            try
            {
                GraphServiceClient graphServiceClient = GraphFileUtility.GetGraphClientWithClientSecretCredential();
                var driveItems = await graphServiceClient.Users["nagadmin@nagendrastorage.onmicrosoft.com"]
                       .Drives.GetAsync(conf =>
                       {
                           conf.QueryParameters.Expand = new[] { "root" };
                       });
                var driveId = "b!vjzdZlwNN0qGLjDC3N_egwrus8LrtqVLj_Sc6rRDa5eI5dQJCxBNSodg9w_KLj6V";
                // Get the drive item (timesheet.xlsx)
                var driveItemExample = await graphServiceClient
                    .Drives[driveId]
                    .Root
                    .ItemWithPath(folderPath)
                    .Children
                    .GetAsync();

                foreach (var itemChild in driveItemExample.Value)
                {
                    if (itemChild.Name.Contains(fileName))
                    {
                        var s = itemChild.Content;
                        var driveItem =
                            await graphServiceClient
                            .Drives[driveId]
                            .Items[itemChild.Id]
                            .Content
                            .GetAsync();
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            driveItem.CopyTo(fileStream);
                        };
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public static DataSet GetDataFromExcelNewWay(string filePath)
        {
            DataSet ds = null;
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Choose one of either 1 or 2:

                    // 1. Use the reader methods
                    do
                    {
                        while (reader.Read())
                        {
                            // reader.GetDouble(0);
                        }
                    } while (reader.NextResult());

                    // 2. Use the AsDataSet extension method
                    var result = reader.AsDataSet();
                    ds = result;
                    // The result of each spreadsheet is in result.Tables
                }
            }
            return ds;
        }

    }
}
