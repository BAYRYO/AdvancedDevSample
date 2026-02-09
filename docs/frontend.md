# Frontend

Le frontend est une application Blazor WebAssembly (`AdvancedDevSample.Frontend`).

## Points clefs

- auth basee sur JWT + refresh token
- stockage session navigateur via `TokenStore`
- client API type (`ApiClient`) avec gestion d'erreur centralisee
- `AuthenticationStateProvider` personnalise (`FrontendAuthStateProvider`)

## Configuration

Fichier: `AdvancedDevSample.Frontend/wwwroot/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7119"
}
```

## Services principaux

- `AuthService`
  - login, register, logout
  - refresh token avec verrou (`SemaphoreSlim`) pour eviter refresh concurrents

- `ApiClient`
  - appels API produits/categories/utilisateurs
  - deserialisation JSON et transformation erreurs en `ApiException`

- `AuthTokenHandler`
  - ajoute automatiquement `Authorization: Bearer ...`

- `FrontendAuthStateProvider`
  - construit les claims (`NameIdentifier`, `Name`, `Email`, `Role`)

## Flux d'authentification

1. utilisateur se connecte (`/api/auth/login`)
2. session stockee localement
3. `AuthenticationState` notifie l'UI
4. a l'expiration proche, tentative de refresh
5. si refresh invalide -> session nettoyee + utilisateur deconnecte

## Pages principales

- `Pages/Home.razor`
- `Pages/Products.razor`
- `Pages/Categories.razor`
- `Pages/Users.razor`
- `Pages/Account/Login.razor`
- `Pages/Account/Register.razor`
- `Pages/Account/Profile.razor`

## Tests frontend

Des tests existent dans `AdvancedDevSample.Test/Frontend` (services et composants).
