# Troubleshooting

## `JWT_SECRET environment variable is not set`

Cause:

- variable absente ou vide

Correction:

1. definir `JWT_SECRET`
2. longueur >= 32 caracteres
3. redemarrer l'API

## `JWT_SECRET is too short`

Cause:

- secret trop court

Correction:

- utiliser une cle aleatoire robuste (>= 32 caracteres)

## Erreur CORS depuis le frontend

Cause possible:

- origin frontend non incluse dans `Cors:AllowedOrigins`

Correction:

- ajouter l'origine exacte dans `appsettings.Development.json` ou variable d'env equivalente

## `401 Unauthorized` apres login

Causes possibles:

- token expire
- audience/issuer incoherents
- role insuffisant sur endpoint protege

Verification rapide:

- tester via Swagger avec le meme token
- verifier `Jwt:Issuer`/`Jwt:Audience`

## `429 Too Many Requests` sur login

Cause:

- rate limiter (5 requetes/min/IP)

Correction:

- attendre la prochaine fenetre ou utiliser une IP differente en test

## Erreur migration EF en CI

Cause:

- modele EF modifie sans migration associee

Correction:

1. generer migration
2. committer migration + snapshot
3. relancer pipeline

## Docs MkDocs: build en echec

Cause frequente:

- liens casses ou markdown invalide (`--strict`)

Correction:

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs build --strict
```

Corriger les erreurs indiquees.
