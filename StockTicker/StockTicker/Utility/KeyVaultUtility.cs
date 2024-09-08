using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace StockTicker.Utility
{
    public static class KeyVaultUtility
    {
        static SecretClient client = null;
        static SecretClientOptions options = null;
        public static string KeyVaultUtilityGetSecret(string key)
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
    }
}
