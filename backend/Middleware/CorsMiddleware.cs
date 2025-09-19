namespace backend.Middleware
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine($"CORS Middleware: {context.Request.Method} {context.Request.Path}");

            // Se for uma requisição OPTIONS (preflight), tratar especificamente
            if (context.Request.Method == "OPTIONS")
            {
                Console.WriteLine("CORS: Handling OPTIONS request");
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
                context.Response.Headers["Access-Control-Max-Age"] = "86400";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("");
                return;
            }

            // Adicionar headers CORS para todas as outras requisições
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";

            await _next(context);
        }
    }
}