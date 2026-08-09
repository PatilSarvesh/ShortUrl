
using Carter;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShortUrl.Factories;
using ShortUrl.Models;
using ShortUrl.Services;

namespace ShortUrl
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCarter();

            //DB Service
            builder.Services.AddOptions<DatabaseSettings>()
                .BindConfiguration("DatabaseSettings")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.ConnectionString),
                    "DatabaseSettings:ConnectionString is required.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.DatabaseName),
                    "DatabaseSettings:DatabaseName is required.")
                .ValidateOnStart();

            builder.Services.AddOptions<DbCollections>()
                .BindConfiguration("DBCollections")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.UrlCollection),
                    "DBCollections:UrlCollection is required.")
                .ValidateOnStart();

            builder.Services.AddOptions<UrlSettings>()
                .BindConfiguration("UrlSettings")
                .Validate(settings => Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var uri) &&
                                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                                       string.IsNullOrEmpty(uri.Query) &&
                                       string.IsNullOrEmpty(uri.Fragment),
                    "UrlSettings:BaseUrl must be an absolute HTTP or HTTPS URL without a query or fragment.")
                .ValidateOnStart();

            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            }));

            builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            builder.Services.AddScoped<IUrlService, UrlService>();
            builder.Services.AddScoped<IUrlFactory, UrlFactory>();
            builder.Services.AddHostedService<MongoIndexInitializer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            var webRootProvider = new PhysicalFileProvider(webRootPath);
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = webRootProvider
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = webRootProvider
            });

            app.MapGet("/styles.css", () => Results.File(Path.Combine(webRootPath, "styles.css"), "text/css"));
            app.MapGet("/app.js", () => Results.File(Path.Combine(webRootPath, "app.js"), "text/javascript"));

            app.MapCarter();

            app.Run();

        }
    }
}
