using Elasticsearch.Net;
using Nest;
using NewsApp.Api.Models;
using NewsApp.Api.Services.Abstract;
using NewsApp.Api.Services.Concrete;
using NewsApp.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// secret.json ekledim
builder.Configuration.AddUserSecrets<Program>();
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<INewsAppService, NewsAppService>();

// elastic search deployment ve indexleme ayarlarýný gerçekleþtirdim.
builder.Services.AddSingleton<IElasticClient>(x =>
{
    var settings = new ConnectionSettings(config["cloudId"], new BasicAuthenticationCredentials(
        "elastic", config["password"]))
        .DefaultIndex("news")
        .DefaultMappingFor<NewsAppDto>(i => i.IndexName("news-app-demo"));

    return new ElasticClient(settings);
});

builder.Services.AddHostedService<TestNews>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();


#region VideoSrc
// https://www.youtube.com/watch?v=tw9svKWq6tg&ab_channel=dotnetFlix
// https://cloud.elastic.co/deployments/b5efd44d51b041638fcb92869db2c65f
#endregion