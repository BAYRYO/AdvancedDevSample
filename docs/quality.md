# Tests et qualite

## Executer les tests

```bash
dotnet test AdvancedDevSample.slnx
```

## Couverture

```bash
dotnet test AdvancedDevSample.slnx --collect:"XPlat Code Coverage"
```

## Pipeline qualite locale

```bash
./eng/quality/quality.sh
```

Sous PowerShell :

```powershell
pwsh ./eng/quality/quality.ps1
```

## Controles CI principaux

- build complet
- tests unitaires et integration
- seuils de couverture
- formatage (`dotnet format --verify-no-changes`)
- verification derive EF migrations
