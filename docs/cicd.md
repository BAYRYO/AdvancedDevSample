# CI/CD

## Vue globale

Le repository utilise GitHub Actions pour:

- qualite applicative
- securite
- release
- publication documentation

## `quality.yml`

Declencheurs:

- push sur `main`
- pull request vers `main`

Etapes principales:

1. checkout
2. setup .NET 10
3. setup Java (scanner Sonar)
4. validation variables/secrets Sonar
5. restore/build/test + couverture
6. verification seuils couverture
7. fin analyse Sonar (Quality Gate bloquante)
8. `dotnet format --verify-no-changes`
9. check derive migrations EF

Variables/secrets attendus:

- `SONAR_HOST_URL`
- `SONAR_PROJECT_KEY`
- `SONAR_ORGANIZATION`
- `SONAR_TOKEN`

## `security.yml`

Declencheurs:

- pull request sur `main`
- cron hebdomadaire (lundi 03:00 UTC)

Jobs:

- dependency review (PR)
- CodeQL C#
- scan secrets Gitleaks

## `release.yml`

Declencheur:

- push tag `v*` (ex: `v1.0.0`)

Pipeline:

1. restore/build/test
2. publish API vers `artifacts/api`
3. publish Frontend vers `artifacts/frontend`
4. archive `.tar.gz`
5. creation release GitHub avec artefacts

## `docs.yml`

Declencheurs:

- push `main` sur changements docs
- execution manuelle

Pipeline:

1. setup Python 3.12
2. install `docs/requirements.txt`
3. `mkdocs build --strict`
4. upload artifact
5. deploy GitHub Pages

## Dependabot

Fichier: `.github/dependabot.yml`

- ecosysteme `nuget`: hebdomadaire
- ecosysteme `github-actions`: hebdomadaire
