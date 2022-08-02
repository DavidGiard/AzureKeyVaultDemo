using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureKeyVaultDemo
{
    internal class Program
    {
        const string KEYVAULTNAME = "MY_KEY_VAULT_NAME"; // Set this to the name of your key vault
        static string keyVaultUri = $"https://{KEYVAULTNAME}.vault.azure.net";
        static SecretClient secretClient = null;

        static async Task Main(string[] args)
        {
            // Uncomment the following line to add logging to the output
            //using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();

            var credentials = new DefaultAzureCredential();
            secretClient = new SecretClient(new Uri(keyVaultUri), credentials);

            ListAllSecrets();

            Console.Write("Input the Secret name to set (ENTER to skip):");
            var setSecretName = Console.ReadLine();
            if (setSecretName != "")
            {
                Console.Write("Input the Secret value to set:");
                var setSecretValue = Console.ReadLine();

                Console.WriteLine("Setting secret...");
                await secretClient.SetSecretAsync(setSecretName, setSecretValue);
                Console.WriteLine($"Secret {setSecretName} set to {setSecretValue}!");

                // Set content type
                var secret = secretClient.GetSecret(setSecretName);
                secret.Value.Properties.ContentType = "Demo Type";
                secretClient.UpdateSecretProperties(secret.Value.Properties);

                Console.WriteLine();
                ListAllSecrets();
            }

            Console.Write("Input a Secret Name to soft delete (ENTER to skip):");
            var deleteSecretName = Console.ReadLine();
            if (deleteSecretName != "")
            {
                var operation = secretClient.StartDeleteSecret(deleteSecretName);
                Console.Write($"Deleting secret {deleteSecretName}...");
                while (!operation.HasCompleted)
                {
                    Thread.Sleep(1000);
                    Console.Write($".");
                    operation.UpdateStatus();
                }
                Console.WriteLine();
                Console.WriteLine($"Secret {deleteSecretName} deleted!");

                Console.WriteLine();
                ListAllSecrets();

            }

            Console.Write("Input a Secret Name to permanently delete (ENTER to skip):");
            var purgeSecretName = Console.ReadLine();
            if (purgeSecretName != "")
            {
                var operation = secretClient.StartDeleteSecret(purgeSecretName);
                Console.Write($"Soft deleting secret {purgeSecretName}...");
                while (!operation.HasCompleted)
                {
                    Thread.Sleep(1000);
                    Console.Write($".");
                    operation.UpdateStatus();
                }
                Console.WriteLine();
                Console.WriteLine($"Secret {purgeSecretName} deleted!");
                secretClient.PurgeDeletedSecret(purgeSecretName);
                Console.WriteLine($"Secret {purgeSecretName} purged!");

                Console.WriteLine();
                ListAllSecrets();
            }


            Console.WriteLine();
            Console.WriteLine("Done!");
        }

        private static void ListAllSecrets()
        {
            Console.WriteLine("All not deleted Secrets with current values:");
            var allSecrets = secretClient.GetPropertiesOfSecrets();
            foreach (var secret in allSecrets)
            {
                var secretValue = secretClient.GetSecret(secret.Name);
                Console.WriteLine($"{secret.Name} | {secretValue.Value.Value} | {secretValue.Value.Properties.ContentType}");
            }
            Console.WriteLine();
        }
    }
}
