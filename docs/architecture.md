# Architecture

Le projet suit une approche Clean Architecture avec separation des responsabilites.

## Couches

- `AdvancedDevSample.Api`: presentation (controllers, middlewares)
- `AdvancedDevSample.Application`: orchestration metier, DTOs, services
- `AdvancedDevSample.Domain`: coeur metier (entites, value objects, contrats)
- `AdvancedDevSample.Infrastructure`: persistance EF Core, implementations techniques
- `AdvancedDevSample.Frontend`: interface utilisateur Blazor WebAssembly
- `AdvancedDevSample.Test`: tests unitaires et integration

## Principes appliques

- Le domaine ne depend d'aucune couche externe
- L'application depend du domaine, pas de l'infrastructure concrete
- L'infrastructure implemente les interfaces du domaine/application
- L'API compose les dependances et expose les endpoints HTTP

## Flux type (API)

1. Un controller recoit une requete HTTP.
2. Le service d'application valide et applique le cas d'usage.
3. Les interfaces de repository sont resolues vers EF Core en infrastructure.
4. La reponse est retournee au client via DTOs.
