using ark.bible.analysis;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CurrentUser>();
// Add services to the container.
builder.Services.AddControllersWithViews();
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
app.Use(async (ctx, nxt) =>
{
    var user = app.Services.GetRequiredService<CurrentUser>();
    user.orig_email = ctx.Request.Cookies["sticky_email"] ?? "";
    user.email = ctx.Request.Cookies["sticky_email"] ?? "";
    user.email = user.email.ToLower().Trim().Replace("@", "_at_the_rate_").Replace(".", "_dot_");
    user.ip = ctx.Request.Cookies["sticky_ip"] ?? "";
    user.base_url = app.Configuration.GetValue<string>("base_url");
    await nxt.Invoke();
});
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
