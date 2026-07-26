using System.Text.RegularExpressions;

namespace EmployeeManagement.API;

public class SwaggerSecurityFix
{
    private readonly RequestDelegate _next;

    public SwaggerSecurityFix(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);

            memStream.Seek(0, SeekOrigin.Begin);
            var json = await new StreamReader(memStream).ReadToEndAsync();
            json = Regex.Replace(json, "\"security\":\\s*\\[\\s*\\{\\s*\\}\\s*\\]", "\"security\": [ { \"Bearer\": [] } ]");

            context.Response.Body = originalBody;
            context.Response.ContentLength = null;
            await context.Response.WriteAsync(json);
        }
        else
        {
            await _next(context);
        }
    }
}
