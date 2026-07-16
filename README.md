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


## Órdenes y checkout transaccional

El módulo de órdenes permite confirmar la compra del carrito activo del usuario autenticado, disminuir el stock de los productos, vaciar el carrito y conservar un historial de compras.

Todos los endpoints de órdenes requieren autenticación mediante JWT.

### Modelo de datos

La persistencia de órdenes utiliza las entidades `Order` y `OrderItem`.

#### `Order`

Representa la cabecera de una compra y almacena:

- Identificador de la orden.
- Identificador del usuario propietario.
- Fecha de creación en UTC.
- Subtotal general.
- Descuento aplicado.
- Total final.
- Colección de productos comprados.

#### `OrderItem`

Representa un producto incluido en una orden y almacena:

- Identificador del detalle.
- Identificador de la orden.
- Identificador del producto.
- Código del producto.
- Nombre del producto.
- Precio unitario al momento de la compra.
- Cantidad comprada.
- Subtotal del producto.

La información comercial del producto se guarda como un **snapshot**. Esto permite conservar los datos históricos de la compra aunque posteriormente cambien el código, nombre o precio del producto.

### Relaciones

```text
User 1 ─── N Order
Order 1 ─── N OrderItem
Product 1 ─── N OrderItem
```

La relación entre `User` y `Order` utiliza eliminación restringida para evitar que la eliminación de un usuario borre accidentalmente su historial de compras.

La relación entre `Product` y `OrderItem` también utiliza eliminación restringida para proteger la trazabilidad histórica.

Los detalles de una orden se eliminan en cascada únicamente cuando se elimina su orden principal.

### Reglas de negocio

El checkout aplica las siguientes reglas:

- El usuario debe estar autenticado.
- La compra utiliza el carrito persistente del usuario autenticado.
- El cliente no envía nuevamente los productos ni las cantidades al confirmar la compra.
- El carrito debe contener al menos un producto.
- Todos los productos del carrito deben existir.
- El stock se consulta y valida nuevamente durante el checkout.
- No se permite comprar una cantidad superior al stock disponible.
- Agregar un producto al carrito no reserva ni disminuye el stock.
- El stock disminuye únicamente cuando la compra se confirma correctamente.
- La orden conserva el código, nombre y precio vigentes al momento del checkout.
- Se aplica un descuento del 10 % cuando el subtotal es estrictamente mayor a `$100`.
- Un subtotal exactamente igual a `$100` no recibe descuento.
- Después de una compra exitosa, el carrito queda vacío.
- Cada usuario puede consultar únicamente sus propias órdenes.
- Una orden inexistente y una orden perteneciente a otro usuario producen la misma respuesta `404 Not Found`.

### Flujo del checkout

El checkout se ejecuta en el siguiente orden:

```text
1. Iniciar la estrategia de ejecución de MySQL.
2. Iniciar una transacción con aislamiento Serializable.
3. Obtener el carrito del usuario autenticado.
4. Rechazar el checkout si el carrito está vacío.
5. Obtener los productos involucrados con seguimiento de EF Core.
6. Verificar que todos los productos existan.
7. Validar el stock disponible de todos los productos.
8. Construir el snapshot de los productos.
9. Crear la orden y sus detalles.
10. Disminuir el stock.
11. Vaciar el carrito.
12. Ejecutar SaveChangesAsync.
13. Confirmar la transacción.
```

Todas las validaciones de existencia y stock se realizan antes de modificar el estado de los productos, la orden o el carrito. Esto evita modificaciones parciales cuando uno de los productos no puede ser comprado.

### Transacción del checkout

La operación completa se ejecuta mediante la abstracción:

```text
IUnitOfWork
```

La capa `Application` define el contrato y la capa `Infrastructure` lo implementa mediante:

```text
EfUnitOfWork
```

La transacción incluye como una sola unidad de trabajo:

```text
Creación de Order
+ creación de OrderItems
+ disminución de Product.Stock
+ eliminación de CartItems
```

Los repositorios y `EfUnitOfWork` utilizan la misma instancia `Scoped` de `ShoppingCartDbContext`. Por ello, todos los cambios rastreados se confirman mediante una sola llamada a:

```csharp
await dbContext.SaveChangesAsync(cancellationToken);
```

Cuando la operación finaliza correctamente, se confirma mediante:

```csharp
await transaction.CommitAsync(cancellationToken);
```

Si ocurre una excepción durante la operación, se ejecuta:

```csharp
await transaction.RollbackAsync(CancellationToken.None);
```

El rollback se intenta con `CancellationToken.None` para que una cancelación de la petición no impida revertir la transacción.

Después del rollback se limpia el `ChangeTracker` para evitar conservar entidades modificadas dentro del mismo `DbContext` si la estrategia de ejecución vuelve a intentar la operación.

### Nivel de aislamiento

El checkout utiliza:

```csharp
IsolationLevel.Serializable
```

Este nivel de aislamiento protege las operaciones de stock frente a checkouts concurrentes y reduce el riesgo de que dos compras se confirmen utilizando simultáneamente las mismas unidades disponibles.

El stock se valida dentro de la transacción, utilizando el valor actual almacenado en la base de datos y no el valor que tenía el producto cuando fue agregado al carrito.

### Estrategia de reintentos de MySQL

La conexión utiliza una estrategia de reintentos para errores transitorios de MySQL.

Debido a que una transacción iniciada manualmente no puede ejecutarse directamente cuando está activa `MySqlRetryingExecutionStrategy`, `EfUnitOfWork` obtiene la estrategia mediante:

```csharp
var executionStrategy =
    dbContext.Database.CreateExecutionStrategy();
```

Toda la unidad transaccional se ejecuta dentro de:

```csharp
await executionStrategy.ExecuteAsync(...);
```

La estructura es:

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

Esta integración permite combinar correctamente los reintentos de conexión con una transacción explícita.

### Endpoints

#### Confirmar una compra

```http
POST /api/orders
```

No requiere body. El servidor utiliza el carrito asociado al usuario autenticado.

Respuesta exitosa:

```text
201 Created
```

Ejemplo:

```json
{
  "id": "50be22fb-421d-4655-a0e5-cd426e90dd1f",
  "createdAtUtc": "2026-07-15T23:30:00Z",
  "items": [
    {
      "productId": "557da7fc-1478-47ce-83f7-a89a264e1248",
      "productCode": "PROD-001",
      "productName": "Mechanical Keyboard",
      "unitPrice": 50,
      "quantity": 2,
      "subtotal": 100
    },
    {
      "productId": "ea640067-535f-434b-b557-b30a69611329",
      "productCode": "PROD-002",
      "productName": "Wireless Mouse",
      "unitPrice": 20,
      "quantity": 1,
      "subtotal": 20
    }
  ],
  "subtotal": 120,
  "discount": 12,
  "total": 108
}
```

Posibles respuestas:

| Código | Descripción |
|---|---|
| `201 Created` | Orden creada correctamente |
| `400 Bad Request` | Identificador o solicitud inválida |
| `401 Unauthorized` | Token ausente, inválido o expirado |
| `404 Not Found` | Alguno de los productos ya no existe |
| `409 Conflict` | Carrito vacío, stock insuficiente u otro conflicto de negocio |
| `500 Internal Server Error` | Error inesperado |
| `503 Service Unavailable` | Base de datos temporalmente no disponible |

#### Obtener historial de compras

```http
GET /api/orders
```

Devuelve únicamente las órdenes pertenecientes al usuario autenticado, ordenadas desde la más reciente.

Respuesta exitosa:

```text
200 OK
```

Cuando el usuario todavía no tiene órdenes, devuelve:

```json
[]
```

#### Obtener una orden por identificador

```http
GET /api/orders/{id}
```

Devuelve el detalle de una orden únicamente cuando pertenece al usuario autenticado.

Posibles respuestas:

| Código | Descripción |
|---|---|
| `200 OK` | Orden encontrada |
| `400 Bad Request` | Identificador inválido |
| `401 Unauthorized` | Token ausente, inválido o expirado |
| `404 Not Found` | La orden no existe o pertenece a otro usuario |

El servidor devuelve `404 Not Found` tanto para una orden inexistente como para una orden perteneciente a otro usuario. Esto evita revelar la existencia de compras ajenas.

### Validación manual mediante Swagger

Para validar el checkout:

1. Ejecutar `POST /api/auth/login`.
2. Copiar el valor de `accessToken`.
3. Autorizar Swagger mediante el esquema Bearer.
4. Consultar los productos con `GET /api/products`.
5. Agregar un producto mediante `POST /api/cart/items`.
6. Confirmar el contenido mediante `GET /api/cart`.
7. Ejecutar `POST /api/orders`.
8. Verificar la respuesta `201 Created`.
9. Consultar nuevamente `GET /api/cart` y confirmar que esté vacío.
10. Consultar `GET /api/products/{id}` y confirmar que el stock disminuyó.
11. Consultar `GET /api/orders`.
12. Consultar `GET /api/orders/{id}`.

Para validar el rechazo de un carrito vacío, ejecutar nuevamente:

```http
POST /api/orders
```

El resultado esperado es:

```text
409 Conflict
```

Para validar el aislamiento entre usuarios:

1. Crear una orden con el usuario Customer.
2. Copiar el identificador de la orden.
3. Iniciar sesión con otro usuario.
4. Reemplazar el JWT en Swagger.
5. Consultar la orden creada por el primer usuario.

El resultado esperado es:

```text
404 Not Found
```

### Pruebas automatizadas

Las pruebas de dominio verifican:

- Creación de órdenes.
- Generación de detalles.
- Snapshot del producto.
- Cálculo del subtotal por producto.
- Cálculo del subtotal general.
- Aplicación del descuento.
- Cálculo del total.
- Rechazo de órdenes sin productos.

Las pruebas de `OrderService` verifican:

- Checkout exitoso.
- Creación de la orden y sus detalles.
- Disminución del stock.
- Vaciado del carrito.
- Rechazo de un carrito vacío.
- Rechazo por stock insuficiente.
- Ausencia de modificaciones parciales antes de completar las validaciones.
- Ejecución de la transacción.
- Confirmación de operaciones exitosas.
- Solicitud de rollback ante excepciones.
- Historial filtrado por usuario.
- Consulta de una orden propia.
- Rechazo de órdenes inexistentes o pertenecientes a otro usuario.
- Ausencia de llamadas directas a `CartRepository.SaveChangesAsync` durante el checkout.

Las pruebas de `OrderController` verifican:

- Obtención del usuario desde los claims del JWT.
- Respuesta `201 Created` durante el checkout.
- Generación de la ruta del recurso creado.
- Respuesta `200 OK` para el historial.
- Respuesta `200 OK` para el detalle.

Para ejecutar únicamente las pruebas relacionadas con órdenes:

```powershell
dotnet test tests/ShoppingCart.UnitTests/ShoppingCart.UnitTests.csproj `
  --filter "FullyQualifiedName~Order"
```

Para validar toda la solución:

```powershell
dotnet build ShoppingCart.sln
dotnet test ShoppingCart.sln
```

### Estado de la validación transaccional

Las pruebas unitarias verifican la orquestación de la transacción mediante una implementación fake de `IUnitOfWork`.

El flujo real del checkout también fue validado manualmente contra MySQL mediante Docker y Swagger, comprobando que:

- Se crea la orden.
- Se crean los detalles.
- Disminuye el stock.
- Se vacía el carrito.
- El historial devuelve la nueva orden.
- El carrito vacío produce `409 Conflict`.

Permanece como mejora adicional una prueba de integración automatizada que provoque deliberadamente un error antes del commit y compruebe directamente en MySQL que no se persistieron cambios parciales.