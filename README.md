# AdvancedDevSample

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=BAYRYO_AdvancedDevSample&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=BAYRYO_AdvancedDevSample)
[![Quality](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/quality.yml/badge.svg)](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/quality.yml)
[![Security](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/security.yml/badge.svg)](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/security.yml)
[![Docker CI/CD](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/docker.yml/badge.svg)](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/docker.yml)
[![Docs](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/docs.yml/badge.svg)](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/docs.yml)
[![Release](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/release.yml/badge.svg)](https://github.com/BAYRYO/AdvancedDevSample/actions/workflows/release.yml)

`AdvancedDevSample` est une reference full-stack .NET 10 orientee production.

Le projet couvre une application complete avec:

- architecture en couches (API, Application, Domaine, Infrastructure, Frontend)
- authentification JWT + refresh token avec rotation
- persistance EF Core 10 + PostgreSQL + migrations
- observabilite (checks de sante, metriques Prometheus, Sentry)
- qualite automatisee (tests, couverture, format, Sonar, scans securite)
- documentation MkDocs publiee automatiquement

## Demarrage rapide

### Prerequis

- .NET SDK 10.x
- Docker + Docker Compose (recommande)
- Git
- Python 3.11+ (uniquement pour MkDocs)

### Configuration locale

Linux/macOS:

```bash
cp .env.example .env
```

PowerShell:

```powershell
Copy-Item .env.example .env
```

Variables importantes:

- `JWT_SECRET` (obligatoire, minimum 32 caracteres)
- `ADMIN_EMAIL` et `ADMIN_PASSWORD` (admin seed en dev)
- `OTEL_EXPORTER_OTLP_ENDPOINT` (optionnel)
- `SENTRY_DSN` (optionnel)

### Lancer toute la stack

```bash
docker compose up --build -d
```

Services disponibles:

- API: `http://localhost:5069`
- Frontend: `http://localhost:8080`
- Swagger: `http://localhost:5069/swagger`
- Scalar: `http://localhost:5069/scalar/v1`
- PostgreSQL: `localhost:5432`

Avec monitoring local:

```bash
docker compose --profile monitoring up -d
```

- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`

### Lancer sans Docker

```bash
dotnet restore AdvancedDevSample.slnx
dotnet run --project AdvancedDevSample.Api
dotnet run --project AdvancedDevSample.Frontend
```

URLs de dev:

- API HTTP: `http://localhost:5069`
- API HTTPS: `https://localhost:7119`
- Frontend HTTP: `http://localhost:5173`
- Frontend HTTPS: `https://localhost:7173`

## Qualite

```bash
dotnet test AdvancedDevSample.slnx
./eng/quality/quality.sh
```

PowerShell:

```powershell
pwsh ./eng/quality/quality.ps1
```

## Documentation

- site public: `https://bayryo.github.io/AdvancedDevSample/`
- index local: `docs/index.md`

Source de verite documentaire:

- `README.md` reste un guide de demarrage synthetique
- les details operationnels et techniques font foi dans `docs/getting-started.md`, `docs/configuration.md`, `docs/api.md`, `docs/operations.md`

Lancer localement:

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs serve
```

## Workflows GitHub Actions

- `quality.yml`: build, tests, couverture, Sonar, format, derive EF
- `security.yml`: dependency review, CodeQL, Gitleaks
- `docker.yml`: build images, tests de fumee compose, publication GHCR
- `release.yml`: artefacts API/Frontend sur tags `v*`
- `docs.yml`: build MkDocs strict + deployment GitHub Pages

## Structure du repository

```text
AdvancedDevSample/
├── AdvancedDevSample.Api/
├── AdvancedDevSample.Application/
├── AdvancedDevSampleDomain/
├── AdvancedDevSample.Infrastructure/
├── AdvancedDevSample.Frontend/
├── AdvancedDevSample.Test/
├── docs/
├── monitoring/
└── eng/quality/
```

## Licence

MIT. Voir `LICENSE`.
