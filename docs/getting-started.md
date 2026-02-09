# Demarrage

## Prerequis

- .NET SDK 10.x
- Git
- Python 3.11+ (uniquement pour construire la documentation en local)

## Lancer le projet en local

```bash
dotnet restore AdvancedDevSample.slnx
dotnet run --project AdvancedDevSample.Api
dotnet run --project AdvancedDevSample.Frontend
```

## URLs de developpement

- API HTTP: `http://localhost:5069`
- API HTTPS: `https://localhost:7119`
- Frontend HTTP: `http://localhost:5173`
- Frontend HTTPS: `https://localhost:7173`

## Documentation API runtime

- Swagger UI: `http://localhost:5069/swagger`
- Scalar: `http://localhost:5069/scalar/v1`

## Construire la documentation (MkDocs)

```bash
python -m pip install -r docs/requirements.txt
mkdocs serve
```

Puis ouvrez `http://127.0.0.1:8000`.
