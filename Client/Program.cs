using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace VgaUI.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Add appsettings.json and appsettings.Development.json to the configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            // Read environment variable and set configuration in memory
            var baseUrl = Environment.GetEnvironmentVariable("ServerApi__BaseUrl") ?? builder.Configuration["ServerApi:BaseUrl"];
            builder.Configuration["ServerApi:BaseUrl"] = baseUrl;

            builder.Services.AddHttpClient("VgaUI.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("VgaUI.ServerAPI"));
            builder.Services.AddScoped<LocalStorageAccessor>(); // Not currently used but want to keep it for future use
            builder.Services.AddSingleton<ResultsWrapper, ResultsWrapper>(); // AddSingleton is fine. Single user client
                                                                             //builder.Services.AddScoped<ResultsWrapper>();
            builder.Services.AddScoped<ActiveTabService>();

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.Cache.CacheLocation = "localStorage"; // Add this line
                string? scopes = builder.Configuration.GetSection("ServerApi")["Scopes"];
                if (scopes != null)
                {
                    options.ProviderOptions.DefaultAccessTokenScopes.Add(scopes);
                }
            });

            await builder.Build().RunAsync();
        }
    }
}