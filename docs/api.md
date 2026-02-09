# API

Base URL locale:

- HTTP: `http://localhost:5069`
- HTTPS: `https://localhost:7119`

## Conventions

- format JSON
- IDs en `Guid`
- erreurs middleware en JSON
- auth via `Authorization: Bearer <token>`

## Authentification et autorisation

- JWT valide issuer/audience/signature/expiration
- politiques:
  - `AdminOnly` => role `Admin`
  - `UserOrAdmin` => role `User` ou `Admin` (definie, peu utilisee pour l'instant)

## Endpoints d'authentification

### `POST /api/auth/register`

Cree un utilisateur, renvoie JWT + refresh token.

Request:

```json
{
  "email": "user@example.com",
  "password": "StrongPassword!123",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

Reponse `201`:

```json
{
  "token": "<jwt>",
  "expiresAt": "2026-02-09T13:00:00Z",
  "refreshToken": "<refresh-token>",
  "refreshTokenExpiresAt": "2026-02-16T13:00:00Z",
  "user": {
    "id": "...",
    "email": "user@example.com",
    "firstName": "Jane",
    "lastName": "Doe",
    "fullName": "Jane Doe",
    "role": "User",
    "isActive": true,
    "createdAt": "...",
    "lastLoginAt": null
  }
}
```

### `POST /api/auth/login`

- limite de debit: **5 requetes/minute par IP**
- echec: `401`
- depassement: `429`

Request:

```json
{
  "email": "admin@example.com",
  "password": "StrongPassword!123"
}
```

### `POST /api/auth/refresh`

Request:

```json
{
  "refreshToken": "<refresh-token>"
}
```

- invalide/expire/revoque => `401`

### `GET /api/auth/me`

- necessite JWT valide
- renvoie l'utilisateur courant

## Endpoints produits (`/api/products`)

| Methode | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/products` | JWT | Creer un produit |
| `GET` | `/api/products` | Public | Recherche paginee |
| `GET` | `/api/products/{id}` | Public | Obtenir un produit |
| `PUT` | `/api/products/{id}` | JWT | Mettre a jour |
| `DELETE` | `/api/products/{id}` | Admin | Supprimer |
| `PUT` | `/api/products/{id}/price` | JWT | Changer le prix |
| `POST` | `/api/products/{id}/discount` | JWT | Appliquer remise |
| `DELETE` | `/api/products/{id}/discount` | JWT | Supprimer remise |
| `GET` | `/api/products/{id}/price-history` | Public | Historique prix |
| `POST` | `/api/products/{id}/activate` | JWT | Activer |
| `POST` | `/api/products/{id}/deactivate` | JWT | Desactiver |

### Recherche produits

`GET /api/products?name=...&minPrice=...&maxPrice=...&categoryId=...&isActive=...&page=1&pageSize=20`

Contraintes:

- `page >= 1`
- `pageSize` entre `1` et `100`

## Endpoints categories (`/api/categories`)

| Methode | Route | Auth | Description |
| --- | --- | --- | --- |
| `POST` | `/api/categories` | JWT | Creer une categorie |
| `GET` | `/api/categories` | Public | Lister categories |
| `GET` | `/api/categories/{id}` | Public | Detail categorie |
| `PUT` | `/api/categories/{id}` | JWT | Mettre a jour |
| `DELETE` | `/api/categories/{id}` | Admin | Supprimer |

Filtre disponible:

- `GET /api/categories?activeOnly=true`

## Endpoints utilisateurs (`/api/users`) - Admin uniquement

| Methode | Route | Description |
| --- | --- | --- |
| `GET` | `/api/users?page=1&pageSize=20` | Liste paginee |
| `GET` | `/api/users/{id}` | Detail utilisateur |
| `PUT` | `/api/users/{id}/role` | Changer role (`User`/`Admin`) |
| `DELETE` | `/api/users/{id}` | Desactivation (soft delete) |

## DTOs principaux

- produit creation: `name`, `sku`, `price`, `stock`, `description`, `categoryId`
- produit update: tous champs optionnels + `clearCategory`
- categorie creation/update: `name`, `description`, `isActive`
- role utilisateur: `role` regex `User|Admin`

## Reponses d'erreur

Exemple metier (`400`, `404`, `409`):

```json
{
  "title": "Erreur metier",
  "detail": "Le nom du produit est obligatoire."
}
```

Exemple technique (`500`):

```json
{
  "error": "Erreur technique"
}
```

## Exploration interactive

En environnement `Development`:

- Swagger UI: `/swagger`
- Scalar: `/scalar/v1`
