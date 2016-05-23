using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Ascon.Pilot.WebClient
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddAuthorization();
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddCaching();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCookieAuthentication(options =>
            {
                options.AuthenticationScheme = ApplicationConst.PilotMiddlewareInstanceName;
                options.LoginPath = new PathString("/Account/LogIn");
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
            });

            loggerFactory.AddNLog();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //env.ConfigureNLog("nlog.config");

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();

            app.Use(async (context, next) =>
            {
                int visits = context.Session.GetInt32(SessionKeys.VisitsCount) ?? 0;
                if (visits == 0)
                {
                    // New session, do any initial setup and then mark the session as ready
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        await context.Authentication.SignOutAsync(ApplicationConst.PilotMiddlewareInstanceName);
                        context.Response.Redirect("/Account/LogIn");
                    }
                    else
                    {
                        context.Session.SetInt32(SessionKeys.VisitsCount, 1);
                    }
                }
                await next();
            });

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}