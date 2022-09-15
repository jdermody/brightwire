using System.Text.Json;
using System.Text.Json.Serialization;
using BrightAPI.Controllers;
using BrightAPI.Database;
using BrightAPI.Helper;
using BrightData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    
});

// add CORS for dev only
if (builder.Environment.IsDevelopment()) {
    builder.Services.AddCors();
}

// connect to database
var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
keepAliveConnection.Open();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(keepAliveConnection);
});

// create the bright data context
builder.Services.AddSingleton(new BrightDataContext());
builder.Services.AddSingleton(new TempFileManager(@"C:\temp"));
builder.Services.AddScoped<DatabaseManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
    app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

using(var scope = app.Services.CreateScope())
{
    // create the in memory database
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.EnsureCreated();

    // load a test file
    var fileData = File.ReadAllText(@"c:\data\iris.csv");
    var context = scope.ServiceProvider.GetRequiredService<BrightDataContext>();
    var tempFileManaher = scope.ServiceProvider.GetRequiredService<TempFileManager>();
    var dataTable = DataTableController.CreateDataTableFromCSV(new DatabaseManager(dataContext), context, tempFileManaher, false, ',', 5, new [] { "C1", "C2", "C3", "C4", "C5"}, "iris.csv", fileData).Result;
}

app.Run();