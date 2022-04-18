/*
 *  OblivionAPI :: Startup
 *
 *  This class is responsible for setting up the application container and initializing all the parameters.
 * 
 */

using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using OblivionAPI.Config;
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
            services.AddSingleton<ImageCacheService>();
            services.AddSingleton<LookupService>();
            services.AddSingleton<BlockchainService>();
            services.AddSingleton<DatabaseService>();
            services.AddControllers();
            services.AddHostedService<MonitorService>();
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            env.WebRootPath = Globals.WEB_ROOT;
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles(new StaticFileOptions {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(Globals.IMAGE_CACHE_DIR),
                RequestPath = new PathString("/image-cache") 
            });
            app.UseRouting();
            app.UseCors("Everyone");
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
