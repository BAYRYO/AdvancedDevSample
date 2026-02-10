# Configuration

## Sources de configuration

Ordre standard ASP.NET Core (du plus faible au plus fort):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. variables d'environnement
4. arguments CLI

Le projet lit explicitement certaines variables d'environnement critiques (`JWT_SECRET`, `SENTRY_DSN`, etc.).

## Variables d'environnement

Template: `.env.example`

| Variable | Requise | Description |
| --- | --- | --- |
| `JWT_SECRET` | Oui | Cle de signature JWT (minimum 32 caracteres) |
| `POSTGRES_DB` | Non | Nom de la base PostgreSQL (Docker Compose) |
| `POSTGRES_USER` | Non | Utilisateur PostgreSQL (Docker Compose) |
| `POSTGRES_PASSWORD` | Non | Mot de passe PostgreSQL (Docker Compose) |
| `FRONTEND_API_BASE_URL` | Non | URL API injectee au build du frontend Docker |
| `SENTRY_DSN` | Non | DSN Sentry (fallback sur `Sentry:Dsn`) |
| `ADMIN_EMAIL` | Non | Email admin seed en dev |
| `ADMIN_PASSWORD` | Non | Mot de passe admin seed en dev |

## Configuration API (`AdvancedDevSample.Api/appsettings*.json`)

### `UseMigrations`

- `true` par defaut
- applique `Database.Migrate()` si migrations disponibles
- en `Development`, fallback `EnsureCreated()` si aucune migration
- hors `Development`, absence de migration => erreur de demarrage

### `SeedDatabase`

- lu au demarrage (`true` par defaut)
- seeding effectif uniquement en `Development`

### `ConnectionStrings:DefaultConnection`

- definie dans `appsettings.json` et `appsettings.Development.json`
- format PostgreSQL (`Host=...;Port=...;Database=...;Username=...;Password=...`)

### JWT (`Jwt:*`)

- `Jwt:Issuer` (defaut `AdvancedDevSample`)
- `Jwt:Audience` (defaut `AdvancedDevSample`)
- `Jwt:ExpirationMinutes` (defaut `60`)

### CORS (`Cors:AllowedOrigins`)

Si vide/non renseigne:

- `http://localhost:5173`
- `https://localhost:7173`

### Sentry (`Sentry:*`)

Le DSN peut etre defini via:

- `SENTRY_DSN` (prioritaire)
- `Sentry:Dsn`

## Launch settings locaux

Fichier API: `AdvancedDevSample.Api/Properties/launchSettings.json`

- profils `http`/`https`
- injecte un `JWT_SECRET` dev pour `dotnet run` local

Fichier Frontend: `AdvancedDevSample.Frontend/Properties/launchSettings.json`

- profils `http`/`https`
- ports locaux 5173/7173

## Configuration frontend

Fichier: `AdvancedDevSample.Frontend/wwwroot/appsettings.json`

- `ApiBaseUrl`: URL racine API (`http://localhost:5069` par defaut)

## Exemple `.env` local

```env
SENTRY_DSN=
JWT_SECRET=change-this-to-a-secure-random-secret-with-32-plus-chars
POSTGRES_DB=advanceddevsample
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
FRONTEND_API_BASE_URL=http://localhost:5069
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=StrongPassword!123
```
