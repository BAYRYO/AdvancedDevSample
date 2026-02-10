# Depannage

## `JWT_SECRET environment variable is not set`

Cause:

- variable absente ou vide

Correction:

1. definir `JWT_SECRET`
2. garantir une longueur >= 32 caracteres
3. redemarrer l'API

## `JWT_SECRET is too short`

Cause:

- secret trop court

Correction:

- utiliser un secret aleatoire robuste d'au moins 32 caracteres

## Erreurs CORS

Cause:

- origine frontend absente de `Cors:AllowedOrigins`

Correction:

1. ajouter l'origine exacte
2. en Docker, inclure `http://localhost:8080` si necessaire
3. redemarrer l'API

## `401 Unauthorized` apres login

Causes frequentes:

- token expire
- issuer/audience non alignes
- refresh token invalide/revoque

Verification:

1. tester le meme token dans Swagger
2. verifier `Jwt:Issuer` et `Jwt:Audience`

## `429 Too Many Requests` sur login

Cause:

- rate limiter (`5/min/IP`) sur `POST /api/auth/login`

Correction:

- attendre la fenetre suivante

## API inaccessible en Docker

Verification:

1. `docker compose ps`
2. `curl http://localhost:5069/health/ready`
3. `docker compose logs api --tail=200`

## Frontend inaccessible en Docker

Verification:

1. `docker compose ps`
2. `curl http://localhost:8080`
3. `docker compose logs frontend --tail=200`

## Erreur derive EF en CI

Cause:

- modele EF modifie sans migration associee

Correction:

1. generer la migration
2. committer migration + snapshot
3. relancer CI

## Echec build docs MkDocs

Cause:

- liens casses ou markdown invalide (`--strict`)

Correction:

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs build --strict
```

Corriger les erreurs remontees par la commande.
