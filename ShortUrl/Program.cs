
using Carter;
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
            builder.Services.Configure<DatabaseSettings>
            (
                builder.Configuration.GetSection("DatabaseSettings")
            );

            builder.Services.Configure<DbCollections>(
                builder.Configuration.GetSection("DBCollections")
            );


            // Add CORS services
            builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });

            builder.Services.AddScoped<IUrlService, UrlService>();
            builder.Services.AddScoped<IUrlFactory, UrlFactory>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.MapCarter();

            app.Run();

        }
    }
}
