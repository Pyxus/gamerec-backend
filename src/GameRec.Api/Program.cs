using GameRec.Api.Repositories;

GameRec.Api.Util.DotEnv.Load();

#region Testing
var clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET");

if (clientId != null && clientSecret != null)
{
    var igdbClient = new IGDBClient(clientId, clientSecret);

    await igdbClient.RefreshAuth();
}

#endregion

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();

var app = builder.Build();
app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
