using System.Text.Json;
using System.Text.Json.Serialization;
using BrightAPI.Controllers;
using BrightAPI.Database;
using BrightAPI.Helper;
using BrightData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
var keepAliveConnection = new SqliteConnection(builder.Configuration["DBConnection"]);
keepAliveConnection.Open();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(keepAliveConnection);
});

// create the bright data context
builder.Services.AddSingleton(new BrightDataContext());
builder.Services.AddSingleton(new TempFileManager(builder.Configuration["TempFileDirectory"]));
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

    // auto load data files
    var initialDataFiles = app.Configuration.GetSection("InitialDataFiles");
    foreach (var dataFile in initialDataFiles.GetChildren()) {
        var path = dataFile["Path"];
        var columns = dataFile.GetSection("Columns").GetChildren().Select(x => x.Value ?? "").ToArray();
        var separator = dataFile["Separator"]?[0];
        var name = dataFile["Name"];
        var hasHeader = dataFile["HasHeader"] == "True";
        var targetIndex = dataFile["Target"];

        if (File.Exists(path)) {
            var fileData = File.ReadAllText(path);
            var context = scope.ServiceProvider.GetRequiredService<BrightDataContext>();
            var tempFileManager = scope.ServiceProvider.GetRequiredService<TempFileManager>();
            var dataTable = DataTableController.CreateDataTableFromCsv(
                new DatabaseManager(dataContext), 
                context, 
                tempFileManager, 
                hasHeader, 
                separator ?? ',', 
                targetIndex is null ? null : uint.Parse(targetIndex), 
                columns, 
                name, 
                fileData
            ).Result;
        }
    }
    
}

app.Run();