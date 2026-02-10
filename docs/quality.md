# Tests et qualite

## Couverture des tests

Projet `AdvancedDevSample.Test`:

- domaine (entites, value objects)
- application (services, validateurs)
- integration API (controllers, middlewares, sante, metriques)
- persistance EF/repositories
- frontend (services et composants)

## Commandes utiles

```bash
dotnet test AdvancedDevSample.slnx
```

```bash
dotnet test AdvancedDevSample.slnx --collect:"XPlat Code Coverage"
```

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

## Seuils couverture

Appliques via `eng/quality/check-coverage.*`:

- global lignes >= `60%`
- `AdvancedDevSample.Infrastructure` >= `45%`
- `AdvancedDevSample.Frontend` >= `8%`

## Controles CI complementaires

`quality.yml` ajoute:

- SonarQube + Quality Gate bloquante
- verification `dotnet format --verify-no-changes`
- verification derive EF (`has-pending-model-changes`)

## Avant PR

- executer `./eng/quality/quality.sh`
- ajouter/mettre a jour les tests des comportements modifies
- mettre a jour la documentation associee
