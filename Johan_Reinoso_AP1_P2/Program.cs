using Blazored.Toast;
using Johan_Reinoso_AP1_P2.Components;
using Johan_Reinoso_AP1_P2.Components.DAL;
using Johan_Reinoso_AP1_P2.Components.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddBlazoredToast();

var conStr = builder.Configuration.GetConnectionString("SqlServerConStr");
builder.Services.AddDbContextFactory<Contexto>(o => o.UseSqlServer(conStr));

builder.Services.AddScoped<ParcialService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
