using IgbExcelDemo.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddTransient<EarthquakeDataService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// 気象庁防災情報 XML から取得したデータに基づき、直近数日間に発生した地震の発生日時の集合を返します。
app.MapGet("/api/earthquake/datetimes", (EarthquakeDataService service) => service.GetDateTimesAsync());

app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
