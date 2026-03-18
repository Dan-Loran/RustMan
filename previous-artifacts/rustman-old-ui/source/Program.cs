using RustMan.Components;
using RustMan.Data;
using RustMan.Services;
using RustMan.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<InstanceEditState>();
builder.Services.AddSingleton<SqliteConnectionFactory>();
builder.Services.AddSingleton<IPlatformService, HostPlatformService>();
builder.Services.AddSingleton<ISystemCommandRunner, ProcessSystemCommandRunner>();
builder.Services.AddSingleton<IRustRconSocketFactory, ClientWebSocketRustRconSocketFactory>();
builder.Services.AddScoped<ServerPropertyCatalogService>();
builder.Services.AddScoped<CommandCatalogService>();
builder.Services.AddScoped<RustRconService>();
builder.Services.AddScoped<InstanceDraftValidationService>();
builder.Services.AddScoped<InstanceService>();
builder.Services.AddScoped<RustManPathService>();
builder.Services.AddScoped<StartupScriptBuilder>();
builder.Services.AddScoped<SystemdUnitBuilder>();
builder.Services.AddScoped<InstanceProvisioningService>();
builder.Services.AddScoped<InstanceCreationService>();
builder.Services.AddScoped<InstanceUpdateService>();
builder.Services.AddScoped<InstanceDeletionService>();
builder.Services.AddScoped<InstanceBackupService>();
builder.Services.AddScoped<InstanceWipeService>();
builder.Services.AddScoped<InstanceConsoleService>();
builder.Services.AddScoped<HostAboutService>();
builder.Services.AddScoped<ISystemdService, SystemdService>();
builder.Services.AddScoped<ISharedRustRuntimeUpdateService, SharedRustRuntimeUpdateService>();
builder.Services.AddSingleton<SharedRustRuntimeUpdateCoordinator>();

var app = builder.Build();

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

