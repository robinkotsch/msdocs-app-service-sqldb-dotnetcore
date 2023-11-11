using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNetCoreSqlDb.Data;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.CodeAnalysis;

using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

//builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration.GetConnectionString("AZURE_POSTGRESQL_CONNECTIONSTRING"));




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
var client = new SecretClient(new Uri("https://iiot-keyvault.vault.azure.net/"), new DefaultAzureCredential());

KeyVaultSecret secret = client.GetSecret("AZURE-SQL-CONNECTIONSTRING");

string secretValue = secret.Value;





/*
var configuration = builder.Configuration.AddAzureKeyVault(
    new Uri($"https://iiot-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
*/



//string configvalue = configuration["AZURE-SQL-CONNECTIONSTRING"];




// Add database context and cache
builder.Services.AddDbContext<MyDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")));
//options.UseSqlServer(secretValue));
Debug.WriteLine("test");
Debug.WriteLine(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING"));
builder.Services.AddStackExchangeRedisCache(options =>
{
options.Configuration = builder.Configuration.GetConnectionString("AZURE_REDIS_CONNECTIONSTRING");

options.InstanceName = "SampleInstance";
});

Debug.WriteLine(builder.Configuration.GetConnectionString("AZURE_REDIS_CONNECTIONSTRING"));





// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.AddDistributedRedisCache(option =>
{
    option.Configuration = builder.Configuration["AZURE-REDIS-CONNECTIONSTRING"];
});

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
