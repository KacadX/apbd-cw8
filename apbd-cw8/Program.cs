using apbd_cw8.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();