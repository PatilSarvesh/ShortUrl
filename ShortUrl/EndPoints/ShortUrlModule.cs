using Carter;
using ShortUrl.Factories;

namespace ShortUrl.EndPoints
{
    public class ShortUrlModule : CarterModule
    {
        public override void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/ShortenUrl", async (IUrlFactory urlFactory, string destinationUrl) =>
            {
                if(!Uri.TryCreate(destinationUrl, UriKind.Absolute, out _)){
                    return Results.BadRequest("The Specified Url is not valid.");
                }
                var url = await urlFactory.GenerateShortenUrlAsync(destinationUrl);
                return Results.Ok(url);
            });

            app.MapGet("/{shortCode}", async(IUrlFactory urlFactory, string shortCode)=>
            {
                var res = await urlFactory.GetDestinationUrl(shortCode);
                return Results.Redirect(res);
            });
        }

    }
}