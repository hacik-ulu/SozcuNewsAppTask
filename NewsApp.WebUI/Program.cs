using Nest;
using NewsApp.WebUI.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<ElasticsearchContext>();

// IElasticClient ile NEST kütüphanesini kullanabiliyor oluyoruz.
// IElasticClient istediðimizde uygulama bize ElasticsearchContext.Client verecek.
builder.Services.AddSingleton<IElasticClient>(x =>
{
    var context = x.GetRequiredService<ElasticsearchContext>();
    return context.Client;
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

