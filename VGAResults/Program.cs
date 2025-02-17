using System.Globalization;
using VgaUI.Shared;

namespace VGAResults
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

            builder.Services.AddSingleton<MongoDBService>();


            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddLogging(loggingBuilder =>
            {
                // Add configuration for logging providers, e.g., console, debug, etc.
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
