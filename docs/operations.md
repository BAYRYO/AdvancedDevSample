# Exploitation

## Observabilite

### Logs

- niveau global configure dans `appsettings*.json`
- middleware d'exceptions journalise selon severite:
  - warning: erreurs metier/app
  - error: infrastructure
  - critical: inattendu

### Sentry

Fonctionnalites activees:

- capture exceptions
- tracing performance
- breadcrumbs HTTP
- flush a l'arret applicatif

Conseil production:

- definir `SENTRY_DSN`
- verifier sampling (`TracesSampleRate`, `ProfilesSampleRate`)

## Demarrage applicatif

Sequence API:

1. configuration services + middlewares
2. initialisation BDD (migrations/ensurecreated)
3. seeding dev conditionnel
4. exposition controllers

## Health checks

Endpoints exposes:

- `GET /health/live`: verifie la disponibilite process (liveness)
- `GET /health/ready`: verifie la connectivite base de donnees (readiness)

Format de reponse:

```json
{
  "status": "Healthy",
  "totalDurationMs": 2.31,
  "checks": {
    "database": {
      "status": "Healthy",
      "durationMs": 1.52,
      "description": "Database connection is available."
    }
  }
}
```

## Runbook rapide

### Symptomes: API ne demarre pas

Verifier:

1. presence `JWT_SECRET` >= 32 caracteres
2. accessibilite PostgreSQL (reseau, credentials, base)
3. coherence migrations si `UseMigrations=true`

### Symptomes: frontend appelle la mauvaise API

Verifier `AdvancedDevSample.Frontend/wwwroot/appsettings.json`:

- `ApiBaseUrl`

### Symptomes: trop de `401`

Verifier:

1. token JWT non expire
2. issuer/audience alignes
3. horloge machine correcte
4. refresh token encore valide

## Runbooks production

## SLI / SLO recommandes

SLI minimaux:

1. disponibilite API (succes `2xx/3xx` sur endpoints critiques)
2. latence p95 sur endpoints critiques
3. taux d'erreur serveur (`5xx`)
4. taux de refresh token en echec
5. statut readiness (`/health/ready`)

SLO de depart (a ajuster selon contexte):

1. disponibilite mensuelle API >= 99.5%
2. latence p95 `GET /api/products` <= 400 ms
3. taux `5xx` <= 0.5% sur 30 minutes glissantes
4. disponibilite readiness >= 99.9%

## Alerting minimal

Declencher une alerte si:

1. `health/ready` en echec 3 fois consecutives
2. `5xx` > 1% pendant 10 minutes
3. latence p95 > 800 ms pendant 15 minutes
4. pic de `401` ou `429` anormal (suspicion brute-force ou desynchronisation auth)

### Rollback apres release

Preconditions:

1. identifier le tag stable precedent (`vX.Y.Z`)
2. verifier les artefacts de la release precedente (API + Frontend)
3. informer les consommateurs API d'une fenetre de rollback

Procedure:

1. redeployer les artefacts du tag stable precedent
2. redemarrer l'API
3. verifier `GET /swagger/v1/swagger.json` et un endpoint de lecture (`GET /api/products`)
4. verifier les logs d'erreurs et les evenements Sentry 5 a 10 minutes apres rollback

### Sauvegarde PostgreSQL

Exemple local:

```bash
mkdir -p backups
docker exec advanceddevsample-postgres pg_dump -U postgres advanceddevsample > "backups/advanceddevsample-$(date +%Y%m%d-%H%M%S).sql"
```

Bonnes pratiques:

1. faire une sauvegarde avant migration/release
2. conserver plusieurs points de restauration
3. tester periodiquement une restauration a blanc

### Restauration PostgreSQL

Exemple local:

```bash
cat backups/advanceddevsample-YYYYMMDD-HHMMSS.sql | docker exec -i advanceddevsample-postgres psql -U postgres -d advanceddevsample
```

Puis:

1. redemarrer l'API
2. verifier les endpoints critiques
3. verifier les contraintes metier (auth, lecture produits, ecriture admin)

### Rotation du secret JWT

Impact:

- tous les JWT emis avant rotation deviennent invalides
- les utilisateurs doivent se reconnecter

Procedure recommandee:

1. generer un nouveau `JWT_SECRET` (>= 32 caracteres, aleatoire)
2. mettre a jour la variable d'environnement
3. redemarrer l'API dans une fenetre planifiee
4. monitorer les `401` pendant la periode de transition

## Publication applicative (hors docs)

Commande API:

```bash
dotnet publish AdvancedDevSample.Api/AdvancedDevSample.Api.csproj -c Release -o artifacts/api
```

Commande Frontend:

```bash
dotnet publish AdvancedDevSample.Frontend/AdvancedDevSample.Frontend.csproj -c Release -o artifacts/frontend
```

## Diagramme de deploiement

```mermaid
flowchart LR
  Dev[Developpeur] -->|push PR/main| GH[GitHub Repository]
  GH --> Q[Workflow quality.yml]
  GH --> S[Workflow security.yml]
  GH --> D[Workflow docs.yml]
  GH -->|tag v*| R[Workflow release.yml]

  D --> Pages[GitHub Pages<br/>Documentation MkDocs]
  R --> Rel[GitHub Release<br/>artifacts api/frontend]

  subgraph Runtime local
    FE[Blazor Frontend]
    API[ASP.NET Core API]
    DB[(PostgreSQL)]
    FE --> API
    API --> DB
  end
```

## Voir aussi

- [Configuration](configuration.md)
- [Securite](security.md)
- [Monitoring](monitoring.md)
- [Troubleshooting](troubleshooting.md)
