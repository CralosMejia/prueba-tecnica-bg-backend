using Microsoft.AspNetCore.HttpLogging;
using ShoppingCart.Api.Errors;
using ShoppingCart.Application;
using ShoppingCart.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuración principal de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración del manejo centralizado de errores
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configuración del logging centralizado de peticiones HTTP
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;

    options.CombineLogs = true;
});

// Registro de los servicios de la capa Application
builder.Services.AddApplication();

// Obtención de la cadena de conexión
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'."
    );

// Registro de persistencia, repositorios y servicios de infraestructura
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Registra información básica de cada petición y respuesta HTTP
app.UseHttpLogging();

// Captura las excepciones generadas por los middlewares y endpoints posteriores
app.UseExceptionHandler();

// Swagger solamente se habilita en el ambiente de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Docker actualmente expone la API solamente mediante HTTP.
// Se habilitará cuando exista una configuración HTTPS válida.
// app.UseHttpsRedirection();

// La autenticación y autorización se agregarán al implementar JWT
// app.UseAuthentication();
// app.UseAuthorization();

// Registra los endpoints definidos mediante controllers
app.MapControllers();

app.Run();