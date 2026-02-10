# Contribution

## Workflow

1. creer une branche depuis `main`
2. implementer le changement avec tests
3. executer les validations locales
4. ouvrir une PR claire et ciblee

## Standards

- respecter `.editorconfig`
- conserver le decouplage des couches
- tester les comportements modifies
- maintenir docs + README alignes

## Checklist avant PR

- `dotnet restore AdvancedDevSample.slnx`
- `dotnet build AdvancedDevSample.slnx`
- `dotnet test AdvancedDevSample.slnx`
- `./eng/quality/quality.sh` (ou `pwsh ./eng/quality/quality.ps1`)
- migrations EF a jour si modele modifie
- docs mises a jour si API/config/comportement change

## Verification docs locale

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs build --strict
```

## Convention release

- tag format `vX.Y.Z`
- push du tag pour declencher `release.yml`

## Voir aussi

- [Tests et qualite](quality.md)
- [CI/CD](cicd.md)
- [Deploiement docs](docs-deployment.md)
