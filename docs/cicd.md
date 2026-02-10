# CI/CD

Le repository utilise GitHub Actions pour la qualite, la securite, Docker, les releases et la documentation.

## `quality.yml`

Declencheurs:

- push `main`
- pull request vers `main`

Actions:

1. restore/build/test couverture
2. seuils couverture
3. SonarQube + attente Quality Gate
4. verification formatage
5. verification derive migrations EF

## `security.yml`

Declencheurs:

- pull request `main`
- cron hebdomadaire (lundi 03:00 UTC)

Actions:

- dependency review (PR)
- CodeQL C#
- scan secrets Gitleaks

## `docker.yml`

Declencheurs:

- pull request `main` (sur chemins applicatifs/docker)
- push `main`
- push tags `v*`

Jobs:

- `docker-ci`
  - build images API et Frontend
  - `docker compose up -d --build`
  - tests de fumee (`/health/ready` API + disponibilite frontend)
- `docker-cd` (sur push)
  - login GHCR
  - buildx multi-arch (`linux/amd64`, `linux/arm64`)
  - push images API + Frontend

## `release.yml`

Declencheur:

- push tag `v*`

Actions:

1. build + tests
2. `dotnet publish` API + Frontend
3. packaging `.tar.gz`
4. creation release GitHub avec artefacts

## `docs.yml`

Declencheurs:

- push `main` sur changements docs
- execution manuelle

Actions:

1. install Python 3.12
2. install dependances docs
3. `mkdocs build --strict`
4. upload + deploy GitHub Pages

## Dependabot

Fichier: `.github/dependabot.yml`

- `nuget`: weekly
- `github-actions`: weekly
