# Base de donnees

Persistance via EF Core 10 + SQLite.

## Contexte

- `AppDbContext`: `AdvancedDevSample.Infrastructure/Persistence/AppDbContext.cs`
- provider: `UseSqlite(...)`
- fallback connection string: `Data Source=advanceddevsample.db`

## Entites persistantes

- `Products`
- `Categories`
- `PriceHistories`
- `Users`
- `RefreshTokens`
- `AuditLogs`

## Contraintes principales

- `Products.Sku` unique (si non null)
- `Users.Email` unique
- `RefreshTokens.Token` unique
- precision prix:
  - `Price` et `OldPrice`/`NewPrice`: `18,2`
  - `DiscountPercentage`: `5,2`

## Relations

- `Product -> Category` (`SetNull` au delete categorie)
- `PriceHistory -> Product` (`Cascade`)
- `RefreshToken -> User` (`Cascade`)

## ERD simplifie

```mermaid
erDiagram
  CATEGORY ||--o{ PRODUCT : categorizes
  PRODUCT ||--o{ PRICE_HISTORY : has
  USER ||--o{ REFRESH_TOKEN : owns
  USER ||--o{ AUDIT_LOG : generates

  CATEGORY {
    guid Id PK
    string Name
    string Description
    bool IsActive
  }

  PRODUCT {
    guid Id PK
    string Name
    string Sku UK
    decimal Price
    decimal DiscountPercentage
    int Stock
    guid CategoryId FK
    bool IsActive
  }

  PRICE_HISTORY {
    guid Id PK
    guid ProductId FK
    decimal OldPrice
    decimal NewPrice
    decimal DiscountPercentage
    datetime ChangedAt
    string Reason
  }

  USER {
    guid Id PK
    string Email UK
    string PasswordHash
    string FirstName
    string LastName
    int Role
    bool IsActive
  }

  REFRESH_TOKEN {
    guid Id PK
    guid UserId FK
    string Token UK
    datetime ExpiresAt
    bool IsRevoked
  }

  AUDIT_LOG {
    guid Id PK
    guid UserId FK
    string EventType
    string UserEmail
    bool IsSuccess
    datetime CreatedAt
  }
```

## Migrations

Dossier:

- `AdvancedDevSample.Infrastructure/Persistence/Migrations`

Au demarrage:

- si `UseMigrations=true` et migrations presentes -> `Migrate()`
- si `Development` sans migration -> fallback `EnsureCreated()`
- hors `Development` sans migration -> erreur

## Seeding (dev)

Ordre des seeders:

1. `AdminUserSeeder`
2. `CategorySeeder`
3. `ProductSeeder`
4. `PriceHistorySeeder`

### Admin seeding

- cree un admin uniquement si aucun admin n'existe deja
- requiert `ADMIN_EMAIL` et `ADMIN_PASSWORD`

### Donnees seed

- categories predefinies + categories aleatoires
- produits predefinis + produits aleatoires
- historiques de prix aleatoires

## Verifier la derive de modele

```bash
dotnet ef migrations has-pending-model-changes \
  --project AdvancedDevSample.Infrastructure/AdvancedDevSample.Infrastructure.csproj \
  --startup-project AdvancedDevSample.Api/AdvancedDevSample.Api.csproj \
  --context AppDbContext
```
