using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using ViteAspNetReact.Areas.Identity.Data;
using ViteAspNetReact.Core;
using ViteAspNetReact.Features;
using ViteAspNetReact.Features.DisableUser;
using ViteAspNetReact.Features.GetInvitedUsers;
using ViteAspNetReact.Features.RegisterUser;
using ViteAspNetReact.Features.SetOrganizationAddress;
using Weasel.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ViteAspNetReact;

public static class ConfigureHost
{
  public static IHostBuilder GetHostBuilder(
    IConfigurationRoot configuration,
    Action<IServiceCollection>? configureServices = null
  )
  {
    var hostBuilder = Host.CreateDefaultBuilder();


    var serilogLogger = Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.Console()
      .CreateLogger();

    var dotnetILogger = new SerilogLoggerFactory(serilogLogger)
      .CreateLogger<Program>();
    // hostBuilder.UseSerilog();

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
            services.AddSingleton<ILogger>(dotnetILogger);
            var identityTestConnectionString = context.Configuration.GetConnectionString("Identity");
            services.AddDbContext<ApplicationDbContext>(
              options =>
              {
                options.UseNpgsql(identityTestConnectionString);
              }
            );

            services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
              .AddEntityFrameworkStores<ApplicationDbContext>();

            services
              .AddTracing();

            services.AddScoped<LinkGeneratorRegistration>();

            services.AddMarten(
              context.Configuration,
              globalStoreOptions =>
              {
                globalStoreOptions.UseRegisteredUserProjections();
                globalStoreOptions.UseDisabledUserProjections();
                globalStoreOptions.UseInvitedUserProjections();
                globalStoreOptions.UseSubscriptionProjections();
              },
              subscriptionStoreOptions =>
              {
                var connectionString =
                  context.Configuration
                    .GetSection("EventStore")["ConnectionString"] ??
                  throw new InvalidOperationException();

                subscriptionStoreOptions.MultiTenantedWithSingleServer(connectionString);
                subscriptionStoreOptions.AutoCreateSchemaObjects = AutoCreate.All;

                subscriptionStoreOptions.UseOrganizationProjections();
              },
              freeUsersStoreOptions => { }
            );

            services.AddControllersWithViews()
              .AddRazorRuntimeCompilation()
              .AddJsonOptions(
                options =>
                {
                  options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                }
              );

            services.AddProblemDetails();

            services.AddMvc();
            
            // services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            // services.AddScoped<IUrlHelper>(x => {
            //   var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
            //   var factory = x.GetRequiredService<IUrlHelperFactory>();
            //   return factory.GetUrlHelper(actionContext);
            // });


            services.AddTransient<IClaimsTransformation, OidcLikeClaimsTransformation>();



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
                var endpointConventionBuilder = endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToController("Index", "Home");
              }
            );
          }
        );
      }
    );

    hostBuilder.UseSerilog(serilogLogger);
    return hostBuilder;
  }
}


// public static class ConfigureHost
// {
//   public static IHostBuilder GetHostBuilder(
//     IConfigurationRoot configuration,
//     Action<IServiceCollection>? configureServices = null
//   )
//   {
//     var hostBuilder = Host.CreateDefaultBuilder();
//
//
//     hostBuilder.ConfigureHostConfiguration(
//       builder =>
//       {
//         builder.AddConfiguration(configuration);
//         // builder.AddInMemoryCollection(configuration);
//       }
//     );
//
//     hostBuilder.ConfigureWebHostDefaults(
//       builder =>
//       {
//         builder.ConfigureServices(
//           (
//             context,
//             services
//           ) =>
//           {
//             var identityTestConnectionString = context.Configuration.GetConnectionString("Identity");
//             services.AddDbContext<ApplicationDbContext>(
//               options =>
//                 options.UseNpgsql(identityTestConnectionString)
//             );
//
//             services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
//               .AddEntityFrameworkStores<ApplicationDbContext>();
//
//
//             
//
//             services.AddControllersWithViews()
//               .AddRazorRuntimeCompilation();
//
//             services.AddMvc();
//
//             configureServices?.Invoke(services);
//           }
//         );
//         builder.Configure(
//           (
//             context,
//             app
//           ) =>
//           {
//             if (!context.HostingEnvironment.IsDevelopment())
//             {
//               app.UseExceptionHandler("/Error");
//               app.UseHsts();
//             }
//
//             app.UseStaticFiles();
//
//             app.UseRouting();
//
//             app.UseAuthentication();
//             app.UseAuthorization();
//
//             app.UseEndpoints(
//               endpoints =>
//               {
//                 endpoints.MapControllers();
//                 endpoints.MapRazorPages();
//                 endpoints.MapDefaultControllerRoute();
//               }
//             );
//           }
//         );
//       }
//     );
//
//     return hostBuilder;
//   }
// }
