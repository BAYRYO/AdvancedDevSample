# AdvancedDevSample

Exemple complet d'application .NET 10 basee sur Clean Architecture / DDD :

- API ASP.NET Core avec JWT, rate limiting, middlewares de securite, seeding, Swagger/Scalar
- Frontend Blazor WebAssembly
- Persistance EF Core SQLite (migrations + seeders)
- Suite de tests unitaires/integration/frontend
- Pipeline qualite/securite/release/docs sur GitHub Actions

## Documentation complete

La documentation detaillee est disponible dans `docs/` et publiee via MkDocs.

- **Version publiee**: `https://bayryo.github.io/AdvancedDevSample/`
- **Accueil docs locale**: `docs/index.md`

## Demarrage rapide

### 1) Prerequis

- .NET SDK 10.x
- Git
- Python 3.11+ (uniquement pour MkDocs)

### 2) Variables d'environnement

Copier le template et adapter les secrets:

Linux/macOS:

```bash
cp .env.example .env
```

PowerShell:

```powershell
Copy-Item .env.example .env
```

Variables principales:

- `JWT_SECRET` (obligatoire, minimum 32 caracteres)
- `SENTRY_DSN` (optionnel)
- `ADMIN_EMAIL` + `ADMIN_PASSWORD` (pour seeding admin en dev)

### 3) Lancer en local

```bash
dotnet restore AdvancedDevSample.slnx
dotnet run --project AdvancedDevSample.Api
dotnet run --project AdvancedDevSample.Frontend
```

URLs de developpement:

- API HTTP: `http://localhost:5069`
- API HTTPS: `https://localhost:7119`
- Frontend HTTP: `http://localhost:5173`
- Frontend HTTPS: `https://localhost:7173`

API docs runtime (en `Development`):

- Swagger UI: `http://localhost:5069/swagger`
- Scalar: `http://localhost:5069/scalar/v1`

## Tests et qualite

```bash
# Tous les tests
dotnet test AdvancedDevSample.slnx

# Couverture
dotnet test AdvancedDevSample.slnx --collect:"XPlat Code Coverage"
```

Pipeline qualite locale (equivalent CI):

```bash
./eng/quality/quality.sh
# ou
pwsh ./eng/quality/quality.ps1
```

## MkDocs en local

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs serve
```

Puis ouvrir `http://127.0.0.1:8000`.

## Workflows GitHub Actions

- `quality.yml`: build/test/couverture/format/migrations/SonarQube
- `security.yml`: dependency review, CodeQL, Gitleaks
- `release.yml`: build + artefacts sur tags `v*`
- `docs.yml`: build/deploiement GitHub Pages

## Structure du repository

```text
AdvancedDevSample/
├── AdvancedDevSample.Api/             # API ASP.NET Core
├── AdvancedDevSample.Application/     # Cas d'usage, DTOs, interfaces applicatives
├── AdvancedDevSampleDomain/           # Entites, value objects, interfaces domaine
├── AdvancedDevSample.Infrastructure/  # EF Core, repositories, persistence, seeders
├── AdvancedDevSample.Frontend/        # Blazor WebAssembly
├── AdvancedDevSample.Test/            # Tests unitaires/integration/frontend
├── docs/                              # Documentation MkDocs
└── eng/quality/                       # Scripts qualite locale
```

## Licence

Projet a finalite pedagogique. Adaptez la licence selon vos besoins.
