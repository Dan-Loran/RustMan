using RustMan.Core.Modules.Routing;
using RustMan.Core.Modules.WebRcon.Contracts;
using RustMan.Infrastructure.Modules.Routing;
using RustMan.Infrastructure.Modules.WebRcon.Runtime;
using RustMan.Infrastructure.Modules.Wiring;
using RustMan.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IRouterModule, RouterModule>();
builder.Services.AddSingleton<IWebRconModule, WebRconModule>();
builder.Services.AddSingleton<RuntimeModuleWiring>();

var app = builder.Build();
_ = app.Services.GetRequiredService<RuntimeModuleWiring>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
