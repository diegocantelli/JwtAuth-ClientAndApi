using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Chamando API...");
            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            AuthConfig config = AuthConfig.ReadJsonFromFile("appsettings.json");
            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Autority))
                .Build();
            
            string[] ResourceIds = new string[] {config.ResourceId};

            AuthenticationResult result = null;

            try
            {
                result = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token Acquired \n");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
            }
            catch (MsalClientException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            if(!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpClient = new HttpClient();
                var defaultRequesteaders = httpClient.DefaultRequestHeaders;

                if(defaultRequesteaders.Accept == null || 
                   !defaultRequesteaders.Accept.Any(x => x.MediaType =="application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                defaultRequesteaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress);

                if(response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Erro ao chamar a API");
                    string content = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Resposta: {content}");
                }
                Console.ResetColor();
            }
        }
    }
}
