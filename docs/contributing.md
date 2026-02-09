# Contribution

## Workflow recommande

1. creer une branche feature depuis `main`
2. implementer la modification avec tests associes
3. executer pipeline qualite locale
4. ouvrir une pull request

## Standards de code

- suivre `.editorconfig`
- conserver separation des couches
- eviter dependances inverses (domaine -> infra interdit)
- ajouter des tests pour toute logique metier/modif endpoint

## Checklist avant PR

- build OK
- tests OK
- couverture conforme
- formatage OK
- migrations EF a jour (si modele modifie)
- documentation mise a jour (`docs/` + `README` si necessaire)

## Gouvernance documentation

Regles minimales:

1. toute PR qui modifie comportement/config/API doit mettre a jour la doc associee
2. les exemples de commandes doivent rester testables
3. preferer des liens Markdown internes plutot que des references texte brutes
4. conserver l'alignement doc/code (routes, statuts, variables d'environnement)

Verification recommandee avant merge:

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs build --strict
```

## Commandes utiles

```bash
dotnet restore AdvancedDevSample.slnx
dotnet build AdvancedDevSample.slnx
dotnet test AdvancedDevSample.slnx
./eng/quality/quality.sh
```

## Convention de versions/release

- tag Git format `vX.Y.Z`
- pousse sur le tag pour declencher `release.yml`

## Voir aussi

- [Tests et qualite](quality.md)
- [CI/CD](cicd.md)
- [Deploiement de la documentation](docs-deployment.md)
