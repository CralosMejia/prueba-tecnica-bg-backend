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
## Ejecutar el proyecto con Docker

El proyecto utiliza Docker Compose para levantar la API y la base de datos MySQL.

Antes de iniciar los contenedores se debe crear el archivo `.env` usando como referencia `.env.example`:

```bash
copy .env.example .env
```

### Primera ejecución con Hot Reload

La primera vez se debe construir la imagen de desarrollo:

```bash
docker compose -f compose.yaml -f compose.dev.yaml up --build --watch
```

Este comando levanta:

- La API de ASP.NET Core.
- La base de datos MySQL.
- El entorno de desarrollo con Hot Reload.

Después de construir la imagen por primera vez, el entorno se puede iniciar con:

```bash
docker compose -f compose.yaml -f compose.dev.yaml up --watch
```

Los cambios realizados dentro de `src` se sincronizan con el contenedor y `dotnet watch` actualiza o reinicia la API automáticamente.

Se debe volver a utilizar `--build` cuando se modifique alguno de estos archivos:

- `Dockerfile`.
- Archivos `.csproj`.
- Paquetes NuGet.
- Configuración de construcción de Docker Compose.

### Ejecución sin Hot Reload

Para comprobar la versión final de la aplicación utilizando la imagen de runtime:

```bash
docker compose -f compose.yaml up --build
```

Después de construir la imagen, las siguientes ejecuciones pueden realizarse sin `--build`:

```bash
docker compose -f compose.yaml up
```

### Acceso a la aplicación

Una vez iniciados los contenedores, Swagger estará disponible en:

```text
http://localhost:8080/swagger
```

El estado de la API se puede comprobar desde:

```text
http://localhost:8080/api/health
```

MySQL estará disponible desde el equipo local en:

```text
Host: localhost
Port: 3307
```

Dentro de Docker, la API se conecta a MySQL usando el nombre del servicio `mysql` y el puerto interno `3306`.

### Detener los contenedores

Si Docker Compose se está ejecutando en primer plano, se debe presionar:

```text
Ctrl + C
```

Después se pueden eliminar los contenedores con:

```bash
docker compose -f compose.yaml -f compose.dev.yaml down
```

Para detener la ejecución sin Hot Reload:

```bash
docker compose -f compose.yaml down
```

Los datos de MySQL se mantienen almacenados en el volumen de Docker.

No se debe utilizar `down -v` a menos que se quiera eliminar también la base de datos y sus datos.



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