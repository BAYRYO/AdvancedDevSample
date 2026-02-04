# AdvancedDevSample

API REST de gestion de produits et catégories, développée avec une architecture Clean Architecture / DDD.

## Architecture

```
AdvancedDevSample/
├── AdvancedDevSample.Api/           # Couche présentation (Controllers, Middlewares)
├── AdvancedDevSample.Application/   # Couche application (Services, DTOs)
├── AdvancedDevSample.Domain/        # Couche domaine (Entities, Value Objects, Interfaces)
├── AdvancedDevSample.Infrastructure/# Couche infrastructure (EF Core, Repositories)
└── AdvancedDevSample.Test/          # Tests (Unit, Integration)
```

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQLite (inclus, aucune installation requise)

## Configuration

1. Copier le fichier d'environnement :
```bash
cp .env.example .env
```

2. Configurer les variables d'environnement dans `.env` :
```env
SENTRY_DSN=https://your-dsn@sentry.io/project-id
```

## Lancement

### Développement

```bash
# Restaurer les dépendances
dotnet restore

# Lancer l'API
dotnet run --project AdvancedDevSample.Api
```

L'API sera disponible sur :
- HTTP: http://localhost:5069
- HTTPS: https://localhost:7119

### Documentation API

En mode développement, la documentation est accessible via :
- **Swagger UI**: http://localhost:5069/swagger
- **Scalar**: http://localhost:5069/scalar/v1

## Tests

```bash
# Exécuter tous les tests
dotnet test

# Avec couverture de code
dotnet test --collect:"XPlat Code Coverage"
```

## Endpoints API

### Produits (`/api/products`)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/products` | Créer un produit |
| `GET` | `/api/products` | Rechercher des produits (avec filtres) |
| `GET` | `/api/products/{id}` | Obtenir un produit par ID |
| `PUT` | `/api/products/{id}` | Mettre à jour un produit |
| `DELETE` | `/api/products/{id}` | Supprimer un produit |
| `PUT` | `/api/products/{id}/price` | Modifier le prix |
| `POST` | `/api/products/{id}/discount` | Appliquer une réduction |
| `DELETE` | `/api/products/{id}/discount` | Supprimer la réduction |
| `GET` | `/api/products/{id}/price-history` | Historique des prix |
| `POST` | `/api/products/{id}/activate` | Activer le produit |
| `POST` | `/api/products/{id}/deactivate` | Désactiver le produit |

### Catégories (`/api/categories`)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/categories` | Créer une catégorie |
| `GET` | `/api/categories` | Lister les catégories |
| `GET` | `/api/categories/{id}` | Obtenir une catégorie par ID |
| `PUT` | `/api/categories/{id}` | Mettre à jour une catégorie |
| `DELETE` | `/api/categories/{id}` | Supprimer une catégorie |

## Base de données

L'application utilise SQLite. La base est créée automatiquement au premier lancement et seedée avec des données de test en mode développement.

Pour désactiver le seeding :
```json
// appsettings.Development.json
{
  "SeedDatabase": false
}
```

## Monitoring

L'application intègre [Sentry](https://sentry.io) pour :
- Capture des erreurs et exceptions
- Tracing des performances
- Breadcrumbs pour le debugging

## Stack technique

- **.NET 10** - Framework
- **Entity Framework Core 10** - ORM
- **SQLite** - Base de données
- **Sentry** - Monitoring
- **xUnit** - Tests
- **Bogus** - Génération de données de test
