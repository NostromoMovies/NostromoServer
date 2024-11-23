using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Nostromo.Server.Database;
using Nostromo.Server.Database.Repositories;
using Nostromo.Server.Scheduling;
using Nostromo.Server.Server;
using Nostromo.Server.Services;
using Nostromo.Server.Settings;
using Nostromo.Server.Utilities;
using Nostromo.Server.Utilities.FileSystemWatcher;

namespace Nostromo.Server;
public static class Program
{
    public static async Task<WebApplication> CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(settings =>
        {
            settings.Title = "Nostromo API";
            settings.Version = "v1";

            // Add this to ensure controller discovery
            settings.UseControllerSummaryAsTagDescription = true;
        });


        // CORS setup
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Development", builder =>
            {
                builder.WithOrigins("http://localhost:5173")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        builder.Services.AddNostromoDatabase(builder.Configuration);
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IDatabaseService, DatabaseService>();
        builder.Services.Configure<WatcherSettings>(builder.Configuration.GetSection("WatcherSettings"));

        builder.Services.AddSingleton<RecoveringFileSystemWatcher>(sp =>
        {
            var watchPath = builder.Configuration.GetValue<string>("WatchSettings:Path")
                ?? throw new InvalidOperationException("Watch path not configured");
            return new RecoveringFileSystemWatcher(watchPath);
        });

        builder.Services.AddQuartzServices();

        builder.Services.AddSingleton<ISettingsProvider>(services =>
        {
            var logger = services.GetRequiredService<ILogger<SettingsProvider>>();
            return new SettingsProvider(logger);
        });

        var settingsProvider = new SettingsProvider(
            builder.Services.BuildServiceProvider().GetRequiredService<ILogger<SettingsProvider>>()
        );

        builder.Services.AddSingleton<FileWatcherService>();
        builder.Services.AddSingleton<NostromoServer>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            var settings = settingsProvider.GetSettings();
            options.ListenAnyIP(settings.ServerPort);
        });

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseCors("Development");
        }

        app.UseOpenApi();
        app.UseSwaggerUi();
                app.MapControllers();
        // Order is important here
        app.UseRouting();

        app.UseCors("Development");  // Added this to ensure CORS is handled
        app.UseAuthorization();

        // Static Files Configuration
        var webuiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webui");
        if (!Directory.Exists(webuiPath))
        {
            throw new DirectoryNotFoundException($"WebUI directory not found at: {webuiPath}");
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(webuiPath),
            RequestPath = "/webui"
        });

        // API Endpoints must be mapped AFTER UseRouting and BEFORE the SPA handler


        // SPA fallback middleware
        app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.StatusCode == 404 &&
                context.Request.Path.StartsWithSegments("/webui") &&
                !Path.HasExtension(context.Request.Path.Value))
            {
                var indexPath = Path.Combine(webuiPath, "index.html");
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(indexPath);
            }
        });

        // Redirect root to /webui
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/webui");
                return;
            }
            await next();
        });

        // Initialize database
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NostromoDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        // Start additional services
        var fileWatcherService = app.Services.GetRequiredService<FileWatcherService>();
        await fileWatcherService.StartAsync(CancellationToken.None);

        var nostromoServer = app.Services.GetRequiredService<NostromoServer>();
        if (!nostromoServer.StartUpServer())
        {
            throw new Exception("Failed to start Nostromo server");
        }

        Utils.ServiceContainer = app.Services;
        Utils.NostromoServer = nostromoServer;
        Utils.SettingsProvider = app.Services.GetRequiredService<ISettingsProvider>();

        return app;
    }

    public static async Task Main(string[] args)
    {
        var app = await CreateApp(args);
        await app.RunAsync();
    }
}