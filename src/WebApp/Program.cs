using Application.Authorization;
using Core.Authorization.ParameterValidation;
using Core.Client;
using Core.Scope;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAuthorizationParameterValidator, AuthorizationParameterValidator>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IScopeRepository, ScopeRepository>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
