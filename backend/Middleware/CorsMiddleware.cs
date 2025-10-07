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
            var origin = context.Request.Headers["Origin"].ToString();
            Console.WriteLine($"CORS Middleware: {context.Request.Method} {context.Request.Path} from Origin: {origin}");

            // Lista de origens permitidas
            var allowedOrigins = new[]
            {
                "https://vivo-onboarding-site.netlify.app",
                "http://localhost:4200",
                "http://localhost:4201",
                "http://localhost:3000"
            };

            // Determinar qual origem permitir
            var allowOrigin = "*";
            if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
            {
                allowOrigin = origin;
                Console.WriteLine($"CORS: Allowing specific origin: {origin}");
            }
            else
            {
                Console.WriteLine($"CORS: Using wildcard origin for: {origin}");
            }

            // Se for uma requisição OPTIONS (preflight), tratar especificamente
            if (context.Request.Method == "OPTIONS")
            {
                Console.WriteLine("CORS: Handling OPTIONS (preflight) request");
                context.Response.Headers["Access-Control-Allow-Origin"] = allowOrigin;
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With, Accept, Origin";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Access-Control-Max-Age"] = "86400";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("");
                return;
            }

            // Adicionar headers CORS para todas as outras requisições
            context.Response.Headers["Access-Control-Allow-Origin"] = allowOrigin;
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With, Accept, Origin";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "true";

            Console.WriteLine($"CORS: Added headers for {context.Request.Method} request");

            await _next(context);
        }
    }
}