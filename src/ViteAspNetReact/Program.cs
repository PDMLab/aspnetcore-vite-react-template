// var builder = WebApplication.CreateBuilder(args);
//
// builder.Services.AddControllersWithViews();
//
// var app = builder.Build();
//
// // var options = new DefaultFilesOptions();
// // options.DefaultFileNames.Clear();
// // options.DefaultFileNames.Add("default.html");
//
//
// // app.UseDefaultFiles(options);
// app.UseStaticFiles();
//
//
// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }
//
//
// app.UseHttpsRedirection();
//
// app.UseRouting();
//
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}"
// );
//
// app.Run();

using ViteAspNetReact;

var configuration = new ConfigurationBuilder()
  .AddEnvironmentVariables()
  .AddCommandLine(args)
  .AddJsonFile("appsettings.json")
  .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
  .Build();

var builder = ConfigureHost.GetHostBuilder(configuration);
builder
  .Build()
  .Run();
