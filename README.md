@"
# Shopping Cart Backend

Esta es la prueba tecnica para BG donde se busca crear un backend modular y desacoplado para gestionar peticiones mediante Apirest.

## Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- Base de datos relacional
- Docker

## Estructura inicial

- `src/`: proyectos de código productivo.
- `tests/`: proyectos de pruebas automatizadas.

# Desiciones arquitectonicas:
Se decidio realizar el back end con una arquitectura limpia. en el cual tendremos por cada proyecto independiente:
- Domain: Entidades y reglas del negocio
- Application: Casos de uso DTO's
- Infraestrutura: EF, repositorios, JWT y servicios externos
- Api: controllers, middlewares y configuracion
- UnitTests: pruebas de las reglas y servicios principalesojk

## Estado

Proyecto en configuración inicial.
"@ | Set-Content README.md