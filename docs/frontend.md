# Frontend

Le frontend est une application Blazor WebAssembly (`AdvancedDevSample.Frontend`).

## Configuration

Fichier: `AdvancedDevSample.Frontend/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "http://localhost:5069"
}
```

## Services principaux

- `AuthService`: login/register/logout/refresh
- `AuthTokenHandler`: ajout automatique du bearer token + refresh proactif
- `TokenStore`: stockage session navigateur
- `FrontendAuthStateProvider`: projection des claims utilisateur
- `ApiClient`: client HTTP typed avec gestion d'erreur uniforme

## Stockage session

Le stockage est fait en `sessionStorage` via `BrowserStorageService`.

Cle utilisee:

- `advanceddevsample.auth`

## Flux auth

1. login/register appelle `/api/auth/*`
2. JWT + refresh token stockes dans le `TokenStore`
3. `AuthenticationStateProvider` notifie l'UI
4. avant expiration courte (`< 30s`), `AuthTokenHandler` tente un refresh
5. sur `401` API, logout automatique

## Pages

- `/products` (auth)
- `/categories` (auth)
- `/users` (auth + role `Admin`)
- `/account/login`
- `/account/register`
- `/account/profile`

## Notes d'integration

- deux clients HTTP sont enregistres: `ApiNoAuth` et `ApiClient`
- `ApiNoAuth` sert pour login/register/refresh
- `ApiClient` passe par `AuthTokenHandler`

## Tests frontend

Les tests sont dans `AdvancedDevSample.Test/Frontend` (services + composants).
