using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNetCoreSqlDb.Data;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetCoreSqlDb.Controllers;

public class Program
{
    public static string Test { get; private set; }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        /*
        SecretClientOptions options = new SecretClientOptions()
        {
            Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                 }
        };


        */

        Program.Test = "test";

        string secretValue = String.Empty;
        /*
        try
        {
        */

       
            var client = new SecretClient(new Uri("https://iiot-keyvault.vault.azure.net/"), new DefaultAzureCredential());

            KeyVaultSecret secret = client.GetSecret("AZURE-SQL-CONNECTIONSTRING");

            secretValue = secret.Value;
        /*
        }
        catch (Exception e)
        {
            String error = e.Message;

            Program.Test = error;
            
            
        }
        if (secretValue != String.Empty)
        {
            Program.Test = secretValue;
        }

        
        



        /*
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://iiot-keyvault.vault.azure.net/"),
            new DefaultAzureCredential());
        */



        // Add database context and cache
        builder.Services.AddDbContext<MyDatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
            options.InstanceName = "SampleInstance";
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Add App Service logging
        builder.Logging.AddAzureWebAppDiagnostics();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Todos}/{action=Index}/{id?}");

        app.Run();

    }
}
