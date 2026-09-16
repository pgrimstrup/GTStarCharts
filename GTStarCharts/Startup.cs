using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTStarCharts.TravellerMap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GTStarCharts
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
            services.AddDbContext<GTStarData.GTStarDbContext>(options =>
            {
                string connectionString = Configuration.GetConnectionString("GTStarCharts");
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Connection string 'GTStarCharts' was not found. Set ConnectionStrings:GTStarCharts in appsettings.json or user secrets.");

                options.UseSqlServer(connectionString);
            });

            services.AddRazorPages();
            services.AddControllers().AddRazorRuntimeCompilation();
        

            services.AddScoped<MapAPI>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
