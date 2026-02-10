# CI/CD

Le repository utilise GitHub Actions pour la qualite, la securite, Docker, les releases et la documentation.

## Vue pipeline

```mermaid
flowchart TD
  PR[Pull Request vers main] --> Q[quality.yml]
  PR --> S[security.yml]
  PR --> DCI[docker.yml job docker-ci]

  MAIN[Push sur main] --> Q
  MAIN --> DCI
  MAIN --> DCD[docker.yml job docker-cd]
  MAIN --> VULN[docker.yml job image-vuln-scan]
  MAIN --> DOCS[docs.yml]

  TAG[Push tag v*] --> REL[release.yml]
  TAG --> DCD
```

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

```mermaid
flowchart LR
  B[build] --> T[tests]
  T --> C[couverture]
  C --> SQ[SonarQube]
  SQ --> QG[Quality Gate]
  QG --> F[format]
  F --> M[migrations drift]
```

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
  - generation SBOM + provenance
  - signature Cosign keyless des images API + Frontend
  - push images API + Frontend
- `image-vuln-scan` (sur push)
  - scan Trivy sur les images publiees
  - blocage sur vulnerabilites `HIGH`/`CRITICAL`

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
