using UI.EmployerPortal.Web.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using UI.EmployerPortal.Web.Startup.WcfServiceClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment());

builder.Services.AddWcfServiceClients(builder.Configuration);
builder.Services.AddScoped<IAddressValidationWrapper, AddressValidationService>();
builder.Services.AddScoped<RegistrationStateService>();
builder.Services.AddScoped<AddressValidationCoordinator>();
builder.Services.AddScoped<IYearQuarterPaidWagesService, YearQuarterPaidWagesService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
