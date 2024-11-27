// WebStartup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Nostromo.Server.Server
{
    public class WebStartup
    {
        private readonly IWebHostEnvironment _env;

        public WebStartup(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Web-specific services only
            services.AddControllers();
            services.AddHttpContextAccessor();

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Nostromo API",
                    Version = "v1",
                    Description = "API endpoints for Nostromo Server"
                });
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("Development", builder =>
                {
                    builder.WithOrigins("http://localhost:5173")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nostromo API V1");
                c.RoutePrefix = "swagger";
            });

            string _serverProjectPath = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../..", "Nostromo.Server"));
            var webuiPath = Path.Combine(_serverProjectPath, "webui");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(webuiPath),
                RequestPath = "/webui",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    Console.WriteLine($"Attempting to serve static file: {ctx.File.PhysicalPath}");
                }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/webui/{**path}", "/index.html", new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(webuiPath)
                });
            });
        }
    }
}