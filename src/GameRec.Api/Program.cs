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
    var igdbClient = new IgdbClient(httpClientFactory, clientId!, clientSecret!);
    igdbClient.RefreshAuth().Wait();

    return igdbClient;
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});

var app = builder.Build();

app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();