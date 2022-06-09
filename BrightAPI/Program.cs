using System.Text.Json.Serialization;
using BrightAPI.Database;
using BrightData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
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
}

app.Run();