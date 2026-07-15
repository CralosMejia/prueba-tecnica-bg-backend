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

## Logging

La API utiliza el sistema de logging incluido en ASP.NET Core mediante `ILogger`.

El registro se encuentra centralizado y permite guardar información básica de cada petición:

- Método HTTP.
- Ruta solicitada.
- Código de respuesta.
- Tiempo de ejecución.

Las excepciones no controladas son registradas por el manejador global e incluyen un `traceId` para relacionar el error devuelto al cliente con los logs de la aplicación.

Por seguridad, no se registran cuerpos de peticiones, contraseñas, tokens JWT ni encabezados de autorización.

Para revisar los logs de la API ejecutada con Docker:

```bash
docker compose -f compose.yaml -f compose.dev.yaml logs api
```

Para observar los logs en tiempo real:

```bash
docker compose -f compose.yaml -f compose.dev.yaml logs -f api
```

## Estado
## Productos

El módulo de productos permite consultar los productos almacenados y realizar búsquedas por nombre, código o categoría.

### Listar productos

```http
GET /api/products
```

Si no existen productos, devuelve una lista vacía con estado `200`.

### Buscar productos

```http
GET /api/products?search=keyboard
```

El parámetro `search` compara el texto ingresado con el nombre, código y categoría del producto.

### Consultar un producto

```http
GET /api/products/{id}
```

Si el producto no existe, la API devuelve `404`.

La base de datos contiene productos semilla para poder probar estos endpoints después de aplicar las migraciones.

## Manejo de errores

La API utiliza un manejador global de excepciones para evitar repetir bloques `try/catch` dentro de cada controller.

Los errores se devuelven usando el formato `ProblemDetails` e incluyen un `traceId` que permite relacionar la respuesta con los logs de la aplicación.

Los códigos principales utilizados son:

- `400`: datos o argumentos inválidos.
- `404`: recurso no encontrado.
- `409`: conflicto con una regla de negocio.
- `500`: error interno no controlado.
- `503`: la base de datos no se encuentra disponible.

Los detalles técnicos y stack traces se registran en los logs, pero no se exponen en las respuestas HTTP.

## Autenticación, usuarios y autorización

La API implementa autenticación mediante JSON Web Token (JWT).

Los usuarios se almacenan en MySQL y sus contraseñas no se guardan en texto plano. La base de datos conserva únicamente un hash generado mediante `PasswordHasher`.

La autenticación sigue el siguiente flujo:

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

### Roles disponibles

Actualmente se manejan dos roles almacenados como texto:

- `Customer`: usuario normal de la aplicación.
- `Admin`: usuario preparado para funcionalidades administrativas opcionales.

El rol se incluye dentro del JWT como un claim. Esto permitirá proteger endpoints administrativos utilizando:

```csharp
[Authorize(Roles = UserRoles.Admin)]
```

Los endpoints de consulta de productos están disponibles para cualquier usuario autenticado.

### Usuarios semilla

La migración de Entity Framework crea los siguientes usuarios de desarrollo:

| Rol | Correo | Contraseña |
|---|---|---|
| Customer | `customer@shoppingcart.com` | `Customer123!` |
| Admin | `admin@shoppingcart.com` | `Admin123!` |

Estas credenciales son únicamente para desarrollo y evaluación técnica.

Las contraseñas no se almacenan directamente en la base de datos. `UserConfiguration` contiene hashes previamente generados y fijos para evitar cambios innecesarios entre migraciones.

### Configuración JWT

La configuración pública del token se encuentra en `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "ShoppingCart.Api",
    "Audience": "ShoppingCart.Client",
    "ExpirationMinutes": 60
  }
}
```

La clave privada utilizada para firmar los tokens no se almacena en Git.

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

El comando genera una clave aleatoria codificada en Base64.

No se debe compartir ni agregar esta clave al repositorio.

### Guardar la clave para ejecución local

La API utiliza User Secrets durante la ejecución local.

Inicializar User Secrets, en caso de que todavía no estén configurados:

```powershell
dotnet user-secrets init `
  --project src/ShoppingCart.Api
```

Guardar la clave:

```powershell
dotnet user-secrets set "Jwt:Key" $jwtKey `
  --project src/ShoppingCart.Api
```

Verificar las configuraciones guardadas:

```powershell
dotnet user-secrets list `
  --project src/ShoppingCart.Api
```

También debe existir una cadena de conexión local:

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

El archivo `.env` no debe subirse al repositorio.

El archivo `.env.example` contiene únicamente valores de referencia:

```env
JWT_ISSUER=ShoppingCart.Api
JWT_AUDIENCE=ShoppingCart.Client
JWT_EXPIRATION_MINUTES=60
JWT_KEY=replace_with_a_secure_key
```

Docker Compose convierte estas variables a la estructura de configuración utilizada por ASP.NET Core:

```yaml
environment:
  Jwt__Issuer: ${JWT_ISSUER}
  Jwt__Audience: ${JWT_AUDIENCE}
  Jwt__ExpirationMinutes: ${JWT_EXPIRATION_MINUTES}
  Jwt__Key: ${JWT_KEY}
```

Los dobles guiones bajos representan secciones anidadas de configuración:

```text
Jwt__Key → Jwt:Key
```

### Ejecutar migraciones

Crear una migración:

```powershell
dotnet ef migrations add AddUsers `
  --project src/ShoppingCart.Infrastructure `
  --startup-project src/ShoppingCart.Api `
  --output-dir Persistence/Migrations
```

Aplicar las migraciones:

```powershell
dotnet ef database update `
  --project src/ShoppingCart.Infrastructure `
  --startup-project src/ShoppingCart.Api
```

La migración crea:

- La tabla `users`.
- Un índice único para el correo electrónico.
- Un usuario `Customer`.
- Un usuario `Admin`.
- Los hashes de las contraseñas.

### Endpoint de login

```http
POST /api/auth/login
```

Request:

```json
{
  "email": "customer@shoppingcart.com",
  "password": "Customer123!"
}
```

Respuesta exitosa:

```json
{
  "accessToken": "JWT_TOKEN",
  "expiresAtUtc": "2026-07-16T12:00:00Z",
  "email": "customer@shoppingcart.com",
  "role": "Customer",
  "tokenType": "Bearer"
}
```

### Códigos HTTP del login

| Código | Significado |
|---|---|
| `200 OK` | Credenciales válidas y token generado. |
| `400 Bad Request` | Correo, contraseña o formato del request inválido. |
| `401 Unauthorized` | Correo o contraseña incorrectos. |
| `500 Internal Server Error` | Error interno no controlado. |
| `503 Service Unavailable` | La base de datos no está disponible. |

La API devuelve el mismo mensaje cuando el correo no existe o la contraseña es incorrecta. Esto evita revelar qué correos están registrados.

### Usar JWT en Swagger

1. Ejecutar `POST /api/auth/login`.
2. Copiar únicamente el valor de `accessToken`.
3. Pulsar el botón **Authorize**.
4. Pegar el token.
5. Confirmar la autorización.
6. Ejecutar los endpoints protegidos.

Con la configuración actual de Swagger no es necesario escribir manualmente la palabra `Bearer`.

### Endpoints protegidos

Los endpoints de productos requieren un usuario autenticado:

```http
GET /api/products
GET /api/products/{id}
```

Una petición sin token o con un token inválido devuelve:

```text
401 Unauthorized
```

### Seguridad

- La clave JWT no se almacena en Git.
- Las contraseñas no se almacenan en texto plano.
- Los tokens y contraseñas no se registran mediante HTTP logging.
- El JWT contiene el identificador, correo y rol del usuario.
- Los tokens tienen un tiempo de expiración configurable.
- La validación verifica firma, issuer, audience y expiración.


## Carrito de compras

La API implementa un carrito de compras persistente para cada usuario autenticado.

Cada carrito pertenece a un único usuario y sus operaciones se realizan utilizando el identificador almacenado en el JWT. El cliente no envía el `UserId` en la ruta ni en el cuerpo de la petición.

### Flujo de identificación del usuario

```text
JWT
  ↓
ClaimTypes.NameIdentifier
  ↓
CartController
  ↓
CartService
  ↓
CartRepository
  ↓
Carrito perteneciente al usuario autenticado
```

Esto evita que un usuario pueda consultar o modificar el carrito de otra persona.

### Modelo relacional

El carrito utiliza las siguientes relaciones:

```text
User 1 ─── 1 Cart
Cart 1 ─── N CartItem
Product 1 ─── N CartItem
```

Tablas involucradas:

```text
users
products
carts
cart_items
```

La tabla `carts` contiene una restricción única sobre `UserId`, garantizando que cada usuario tenga como máximo un carrito activo.

La tabla `cart_items` utiliza una clave compuesta:

```text
CartId + ProductId
```

Esto impide que el mismo producto se almacene varias veces dentro del mismo carrito. Cuando se vuelve a agregar un producto existente, se incrementa su cantidad.

### Reglas de negocio

El carrito implementa las siguientes reglas:

- Las cantidades deben ser mayores que cero.
- El producto debe existir.
- La cantidad solicitada no puede superar el stock disponible.
- Al agregar nuevamente un producto, se valida la cantidad acumulada.
- Al actualizar un producto, la nueva cantidad reemplaza a la anterior.
- Un usuario solo puede acceder a su propio carrito.
- El subtotal por producto se calcula multiplicando el precio actual por la cantidad.
- El subtotal general corresponde a la suma de todos los productos.
- Se aplica un descuento del 10 % cuando el subtotal es estrictamente mayor que `$100`.
- Cuando el subtotal es exactamente `$100`, no se aplica descuento.

### Manejo del stock

Agregar un producto al carrito no disminuye el stock.

```text
Agregar al carrito
→ validar stock disponible
→ guardar CartItem
→ no modificar Product.Stock
```

Eliminar un producto o vaciar el carrito tampoco aumenta el stock, porque las unidades nunca fueron reservadas.

El stock se modificará únicamente durante el checkout:

```text
Confirmar compra
→ validar nuevamente el stock
→ crear la orden
→ disminuir el stock
→ vaciar el carrito
→ confirmar la transacción
```

La validación se realiza nuevamente durante el checkout porque otro usuario podría comprar las unidades disponibles antes de que se confirme la compra.

### Respuesta del carrito

Ejemplo:

```json
{
  "items": [
    {
      "productId": "11111111-1111-1111-1111-111111111111",
      "code": "PROD-001",
      "name": "Mechanical Keyboard",
      "unitPrice": 50,
      "quantity": 3,
      "subtotal": 150
    }
  ],
  "subtotal": 150,
  "discount": 15,
  "total": 135
}
```

El precio no se almacena dentro de `cart_items`. Para construir la respuesta, la API consulta el precio actual desde la tabla `products`.

Los precios históricos se almacenarán posteriormente en los detalles de las órdenes.

### Obtener el carrito

```http
GET /api/cart
```

Una respuesta para un carrito vacío es:

```json
{
  "items": [],
  "subtotal": 0,
  "discount": 0,
  "total": 0
}
```

Código esperado:

```text
200 OK
```

### Agregar un producto

```http
POST /api/cart/items
```

Request:

```json
{
  "productId": "11111111-1111-1111-1111-111111111111",
  "quantity": 2
}
```

Código esperado:

```text
201 Created
```

Si el producto ya está en el carrito, la cantidad enviada se suma a la cantidad existente.

### Actualizar una cantidad

```http
PUT /api/cart/items/{productId}
```

Request:

```json
{
  "quantity": 4
}
```

La cantidad enviada reemplaza a la cantidad anterior.

Código esperado:

```text
200 OK
```

### Eliminar un producto

```http
DELETE /api/cart/items/{productId}
```

Código esperado:

```text
200 OK
```

### Vaciar el carrito

```http
DELETE /api/cart
```

La operación elimina todos los ítems, pero conserva el carrito principal asociado al usuario.

Código esperado:

```text
200 OK
```

### Códigos HTTP

| Código | Descripción |
|---|---|
| `200 OK` | Consulta, actualización, eliminación o vaciado exitoso. |
| `201 Created` | Producto agregado correctamente al carrito. |
| `400 Bad Request` | Cantidad, identificador o request inválido. |
| `401 Unauthorized` | Token ausente, inválido o expirado. |
| `404 Not Found` | Producto, carrito o ítem inexistente. |
| `409 Conflict` | La cantidad solicitada supera el stock disponible. |
| `500 Internal Server Error` | Error inesperado. |
| `503 Service Unavailable` | La base de datos no está disponible. |

### Autenticación

Todos los endpoints del carrito requieren JWT:

```csharp
[Authorize]
```

Para probarlos desde Swagger:

1. Ejecutar `POST /api/auth/login`.
2. Copiar el valor de `accessToken`.
3. Pulsar **Authorize**.
4. Pegar únicamente el token.
5. Ejecutar los endpoints del carrito.

### Pruebas

Ejecutar las pruebas relacionadas con el carrito:

```powershell
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj `
  --filter "FullyQualifiedName~CartTests|FullyQualifiedName~CartServiceTests|FullyQualifiedName~CartControllerTests"
```

Ejecutar todas las pruebas:

```powershell
dotnet test ShoppingCart.sln
```