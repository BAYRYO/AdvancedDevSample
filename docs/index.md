# AdvancedDevSample

Bienvenue dans la documentation technique de `AdvancedDevSample`.

## Objectif du projet

`AdvancedDevSample` est une base de reference pour construire une application .NET modulaire avec:

- separation stricte des responsabilites (Clean Architecture)
- domaine metier explicite (DDD)
- API REST securisee (JWT)
- frontend Blazor WebAssembly
- qualite logicielle automatisee (tests, couverture, format, checks CI)

## Stack technique

- **.NET 10**
- **ASP.NET Core** (API)
- **Blazor WebAssembly** (Frontend)
- **Entity Framework Core 10** + **SQLite**
- **Sentry** (observabilite)
- **xUnit** (tests)
- **MkDocs Material** (documentation)

## Ce que couvre cette documentation

- installation locale et prerequis
- configuration complete (env, appsettings, CORS, JWT)
- architecture technique et flux de donnees
- contrat API detaille (auth, produits, categories, utilisateurs)
- frontend (auth state, refresh token, appels API)
- base de donnees, migrations et seeding
- securite (headers, rate limiting, roles)
- tests, qualite, CI/CD
- exploitation et troubleshooting
- contribution projet

## Lecture recommandee

1. `getting-started.md`
2. `configuration.md`
3. `architecture.md`
4. `api.md`
5. `quality.md`
