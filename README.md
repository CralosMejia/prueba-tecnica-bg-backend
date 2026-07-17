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
- UnitTests: pruebas de las reglas y servicios principales
- IntegrationTests: prueba del checkout transaccional contra una base MySQL real

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

## Entity Framework Core y migraciones

El proyecto utiliza Entity Framework Core junto con Pomelo para realizar la conexión y persistencia de datos en MySQL.

El contexto principal de la base de datos se encuentra en:

```text
src/ShoppingCart.Infrastructure/Persistence/ShoppingCartDbContext.cs
```

Las configuraciones de las entidades se encuentran en:

```text
src/ShoppingCart.Infrastructure/Persistence/Configurations
```

Las migraciones generadas se almacenan en:

```text
src/ShoppingCart.Infrastructure/Persistence/Migrations
```

### Restaurar la herramienta de Entity Framework

El proyecto utiliza `dotnet-ef` como una herramienta local. Después de clonar el repositorio se debe ejecutar:

```bash
dotnet tool restore
```

Para verificar que la herramienta se encuentra disponible:

```bash
dotnet tool run dotnet-ef --version
```

### Configurar User Secrets

Cuando Entity Framework se ejecuta desde la máquina local, necesita una cadena de conexión para comunicarse con MySQL.

Para evitar guardar contraseñas dentro de `appsettings.json`, se utiliza User Secrets durante el desarrollo local.

Primero se debe inicializar User Secrets en el proyecto de la API:

```bash
dotnet user-secrets init --project src/ShoppingCart.Api
```

Este comando solo se debe ejecutar una vez por proyecto.

Después se debe guardar la cadena de conexión local:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3309;Database=shopping_cart_db;User=shopping_cart_user;Password=YOUR_LOCAL_PASSWORD;" --project src/ShoppingCart.Api
```

Se debe reemplazar `YOUR_LOCAL_PASSWORD` por la misma contraseña configurada en la variable `MYSQL_PASSWORD` del archivo `.env`.

Para revisar los secretos configurados:

```bash
dotnet user-secrets list --project src/ShoppingCart.Api
```

Los User Secrets se almacenan fuera del repositorio, por lo que cada desarrollador debe configurar su propia cadena de conexión después de clonar el proyecto.

La contraseña real no debe guardarse en el README, en `.env.example` ni dentro de los archivos `appsettings`.

### Cadenas de conexión

Cuando la API o Entity Framework se ejecutan directamente desde Windows, la conexión utiliza el puerto publicado por Docker:

```text
Server: localhost
Port: 3309
```

Cuando la API se ejecuta dentro de Docker, se conecta usando el nombre del servicio y el puerto interno de MySQL:

```text
Server: mysql
Port: 3306
```

Docker Compose obtiene las credenciales desde el archivo `.env`.

### Iniciar MySQL

Antes de crear o aplicar migraciones, se debe iniciar la base de datos:

```bash
docker compose up -d mysql
```

Para revisar su estado:

```bash
docker compose ps
```

El contenedor debe aparecer con estado `healthy`.

### Crear una migración

Para crear una nueva migración se debe ejecutar:

```bash
dotnet tool run dotnet-ef migrations add MigrationName --project src/ShoppingCart.Infrastructure --startup-project src/ShoppingCart.Api --output-dir Persistence/Migrations
```

Se debe reemplazar `MigrationName` por un nombre que describa el cambio realizado.

Ejemplo:

```bash
dotnet tool run dotnet-ef migrations add InitialCreate --project src/ShoppingCart.Infrastructure --startup-project src/ShoppingCart.Api --output-dir Persistence/Migrations
```

### Aplicar las migraciones

Para aplicar las migraciones pendientes en MySQL:

```bash
dotnet tool run dotnet-ef database update --project src/ShoppingCart.Infrastructure --startup-project src/ShoppingCart.Api
```

### Eliminar la última migración

Si una migración todavía no fue aplicada y se necesita corregir el modelo:

```bash
dotnet tool run dotnet-ef migrations remove --project src/ShoppingCart.Infrastructure --startup-project src/ShoppingCart.Api
```

Después de corregir el modelo o su configuración, se puede volver a generar la migración.

### Ver migraciones disponibles

```bash
dotnet tool run dotnet-ef migrations list --project src/ShoppingCart.Infrastructure --startup-project src/ShoppingCart.Api
```

Las migraciones forman parte del código fuente y deben mantenerse dentro del repositorio para permitir que otros desarrolladores creen la misma estructura de base de datos.

## Documentación de la API

Todos los endpoints estan documentados en Swagger. Levantando el proyecto (local o Docker) se puede entrar a `/swagger` y ahi aparecen las rutas, los request, los response y los codigos de estado de cada uno.

Para probar los endpoints protegidos hay que autenticarse primero:

1. Ejecutar `POST /api/auth/login` con alguno de los usuarios semilla.
2. Copiar el valor de `accessToken`.
3. Pulsar el botón **Authorize**.
4. Pegar el token.

Con la configuración actual de Swagger no hace falta escribir manualmente la palabra `Bearer`.

## Base de datos y relaciones

La persistencia se maneja en MySQL con estas tablas:

```text
users
products
carts
cart_items
orders
order_items
```

### Carrito

El carrito se relaciona asi:

```text
User 1 ─── 1 Cart
Cart 1 ─── N CartItem
Product 1 ─── N CartItem
```

La tabla `carts` tiene una restricción única sobre `UserId`, para que cada usuario tenga como máximo un carrito.

La tabla `cart_items` usa una clave compuesta:

```text
CartId + ProductId
```

Con esto evito que el mismo producto se guarde dos veces en el mismo carrito. Si se vuelve a agregar un producto que ya está, lo que hago es sumar la cantidad.

El precio no se guarda dentro de `cart_items`. Para armar la respuesta del carrito consulto el precio actual desde `products`. Los precios históricos se guardan después, pero en el detalle de las órdenes.

### Órdenes

Las órdenes usan `Order` y `OrderItem`:

```text
User 1 ─── N Order
Order 1 ─── N OrderItem
Product 1 ─── N OrderItem
```

`Order` es la cabecera de la compra (usuario, fecha, subtotal, descuento y total) y `OrderItem` es cada producto comprado.

Algo importante: en `OrderItem` guardo un **snapshot** del producto (código, nombre y precio unitario al momento de la compra). De esta forma, aunque el producto cambie de precio o nombre después, el historial sigue mostrando lo que el usuario realmente pagó.

Las relaciones `User → Order` y `Product → OrderItem` usan eliminación restringida para no perder el historial si se llega a borrar un usuario o un producto. Los `OrderItem` solo se borran en cascada cuando se borra su `Order`.

### Borrado lógico de productos

Los productos no se borran físicamente. Uso un campo `IsActive` para desactivarlos. Decidi hacerlo asi porque las órdenes referencian productos, y borrarlos de verdad rompería el historial de compras. Los endpoints públicos solo devuelven productos activos; el administrador puede ver también los inactivos.

## Autenticación, usuarios y autorización

La autenticación la manejo con JSON Web Token (JWT).

Los usuarios se guardan en MySQL y las contraseñas nunca se guardan en texto plano, solo un hash generado con `PasswordHasher`.

El flujo del login es este:

```text
POST /api/auth/login
        ↓
AuthController
        ↓
AuthService
        ↓
UserRepository
        ↓
MySQL
        ↓
Verificación de contraseña
        ↓
Generación del token JWT
```

Cuando el correo no existe o la contraseña es incorrecta devuelvo el mismo mensaje. Asi no revelo qué correos están registrados.

### Roles disponibles

Manejo dos roles guardados como texto:

- `Customer`: usuario normal de la aplicación.
- `Admin`: usuario con acceso a la administración de productos.

El rol va dentro del JWT como un claim, lo que me permite proteger endpoints con:

```csharp
[Authorize(Roles = UserRoles.Admin)]
```

### Usuarios semilla

La migración crea estos usuarios de desarrollo:

| Rol | Correo | Contraseña |
|---|---|---|
| Customer | `customer@shoppingcart.com` | `Customer123!` |
| Admin | `admin@shoppingcart.com` | `Admin123!` |

Estas credenciales son únicamente para desarrollo y evaluación técnica.

Las contraseñas no se guardan directamente. `UserConfiguration` tiene hashes previamente generados y fijos para evitar cambios innecesarios entre migraciones.

### Configuración JWT

La parte pública del token está en `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "ShoppingCart.Api",
    "Audience": "ShoppingCart.Client",
    "ExpirationMinutes": 60
  }
}
```

La clave privada para firmar los tokens no se guarda en Git.

### Generar una clave JWT

Desde PowerShell:

```powershell
$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()

$jwtKey = [Convert]::ToBase64String($bytes)
$jwtKey
```

El comando genera una clave aleatoria en Base64. No se debe compartir ni subir al repositorio.

### Guardar la clave para ejecución local

La API usa User Secrets en local.

Inicializar User Secrets, si todavía no están configurados:

```powershell
dotnet user-secrets init `
  --project src/ShoppingCart.Api
```

Guardar la clave:

```powershell
dotnet user-secrets set "Jwt:Key" $jwtKey `
  --project src/ShoppingCart.Api
```

Verificar lo guardado:

```powershell
dotnet user-secrets list `
  --project src/ShoppingCart.Api
```

También debe existir la cadena de conexión local:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DefaultConnection" `
  "Server=localhost;Port=3309;Database=shopping_cart_db;User=shopping_cart_user;Password=YOUR_PASSWORD;" `
  --project src/ShoppingCart.Api
```

### Configuración JWT para Docker

El archivo local `.env` debe contener:

```env
JWT_ISSUER=ShoppingCart.Api
JWT_AUDIENCE=ShoppingCart.Client
JWT_EXPIRATION_MINUTES=60
JWT_KEY=YOUR_GENERATED_SECRET
```

El archivo `.env.example` solo tiene valores de referencia:

```env
JWT_ISSUER=ShoppingCart.Api
JWT_AUDIENCE=ShoppingCart.Client
JWT_EXPIRATION_MINUTES=60
JWT_KEY=replace_with_a_secure_key
```

Docker Compose convierte estas variables a la estructura de configuración de ASP.NET Core:

```yaml
environment:
  Jwt__Issuer: ${JWT_ISSUER}
  Jwt__Audience: ${JWT_AUDIENCE}
  Jwt__ExpirationMinutes: ${JWT_EXPIRATION_MINUTES}
  Jwt__Key: ${JWT_KEY}
```

Los dobles guiones bajos representan secciones anidadas:

```text
Jwt__Key → Jwt:Key
```

### Seguridad

- La clave JWT no se guarda en Git.
- Las contraseñas no se guardan en texto plano.
- Los tokens y contraseñas no se registran en el HTTP logging.
- El JWT contiene el identificador, correo y rol del usuario.
- Los tokens tienen expiración configurable.
- La validación verifica firma, issuer, audience y expiración.

## Checkout transaccional

El checkout es la parte mas delicada, porque tengo que crear la orden, disminuir el stock y vaciar el carrito, y todo eso tiene que pasar junto o no pasar nada.

### Reglas de negocio

- El usuario debe estar autenticado.
- La compra usa el carrito persistente del usuario. El cliente no vuelve a enviar los productos.
- El carrito debe tener al menos un producto.
- Todos los productos del carrito deben existir.
- El stock se vuelve a consultar y validar durante el checkout.
- No se permite comprar más de lo disponible.
- Agregar al carrito no reserva ni disminuye stock.
- El stock disminuye solo cuando la compra se confirma.
- La orden guarda el código, nombre y precio vigentes al momento del checkout.
- Se aplica un descuento del 10 % cuando el subtotal es estrictamente mayor a `$100`. Si es exactamente `$100`, no hay descuento.
- Después de una compra exitosa, el carrito queda vacío.
- Cada usuario solo puede ver sus propias órdenes.

Todas las validaciones de existencia y stock las hago antes de tocar nada. Asi evito modificaciones parciales cuando alguno de los productos no se puede comprar.

### Orden del checkout

```text
1. Iniciar la estrategia de ejecución de MySQL.
2. Iniciar una transacción con aislamiento Serializable.
3. Obtener el carrito del usuario autenticado.
4. Rechazar el checkout si el carrito está vacío.
5. Obtener los productos con seguimiento de EF Core.
6. Verificar que todos los productos existan.
7. Validar el stock de todos los productos.
8. Construir el snapshot de los productos.
9. Crear la orden y sus detalles.
10. Disminuir el stock.
11. Vaciar el carrito.
12. Ejecutar SaveChangesAsync.
13. Confirmar la transacción.
```

### La transacción

Toda la operación pasa por la abstracción:

```text
IUnitOfWork
```

La capa `Application` define el contrato y la capa `Infrastructure` lo implementa con:

```text
EfUnitOfWork
```

Dentro de la misma unidad de trabajo entra:

```text
Creación de Order
+ creación de OrderItems
+ disminución de Product.Stock
+ eliminación de CartItems
```

Los repositorios y `EfUnitOfWork` comparten la misma instancia `Scoped` de `ShoppingCartDbContext`, por eso todos los cambios se confirman con una sola llamada:

```csharp
await dbContext.SaveChangesAsync(cancellationToken);
```

Si todo sale bien, confirmo:

```csharp
await transaction.CommitAsync(cancellationToken);
```

Si algo revienta, hago rollback:

```csharp
await transaction.RollbackAsync(CancellationToken.None);
```

Uso `CancellationToken.None` en el rollback para que una cancelación de la petición no impida revertir la transacción. Después del rollback limpio el `ChangeTracker` para no quedarme con entidades modificadas si la estrategia de ejecución reintenta.

### Nivel de aislamiento

El checkout usa:

```csharp
IsolationLevel.Serializable
```

Este nivel protege el stock frente a checkouts concurrentes y reduce el riesgo de que dos compras se confirmen usando las mismas unidades disponibles. El stock se valida dentro de la transacción, con el valor actual en la base de datos y no con el que tenía cuando se agregó al carrito.

### Estrategia de reintentos de MySQL

La conexión usa una estrategia de reintentos para errores transitorios de MySQL.

El problema es que una transacción iniciada a mano no se puede ejecutar directamente cuando está activa `MySqlRetryingExecutionStrategy`. Por eso `EfUnitOfWork` obtiene la estrategia con:

```csharp
var executionStrategy =
    dbContext.Database.CreateExecutionStrategy();
```

Y toda la unidad transaccional se ejecuta dentro de:

```csharp
await executionStrategy.ExecuteAsync(...);
```

La estructura queda asi:

```text
CreateExecutionStrategy
└── ExecuteAsync
    └── BeginTransactionAsync
        ├── consultar carrito
        ├── consultar productos
        ├── validar stock
        ├── crear orden
        ├── disminuir stock
        ├── vaciar carrito
        ├── SaveChangesAsync
        └── CommitAsync
```

De esta forma combino los reintentos de conexión con una transacción explícita.

## Manejo de errores

Uso un manejador global de excepciones para no repetir bloques `try/catch` en cada controller.

Los errores se devuelven con el formato `ProblemDetails` e incluyen un `traceId` que permite relacionar la respuesta con los logs.

Los códigos principales son:

- `400`: datos o argumentos inválidos.
- `401`: token ausente, inválido o expirado.
- `403`: el usuario no tiene el rol necesario.
- `404`: recurso no encontrado.
- `409`: conflicto con una regla de negocio (carrito vacío, stock insuficiente).
- `500`: error interno no controlado.
- `503`: la base de datos no está disponible.

Los detalles técnicos y stack traces quedan en los logs, no se exponen en las respuestas.

## Logging

La API usa el logging de ASP.NET Core con `ILogger`.

El registro está centralizado y guarda info básica de cada petición:

- Método HTTP.
- Ruta solicitada.
- Código de respuesta.
- Tiempo de ejecución.

Las excepciones no controladas las registra el manejador global con un `traceId`.

Por seguridad, no registro cuerpos de peticiones, contraseñas, tokens JWT ni encabezados de autorización.

Para ver los logs de la API en Docker:

```bash
docker compose -f compose.yaml -f compose.dev.yaml logs -f api
```

## Pruebas

El proyecto lo he desarrollado con tdd para poder probar cada funcionalidad.

El ciclo que uso es:

1. **Red:** crear una prueba y comprobar que falla.
2. **Green:** implementar el código mínimo para que pase.
3. **Refactor:** mejorar el código sin cambiar su comportamiento.
4. Volver a ejecutar todas las pruebas antes del commit.

Las pruebas unitarias están en:

```text
tests/ShoppingCart.UnitTests
````

### Ejecutar todas las pruebas

Desde la raíz del backend:

```bash
dotnet test ShoppingCart.sln
```

### Ejecutar solo las pruebas unitarias

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj
```

### Ejecutar una prueba o grupo específico

Con un filtro por nombre de la clase:

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj --filter "FullyQualifiedName~CartTests"
```

O por nombre de método:

```bash
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj --filter "FullyQualifiedName~CalculateTotals"
```

### Pruebas de integración

Ademas de las unitarias hay pruebas de integración en:

```text
tests/ShoppingCart.IntegrationTests
```

Estas levantan un contenedor MySQL real con Testcontainers, asi que se necesita Docker corriendo. La prueba principal valida el rollback del checkout: fuerzo un error antes del commit y compruebo directo en MySQL que no quedó nada a medias (no se creó la orden, no se creó el detalle, el stock no bajó y el carrito sigue igual).

Para ejecutarlas:

```bash
dotnet test tests/ShoppingCart.IntegrationTests/ShoppingCart.IntegrationTests.csproj
```

## Resumen de decisiones técnicas

Un resumen rápido de las decisiones más importantes que tomé:

- **Clean Architecture:** separé la solución en Domain, Application, Infrastructure y Api. Las dependencias solo van hacia adentro, asi puedo probar la lógica de negocio sin base de datos y cambiar detalles técnicos sin tocar el dominio.
- **Aggregate Roots:** `Cart` y `Order` controlan a sus hijos. Los `CartItem` solo se modifican a través del carrito, para que las reglas siempre se cumplan.
- **Lógica de negocio en el dominio:** el cálculo de totales y el descuento del 10 % viven en `Cart.CalculateTotals()` y `Order.Create()`, no en los controllers ni en los servicios.
- **Snapshot de precios:** los `OrderItem` guardan código, nombre y precio del momento de la compra, asi el historial no cambia si el producto cambia después.
- **Transacción Serializable:** el checkout va dentro de una transacción con `IsolationLevel.Serializable` a través de `IUnitOfWork`, para que dos compras al mismo tiempo no consuman el mismo stock.
- **Doble validación de stock:** valido al agregar al carrito (cantidad acumulada) y otra vez dentro de la transacción del checkout, porque el stock pudo cambiar entre esos dos momentos.
- **Borrado lógico:** los productos se desactivan con `IsActive` en vez de borrarse, para no romper el historial de compras.
- **DTOs:** nunca expongo las entidades directamente. Todo entra y sale con records de la capa Application.
- **Manejo centralizado de errores:** un `GlobalExceptionHandler` convierte las excepciones de negocio en `ProblemDetails` con el código HTTP correcto y un `traceId`.
- **Configuración segura:** las cadenas de conexión y la clave JWT no van al repositorio. En local uso User Secrets y en Docker variables de entorno con `.env`.
- **TDD:** desarrollé con el ciclo Red-Green-Refactor. Las unitarias usan fakes y las de integración validan la transacción contra MySQL real.
