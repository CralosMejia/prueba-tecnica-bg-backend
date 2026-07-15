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

## Dependencias entre capas

La solución mantiene las siguientes reglas de dependencia:

- `Domain` no depende de otros proyectos.
- `Application` depende de `Domain`.
- `Infrastructure` depende de `Application` y `Domain`.
- `Api` depende de `Application` e `Infrastructure`.
- `UnitTests` prueba principalmente `Domain` y `Application`.

De esta forma mantenemos la solución desacoplada


## Cómo iniciar el proyecto

Para iniciar el backend de forma local se debe tener instalado .NET 8.

Primero se restauran las dependencias de la solución:

```bash
dotnet restore
````

Luego se compila el proyecto para verificar que no existan errores:

```bash
dotnet build ShoppingCart.sln
```

Para iniciar la API usando el perfil HTTPS:

```bash
dotnet run --project src/ShoppingCart.Api --launch-profile https
```
Para iniciar la API usando el perfil HTTPS con Hotreload:

```bash
dotnet watch --project src/ShoppingCart.Api run --launch-profile https
```

Una vez iniciado el proyecto, Swagger estará disponible en:

```text
https://localhost:7105/swagger
```

También se puede acceder mediante HTTP:

```text
http://localhost:5123/swagger
```

Los puertos pueden cambiar dependiendo de la configuración del archivo `launchSettings.json`.

Si aparece una advertencia relacionada con el certificado HTTPS, se debe ejecutar:

```bash
dotnet dev-certs https --trust
```

Para detener la aplicación se debe presionar:

```text
Ctrl + C
```

## Pruebas

El proyecto lo he desarrollado con tdd para poder probar cada funcionalida

El ciclo que se utiliza es:

1. **Red:** crear una prueba y comprobar que falla.
2. **Green:** implementar el código mínimo necesario para que la prueba pase.
3. **Refactor:** mejorar el código sin cambiar su comportamiento.
4. Ejecutar nuevamente todas las pruebas antes de realizar el commit.

Las pruebas unitarias se encuentran dentro del proyecto:

```text
tests/ShoppingCart.UnitTests
````

### Ejecutar todas las pruebas

Desde la raíz del backend:

```bash
dotnet test ShoppingCart.sln
```

### Ejecutar solo el proyecto de pruebas unitarias

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj
```

### Ejecutar una prueba o grupo específico

Se puede utilizar un filtro con el nombre de la clase de prueba:

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj --filter "FullyQualifiedName~CartTests"
```

También se puede filtrar por el nombre de un método:

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj --filter "FullyQualifiedName~CalculateTotals"
```

### Ejecutar pruebas mientras se modifica el código

Para ejecutar automáticamente las pruebas cada vez que se guarda un cambio:

```bash
dotnet watch --project tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj test
```

Antes de realizar un commit se deben ejecutar todas las pruebas para confirmar que los cambios nuevos no rompieron funcionalidades anteriores:

```bash
dotnet test ShoppingCart.sln
```



## Estado

Proyecto en configuración inicial.
"@ | Set-Content README.md