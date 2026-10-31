namespace MiniBilling.Api.Middleware;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Si el request ya trae un Correlation ID, lo usamos; si no, generamos uno nuevo
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Lo guardamos en el contexto para que otros middlewares o controladores lo lean
        context.Items[HeaderName] = correlationId;

        // Lo incluimos en el response para que el cliente lo reciba de vuelta
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}