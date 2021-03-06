﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OptionsPro.Extensions;
using OptionsPro.Models;

namespace OptionsPro
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Locations>(Configuration.GetSection("Locations"));
            services.Configure<JwtOptions>(Configuration.GetSection("JWT"));
            services.Configure<Shared>(Configuration.GetSection("Shared"));
            var serviceProvider = services.BuildServiceProvider();
            var opt = serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;
            //services.AddJwtAuthentication(opt.Issuer, opt.Key);
            services.AddJwtAuthentication(opt);
            services.ConfigureWritable<Locations>(Configuration.GetSection("Locations"));
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory
            , IOptionsMonitor<Locations> locationsMonitor
            , IOptionsMonitor<JwtOptions> jwtMonitor)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            locationsMonitor.OnChange(
                    vals =>
                    {
                        loggerFactory
                            .CreateLogger<IOptionsMonitor<Locations>>()
                            .LogDebug($"Config changed: {string.Join(", ", vals)}");
                    });
            jwtMonitor.OnChange(
                    vals =>
                    {
                        loggerFactory
                            .CreateLogger<IOptionsMonitor<JwtOptions>>()
                            .LogDebug($"Config changed: {string.Join(", ", vals)}");
                    });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
