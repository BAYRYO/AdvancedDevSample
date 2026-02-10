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
- **Entity Framework Core 10** + **PostgreSQL**
- **Sentry** (observabilite)
- **xUnit** (tests)
- **MkDocs Material** (documentation)

## Ce que couvre cette documentation

- installation locale et prerequis
- configuration complete (env, appsettings, CORS, JWT)
- vision produit, roadmap, gouvernance
- architecture technique et flux de donnees
- contrat API detaille (auth, produits, categories, utilisateurs)
- frontend (auth state, refresh token, appels API)
- base de donnees, migrations et seeding
- securite (headers, rate limiting, roles)
- tests, qualite, CI/CD
- exploitation et troubleshooting
- contribution projet

## Lecture recommandee

1. [Demarrage](getting-started.md)
2. [Vision produit](product.md)
3. [Configuration](configuration.md)
4. [Architecture](architecture.md)
5. [API](api.md)

## Parcours par profil

- Nouveau developpeur: [Demarrage](getting-started.md) -> [Configuration](configuration.md) -> [Troubleshooting](troubleshooting.md)
- Developpeur backend: [Architecture](architecture.md) -> [API](api.md) -> [Base de donnees](database.md)
- Developpeur frontend: [Frontend](frontend.md) -> [API](api.md) -> [Securite](security.md)
- Mainteneur projet: [Tests et qualite](quality.md) -> [CI/CD](cicd.md) -> [Exploitation](operations.md)
