using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore; // Needed for services.AddDbContext<Entities.CityInfoContext>(o => o.UseSqlServer);

namespace CityInfo.API
{
    public class Startup
    {
        // New Configuration configuration syntax, see old configuration syntax at the end of the file
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Startup.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc() // Add middleware to support web api

                // Add support for application/xml format
                // Header: Accept=application/xml
                .AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()))

                // .AddMvcOptions(o => o.OutputFormatters.Clear())

                /* Set Json format as Pascal Case
                .AddJsonOptions(o => { if(o.SerializerSettings.ContractResolver != null) {
                        var castedResolver = o.SerializerSettings.ContractResolver as DefaultContractResolver;
                        castedResolver.NamingStrategy = null;
                    }})
                */
            ;

            // Custom Service
#if DEBUG
            services.AddTransient<Services.IMailService, Services.LocalMailService>();
#else
            services.AddTransient<Services.IMailService, Services.CloudMailService>();
#endif
            // services.AddTransient<Services.LocalMailService>(); // Added each time the instance is request via dependency injection
            // services.AddScoped<Services.LocalMailService>(); // Added for each request
            // services.AddSingleton<Services.LocalMailService>(); // Added the firt time the instance is requested
            
            // Register/Initialize Entity Framework Context
            // Will use the file .\appSettings.json
            // Or Will use the file ENV VAR connectionStrings:cityInfoDBConnectionString, 
            // See project Debug section Environment Variables or Windows Systen Environment Variables.
            // Environment Variables override all previous settings.
            var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];

            services.AddDbContext<Entities.CityInfoContext>(o => o.UseSqlServer(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // No need to add these loggers in ASP.NET Core 2.0: the call to WebHost.CreateDefaultBuilder(args) 
            // in the Program class takes care of that.
            loggerFactory.AddConsole();
            // loggerFactory.AddDebug();
            loggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
            
            // Exception middleware must be first
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            // Return http status code and status text on error
            // to be available in a browser
            app.UseStatusCodePages();

            // Add Mvc middleware to the request pipeline
            app.UseMvc();

            //app.Run((context) =>
            //{
            //    throw new Exception("Example exception");
            //});

            // Answer for http://localhost
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}

/*
    Old Configuration configuration syntax

    public static IConfigurationRoot Configuration;
    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
        Startup.Configuration = builder.Build();
    }
*/

