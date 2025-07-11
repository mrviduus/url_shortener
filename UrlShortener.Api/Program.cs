using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var keyVaultName = builder.Configuration["KeyVaultName"];
if(!string.IsNullOrEmpty(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "URL Shortener API", 
        Version = "v1",
        Description = "A simple URL shortener service API",
        Contact = new()
        {
            Name = "URL Shortener Team"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at the app's root
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// URL Shortener API endpoints
app.MapPost("/shorten", (ShortenUrlRequest request) =>
{
    // TODO: Implement URL shortening logic
    var shortCode = GenerateShortCode();
    var shortenedUrl = $"{request.BaseUrl?.TrimEnd('/')}/s/{shortCode}";
    
    return Results.Ok(new ShortenUrlResponse(shortenedUrl, shortCode));
})
.WithName("ShortenUrl")
.WithOpenApi(operation => new(operation)
{
    Summary = "Shorten a URL",
    Description = "Creates a shortened version of the provided URL",
    Tags = [new() { Name = "URL Operations" }]
});

app.MapGet("/s/{shortCode}", (string shortCode) =>
{
    // TODO: Implement URL resolution and redirect logic
    // For now, return a placeholder
    return Results.Redirect("https://example.com");
})
.WithName("RedirectToOriginalUrl")
.WithOpenApi(operation => new(operation)
{
    Summary = "Redirect to original URL",
    Description = "Redirects to the original URL using the short code",
    Tags = [new() { Name = "URL Operations" }]
});

app.MapGet("/info/{shortCode}", (string shortCode) =>
{
    // TODO: Implement URL info retrieval logic
    return Results.Ok(new UrlInfo(
        shortCode, 
        "https://example.com", 
        DateTime.UtcNow, 
        0
    ));
})
.WithName("GetUrlInfo")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get URL information",
    Description = "Retrieves information about a shortened URL",
    Tags = [new() { Name = "URL Operations" }]
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
.WithName("HealthCheck")
.WithOpenApi(operation => new(operation)
{
    Summary = "Health check",
    Description = "Returns the health status of the API",
    Tags = [new() { Name = "System" }]
});

app.Run();

// Helper method to generate short codes
static string GenerateShortCode()
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, 6)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}

// DTOs
public record ShortenUrlRequest(string Url, string? BaseUrl = null);
public record ShortenUrlResponse(string ShortenedUrl, string ShortCode);
public record UrlInfo(string ShortCode, string OriginalUrl, DateTime CreatedAt, int ClickCount);
