// =====================================================================================================
//  SGCAE - Employee Access Management and Control System
//  SGCAE - Sistema de Gestión y Control de Acceso de Empleados
//
//  Author / Autor: Juan Pablo Robledo Urrego
//  Course / Materia: Distributed Programming / Programación Distribuida
//
//  NOTE (EN): Following the professor's feedback on the previous deliverable, all the source code
//             (classes, methods, variables, endpoints) is written in ENGLISH. The comments are written
//             in both English and Spanish to make the solution easier to review.
//  NOTA (ES): Atendiendo la observación del profesor en el entregable anterior, todo el código fuente
//             (clases, métodos, variables, endpoints) está escrito en INGLÉS. Los comentarios están escritos
//             en inglés y en español para facilitar la revisión de la solución.
// =====================================================================================================

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Sgcae.Api.Middleware;
using Sgcae.Application;
using Sgcae.Infrastructure;
using Sgcae.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// EN: Here I register the controllers and configure the enums to travel as text in JSON ("Entry", "Exit"...).
// ES: Aquí registro los controladores y configuro los enums para que viajen como texto en el JSON ("Entry", "Exit"...).
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// EN: Here I configure Swagger (OpenAPI) to document and test the endpoints from the browser.
// ES: Aquí configuro Swagger (OpenAPI) para documentar y probar los endpoints desde el navegador.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SGCAE API",
        Version = "v1",
        Description = "Employee Access Management and Control System (Clean Architecture + DDD + CQRS)."
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// EN: Here I wire the layers: Application (use cases) and Infrastructure (EF Core + SQL Server).
// ES: Aquí conecto las capas: Aplicación (casos de uso) e Infraestructura (EF Core + SQL Server).
var connectionString = builder.Configuration.GetConnectionString("SgcaeDb") ?? string.Empty;
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

// EN: This middleware converts every exception into a standard HTTP response (ProblemDetails).
// ES: Este middleware convierte cada excepción en una respuesta HTTP estándar (ProblemDetails).
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options => options.DocumentTitle = "SGCAE API");

app.MapControllers();

// EN: Here I redirect the root URL to Swagger so the documentation opens directly.
// ES: Aquí redirijo la URL raíz a Swagger para que la documentación abra directamente.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// EN: Here I create/migrate the database and load the initial access points when the API starts.
// ES: Aquí creo/migro la base de datos y cargo los puntos de acceso iniciales cuando la API arranca.
if (app.Configuration.GetValue("Database:InitializeOnStartup", true))
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();

// EN: I expose the Program class so it can be used by integration tests.
// ES: Expongo la clase Program para que pueda usarse en pruebas de integración.
public partial class Program
{
}
