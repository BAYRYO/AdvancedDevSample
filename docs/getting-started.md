# Demarrage

## Prerequis

- .NET SDK 10.x
- Git
- Python 3.11+ (uniquement pour la doc MkDocs)

Verifier rapidement:

```bash
dotnet --version
git --version
python --version
```

## Installation

```bash
git clone <url-du-repo>
cd AdvancedDevSample
dotnet restore AdvancedDevSample.slnx
```

## Configuration minimale

Copier le template:

Linux/macOS:

```bash
cp .env.example .env
```

PowerShell:

```powershell
Copy-Item .env.example .env
```

Variables essentielles:

- `JWT_SECRET` (obligatoire; >= 32 caracteres)
- `ADMIN_EMAIL` + `ADMIN_PASSWORD` (recommande en dev pour creer un admin)

## Lancer toute la stack avec Docker

```bash
docker compose up --build -d
```

## URLs en Docker

- PostgreSQL: `localhost:5432`
- API HTTP: `http://localhost:5069`
- Frontend: `http://localhost:8080`
- Swagger: `http://localhost:5069/swagger`
- Scalar: `http://localhost:5069/scalar/v1`

## Lancer sans Docker (optionnel)

Dans 2 terminaux:

```bash
dotnet run --project AdvancedDevSample.Api
```

```bash
dotnet run --project AdvancedDevSample.Frontend
```

## URLs de developpement

- API HTTP: `http://localhost:5069`
- API HTTPS: `https://localhost:7119`
- Frontend HTTP: `http://localhost:5173`
- Frontend HTTPS: `https://localhost:7173`

Le frontend pointe par defaut vers `http://localhost:5069` (`AdvancedDevSample.Frontend/wwwroot/appsettings.json`).

## Documentation API runtime (dev)

- Swagger: `http://localhost:5069/swagger`
- Scalar: `http://localhost:5069/scalar/v1`

## Base de donnees au premier demarrage

Au lancement de l'API:

- application des migrations EF si disponibles
- fallback `EnsureCreated()` en `Development` si aucune migration
- seeding automatique si `SeedDatabase=true` (dev)

## Executer les tests

```bash
dotnet test AdvancedDevSample.slnx
```

Avec couverture:

```bash
dotnet test AdvancedDevSample.slnx --collect:"XPlat Code Coverage"
```

## Lancer la documentation localement

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs serve
```

Ouvrir `http://127.0.0.1:8000`.

## Voir aussi

- [Configuration](configuration.md)
- [API](api.md)
- [Troubleshooting](troubleshooting.md)
