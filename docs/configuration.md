# Configuration

## Sources de configuration

Ordre de priorite ASP.NET Core:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. variables d'environnement
4. arguments CLI

## Variables d'environnement

Template: `.env.example`

| Variable | Requise | Description |
| --- | --- | --- |
| `JWT_SECRET` | Oui | Secret JWT (>= 32 caracteres) |
| `POSTGRES_DB` | Non | Nom base PostgreSQL (compose) |
| `POSTGRES_USER` | Non | Utilisateur PostgreSQL (compose) |
| `POSTGRES_PASSWORD` | Non | Mot de passe PostgreSQL (compose) |
| `FRONTEND_API_BASE_URL` | Non | URL API injectee au build Docker frontend |
| `ADMIN_EMAIL` | Non | Email admin seed (`Development`) |
| `ADMIN_PASSWORD` | Non | Mot de passe admin seed (`Development`) |
| `SENTRY_DSN` | Non | DSN Sentry |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Non | Point d'entree OTLP (export traces/metriques) |

## Configuration API

Fichiers: `AdvancedDevSample.Api/appsettings.json` et `AdvancedDevSample.Api/appsettings.Development.json`

### Base de donnees

- `ConnectionStrings:DefaultConnection` pour PostgreSQL
- `UseMigrations` (defaut `true`)
- `UseInMemoryDatabase` (defaut `false`)
- `InMemoryDatabaseName` (defaut `AdvancedDevSample`)
- `SeedDatabase` (defaut `true`, actif en `Development` uniquement)

### JWT

- `JWT_SECRET` obligatoire, verifie au demarrage
- `Jwt:Issuer` (defaut `AdvancedDevSample`)
- `Jwt:Audience` (defaut `AdvancedDevSample`)
- `Jwt:ExpirationMinutes` (defaut `60`)

### CORS

`Cors:AllowedOrigins`.

Fallback (si section vide/non definie):

- `http://localhost:5173`
- `https://localhost:7173`

En Docker Compose, des origines supplementaires sont injectees via variables:

- `http://localhost:8080`

### Observabilite

- `SENTRY_DSN` ou `Sentry:Dsn`
- `OpenTelemetry:ServiceName` (defaut `AdvancedDevSample.Api`)
- `OpenTelemetry:Otlp:Endpoint` ou `OTEL_EXPORTER_OTLP_ENDPOINT`

## Launch settings

API: `AdvancedDevSample.Api/Properties/launchSettings.json`

- profiles `http` et `https`
- injecte un `JWT_SECRET` local de dev

Frontend: `AdvancedDevSample.Frontend/Properties/launchSettings.json`

- profiles `http`/`https`
- ports `5173` / `7173`

## Configuration frontend

Fichier: `AdvancedDevSample.Frontend/wwwroot/appsettings.json`

- `ApiBaseUrl` (defaut `http://localhost:5069`)

## Exemple `.env`

```env
JWT_SECRET=replace-with-a-secure-secret-min-32-chars
POSTGRES_DB=advanceddevsample
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
FRONTEND_API_BASE_URL=http://localhost:5069
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=change-me-now
SENTRY_DSN=
OTEL_EXPORTER_OTLP_ENDPOINT=
```
