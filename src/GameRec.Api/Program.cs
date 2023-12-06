using GameRec.Api.Clients;
using GameRec.Api.Models;

GameRec.Api.Util.DotEnv.Load();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddSingleton(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET");
    return new IgdbClient(httpClientFactory, clientId!, clientSecret!);
});

//  For Quick Testing
var provider = builder.Services.BuildServiceProvider();
var igdb = provider.GetService<IgdbClient>()!;

await igdb.RefreshAuth();
var games = await igdb.Query<Game[]>("games", "fields id, name;")!;
Console.WriteLine(games?[0].Id);
Console.WriteLine(games?[0].Name);
Console.WriteLine(games?.Length);

var app = builder.Build();

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();