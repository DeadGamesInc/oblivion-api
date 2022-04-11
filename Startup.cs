/*
 *  OblivionAPI :: Startup
 *
 *  This class is responsible for setting up the application container and initializing all the parameters.
 * 
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OblivionAPI.Services;

namespace OblivionAPI {
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddCors(options => 
                options.AddPolicy("Everyone", builder => {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }));
            services.AddSingleton<LookupService>();
            services.AddSingleton<BlockchainService>();
            services.AddSingleton<DatabaseService>();
            services.AddControllers();
            services.AddHostedService<MonitorService>();
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseCors("Everyone");
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
