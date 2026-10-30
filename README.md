# SGCAE – Employee Access Management and Control System
### Sistema de Gestión y Control de Acceso de Empleados

**Author / Autor:** Juan Pablo Robledo Urrego
**Course / Materia:** Distributed Programming / Programación Distribuida – Seguimiento 2

> **Language note / Nota sobre el idioma**
> **EN:** Following the professor's feedback on the previous deliverable, all the source code (classes, methods, variables, endpoints, database tables) is written in **English**. Code comments are written in **English and Spanish**.
> **ES:** Atendiendo la observación del profesor en el entregable anterior, todo el código fuente (clases, métodos, variables, endpoints, tablas) está escrito en **inglés**. Los comentarios del código están en **inglés y español**.

---

## 1. Description / Descripción

**EN:** Backend with a REST API to manage employees, credentials, access points and the entry/exit records of an organization. Built with **ASP.NET Core 8 Web API**, **Entity Framework Core 8** and **SQL Server**, applying **Clean Architecture**, **DDD** and **CQRS**.

**ES:** Backend con API REST para gestionar empleados, credenciales, puntos de acceso y los registros de entrada/salida de una organización. Construido con **ASP.NET Core 8 Web API**, **Entity Framework Core 8** y **SQL Server**, aplicando **Arquitectura Limpia**, **DDD** y **CQRS**.

## 2. Architecture / Arquitectura

```
            ┌──────────────────────────────┐
            │        Sgcae.Api             │  Controllers, middleware, Swagger (composition root)
            └──────────────┬───────────────┘
                           │
      ┌────────────────────┴─────────────────────┐
      ▼                                          ▼
┌───────────────────────┐            ┌────────────────────────────┐
│  Sgcae.Application    │◄───────────│   Sgcae.Infrastructure     │  EF Core, SQL Server,
│  Commands / Queries   │ implements │   repositories, queries    │  migrations, seed
└──────────┬────────────┘            └────────────────────────────┘
           ▼
┌───────────────────────┐
│     Sgcae.Domain      │  Entities, value objects, rules, domain service (no dependencies)
└───────────────────────┘
```

**EN:** Dependencies always point inwards. The Domain does not depend on anything; Infrastructure implements the interfaces defined by the Domain (repositories) and by the Application (query services).
**ES:** Las dependencias siempre apuntan hacia adentro. El Dominio no depende de nada; la Infraestructura implementa las interfaces definidas por el Dominio (repositorios) y por la Aplicación (servicios de consulta).

| Project / Proyecto | Responsibility / Responsabilidad |
|---|---|
| `Sgcae.Domain` | Aggregates `Employee`, `Credential`, `AccessPoint`, `AccessLog`; value objects `DocumentNumber`, `Email`, `CredentialCode`, `AccessPointCode`; domain service `AccessPolicy`; repository contracts. |
| `Sgcae.Application` | CQRS use cases: commands (write) and queries (read), own dispatcher, DTOs, pagination. |
| `Sgcae.Infrastructure` | `SgcaeDbContext` (EF Core + SQL Server), Fluent API mappings, repositories (write), query services with `AsNoTracking` (read), database initializer. |
| `Sgcae.Api` | REST controllers, exception middleware (ProblemDetails), Swagger, `.http` file with test requests. |
| `tests/Sgcae.Domain.Tests` | xUnit unit tests of the business rules. |

### DDD
- **Aggregates / Agregados:** `Employee`, `Credential`, `AccessPoint`, `AccessLog` (they reference each other only by Id / se referencian solo por Id).
- **Value objects / Objetos de valor:** immutable records validated on creation / records inmutables validados al crearse.
- **Domain service / Servicio de dominio:** `AccessPolicy` decides if an access is granted (it involves three aggregates) / decide si un acceso se autoriza (involucra tres agregados).
- **Invariants / Invariantes:** private setters + methods that protect state transitions / setters privados + métodos que protegen las transiciones de estado.

### CQRS
- **Commands / Comandos** (`ICommand<T>`) use domain repositories + Unit of Work → modify state.
- **Queries / Consultas** (`IQuery<T>`) use read-only query services with `AsNoTracking`, filters and pagination in SQL.
- **EN:** I implemented a small dispatcher (`IDispatcher`) instead of MediatR to keep the CQRS separation explicit and dependency-free.
- **ES:** Implementé un despachador propio (`IDispatcher`) en vez de MediatR para que la separación CQRS sea explícita y sin dependencias externas.

## 3. Business rules / Reglas de negocio

| # | Rule (EN) | Regla (ES) |
|---|---|---|
| 1 | Document number and email are unique. | El documento y el correo son únicos. |
| 2 | Deactivating an employee revokes all their credentials. | Desactivar un empleado revoca todas sus credenciales. |
| 3 | An inactive employee can not receive credentials. | Un empleado inactivo no puede recibir credenciales. |
| 4 | An employee holds only one non-revoked credential at a time. | Un empleado solo tiene una credencial no revocada a la vez. |
| 5 | Credential lifecycle: Active ⇄ Blocked → Revoked (final). | Ciclo de la credencial: Activa ⇄ Bloqueada → Revocada (final). |
| 6 | Access is denied if the access point does not exist or is disabled. | Se rechaza si el punto de acceso no existe o está deshabilitado. |
| 7 | Access is denied if the credential does not exist, is blocked, revoked or expired. | Se rechaza si la credencial no existe, está bloqueada, revocada o vencida. |
| 8 | Access is denied if the employee is inactive. | Se rechaza si el empleado está inactivo. |
| 9 | Anti-passback per access point: no double entry, no exit without entry. | Anti-passback por punto: no doble entrada, no salida sin entrada. |
| 10 | **Every attempt is recorded, including denied ones (with the reason).** | **Todo intento se registra, incluidos los rechazados (con el motivo).** |

## 4. Endpoints

| Method | Route | RF | Description / Descripción |
|---|---|---|---|
| POST | `/api/employees` | RF-01 | Register employee / Registrar empleado |
| GET | `/api/employees?status=&area=&search=&page=&pageSize=` | RF-01 | List (paginated) / Listar (paginado) |
| GET | `/api/employees/{id}` | RF-01 | Get by id / Consultar por id |
| PUT | `/api/employees/{id}` | RF-01 | Update / Actualizar |
| PATCH | `/api/employees/{id}/deactivate` | RF-01 | Deactivate / Desactivar |
| GET | `/api/employees/{id}/credentials` | RF-02 | Employee credentials / Credenciales del empleado |
| POST | `/api/credentials` | RF-02 | Assign credential / Asignar credencial |
| GET | `/api/credentials/{id}` · `/api/credentials/code/{code}` | RF-02 | Status / Estado |
| PATCH | `/api/credentials/{id}/block` · `/unblock` · `/revoke` | RF-02 | Block, unblock, revoke / Bloquear, desbloquear, revocar |
| POST | `/api/access-requests` | RF-03 | Validate and record entry/exit / Validar y registrar entrada/salida |
| GET | `/api/access-logs?employeeId=&fromUtc=&toUtc=&accessPointId=&result=&movement=&page=&pageSize=` | RF-04 | History (paginated) / Historial (paginado) |
| POST | `/api/access-points` | RF-06 | Register access point / Registrar punto de acceso |
| GET | `/api/access-points?isEnabled=&type=` · `/api/access-points/{id}` | RF-06 | Query / Consultar |
| PATCH | `/api/access-points/{id}/enable` · `/disable` | RF-06 | Enable, disable / Habilitar, deshabilitar |

**HTTP codes:** `200/201` OK · `400` invalid data / datos inválidos · `404` not found / no existe · `409` conflict or business rule / conflicto o regla de negocio. Errors use the **ProblemDetails** format.

## 5. How to run / Cómo ejecutar

### Requirements / Requisitos
- .NET 8 SDK
- SQL Server (LocalDB, Express, Developer) **or / o** Docker (`docker compose up -d`)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Connection string / Cadena de conexión
File / Archivo: `src/Sgcae.Api/appsettings.json` → `ConnectionStrings:SgcaeDb`

| Option | Connection string |
|---|---|
| LocalDB (default, Visual Studio) | `Server=(localdb)\\MSSQLLocalDB;Database=SgcaeDb;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server Express | `Server=localhost\\SQLEXPRESS;Database=SgcaeDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Docker (`docker-compose.yml`) | `Server=localhost,1433;Database=SgcaeDb;User Id=sa;Password=Sgcae_Passw0rd!;TrustServerCertificate=True` |

### Steps / Pasos
```bash
# 1. Restore and build / Restaurar y compilar
dotnet build

# 2. Create the EF Core migration (first time) / Crear la migración de EF Core (primera vez)
dotnet ef migrations add InitialCreate -p src/Sgcae.Infrastructure -s src/Sgcae.Api -o Persistence/Migrations

# 3. Create the database in SQL Server / Crear la base de datos en SQL Server
dotnet ef database update -p src/Sgcae.Infrastructure -s src/Sgcae.Api

# 4. Run the API / Ejecutar la API
dotnet run --project src/Sgcae.Api
```
Open / Abrir: **http://localhost:5080/swagger**

**EN:** On startup the API applies pending migrations and creates 4 sample access points (`RECEPTION-01`, `PARKING-01`, `WAREHOUSE-01`, `RESTRICTED-01`).
**ES:** Al iniciar, la API aplica las migraciones pendientes y crea 4 puntos de acceso de ejemplo (`RECEPTION-01`, `PARKING-01`, `WAREHOUSE-01`, `RESTRICTED-01`).

### Tests / Pruebas
```bash
dotnet test
```

## 6. Verifying the use cases / Verificación de los casos de uso

**EN:** The file `src/Sgcae.Api/Sgcae.Api.http` contains the full flow (20 requests) ready to run from Visual Studio or VS Code (REST Client). The same flow can be executed from Swagger.
**ES:** El archivo `src/Sgcae.Api/Sgcae.Api.http` contiene el flujo completo (20 peticiones) listo para ejecutar desde Visual Studio o VS Code (REST Client). El mismo flujo se puede ejecutar desde Swagger.

Expected flow / Flujo esperado:
1. Register employee → `201` · same document again → `409`
2. Assign credential `CRD-JPRU0001` → `201`
3. Entry at `RECEPTION-01` → `Granted` · second entry → `Denied (AlreadyInside)` · exit → `Granted`
4. Unknown credential → `Denied (CredentialNotFound)`
5. Block credential → access `Denied (CredentialBlocked)`
6. Query `/api/access-logs` with filters and pagination
7. Deactivate employee → credentials revoked → access `Denied (CredentialRevoked)`
