using GameRec.Api.Repositories;

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