using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(opts =>
            {
                opts.Conventions.Insert(0, new LocalizationConvention());
                opts.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));
            });

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("de"),
                new CultureInfo("fr-FR"),
            };

            var options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            options.RequestCultureProviders = new[]
            {
            new RouteDataRequestCultureProvider()
                {
                    RouteDataStringKey = "culture",
                    Options = options
                }
             };

            services.AddSingleton(options);
            services.Configure<RouteOptions>(opts =>
             opts.ConstraintMap.Add("culturecode", typeof(CultureRouteConstraint)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, RequestLocalizationOptions localizationOptions, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{culture:culturecode}/{controller=Home}/{action=Index}/{id?}");
                routes.MapGet("{culture:culturecode}/{*path}", appBuilder => { });
                routes.MapGet("{*path}", (RequestDelegate)(ctx =>
                {
                    var defaultCulture = localizationOptions.DefaultRequestCulture.Culture.Name;
                    var path = ctx.GetRouteValue("path") ?? string.Empty;
                    var culturedPath = $"/{defaultCulture}/{path}";
                    ctx.Response.Redirect(culturedPath);
                    return Task.CompletedTask;
                }));
            });
        }
    }
}
