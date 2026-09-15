using KidsRewards.Api.Configuration;
using KidsRewards.Api.Repositories.Interfaces;
using KidsRewards.Api.Repositories.Mongo;
using KidsRewards.Api.Services.Implementation;
using KidsRewards.Api.Services.Interfaces;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>() ?? new MongoDbSettings();

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();
builder.Services.AddScoped<IPointsHistoryRepository, PointsHistoryRepository>();

builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<IPointsHistoryService, PointsHistoryService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://192.168.1.102:3000", "http://54.232.189.113:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextJs");
app.UseAuthorization();
app.MapControllers();

app.Run();