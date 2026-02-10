# Tests et qualite

## Strategie de test

`AdvancedDevSample.Test` couvre:

- domaine (entites, value objects)
- application (services, validateurs)
- integration API (controllers, middlewares)
- persistance (PostgreSQL/in-memory)
- frontend (services/composants)

## Lancer les tests

```bash
dotnet test AdvancedDevSample.slnx
```

## Couverture

```bash
dotnet test AdvancedDevSample.slnx --collect:"XPlat Code Coverage"
```

Fichier de configuration coverage:

- `eng/quality/coverage.runsettings`

## Pipeline qualite locale

Linux/macOS:

```bash
./eng/quality/quality.sh
```

PowerShell:

```powershell
pwsh ./eng/quality/quality.ps1
```

Le pipeline local execute:

1. restore
2. build
3. tests + couverture
4. verification seuils couverture
5. verification formatage

## Seuils de couverture

Dans la pipeline qualite (`quality.sh` / `quality.ps1`):

- couverture globale lignes >= `60%`
- couverture lignes `AdvancedDevSample.Infrastructure` >= `45%`
- couverture lignes `AdvancedDevSample.Frontend` >= `8%` (seuil anti-regression)

Scripts d'analyse:

- `eng/quality/check-coverage.sh`
- `eng/quality/check-coverage.ps1`

## Formatage

```bash
dotnet format AdvancedDevSample.slnx --verify-no-changes --severity error --verbosity minimal
```

## Verifications CI additionnelles

Le workflow CI `quality.yml` ajoute:

- SonarQube (Quality Gate)
- controle derive modele EF (`has-pending-model-changes`)

## Conseils avant PR

- executer `./eng/quality/quality.sh`
- verifier que les nouveaux endpoints sont testes
- verifier les cas d'erreur (400/401/403/404/409/500)
