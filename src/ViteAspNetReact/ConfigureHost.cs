using Microsoft.EntityFrameworkCore;
using ViteAspNetReact.Areas.Identity.Data;

namespace ViteAspNetReact;

public static class ConfigureHost
{
  public static IHostBuilder GetHostBuilder(
    IConfigurationRoot configuration,
    Action<IServiceCollection>? configureServices = null
  )
  {
    var hostBuilder = Host.CreateDefaultBuilder();


    hostBuilder.ConfigureHostConfiguration(
      builder =>
      {
        builder.AddConfiguration(configuration);
        // builder.AddInMemoryCollection(configuration);
      }
    );

    hostBuilder.ConfigureWebHostDefaults(
      builder =>
      {
        builder.ConfigureServices(
          (
            context,
            services
          ) =>
          {
            var identityTestConnectionString = context.Configuration.GetConnectionString("Identity");
            services.AddDbContext<IdentityDbContext>(
              options =>
                options.UseNpgsql(identityTestConnectionString)
            );

            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
              .AddEntityFrameworkStores<IdentityDbContext>();


            ;

            services.AddControllersWithViews()
              .AddRazorRuntimeCompilation();

            services.AddMvc();

            configureServices?.Invoke(services);
          }
        );
        builder.Configure(
          (
            context,
            app
          ) =>
          {
            if (!context.HostingEnvironment.IsDevelopment())
            {
              app.UseExceptionHandler("/Error");
              app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
              endpoints =>
              {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
              }
            );
          }
        );
      }
    );

    return hostBuilder;
  }
}
