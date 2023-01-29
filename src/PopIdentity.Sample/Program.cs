using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PopIdentity.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddPopIdentity();
services.AddHttpContextAccessor();
services.AddControllersWithViews();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();