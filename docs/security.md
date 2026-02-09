# Securite

## Authentification

- scheme `JwtBearer`
- verification issuer/audience/signature/expiration
- `ClockSkew = 0` (pas de marge)
- secret JWT impose: minimum 32 caracteres

## Autorisation

Politiques configurees:

- `AdminOnly`: role `Admin`
- `UserOrAdmin`: roles `User` ou `Admin`

Exemples d'usage:

- suppression produit/categorie: admin uniquement
- endpoints `/api/users`: admin uniquement

## Refresh token

- token random 64 bytes (Base64)
- stockage en hash SHA-256 (`TokenHash`)
- rotation a chaque refresh
- revocation des anciens tokens a la connexion

## Rate limiting

Politique `login`:

- `5` requetes / minute / IP
- depassement: HTTP `429`

Appliquee sur:

- `POST /api/auth/login`

## Headers de securite

Middleware `SecurityHeadersMiddleware`:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- `Cross-Origin-Opener-Policy: same-origin`
- `Cross-Origin-Resource-Policy: same-origin`
- HSTS en HTTPS hors `Development`
- CSP differenciee pour `/swagger` et `/scalar`

## CORS

Politique `Frontend`:

- origines configurees via `Cors:AllowedOrigins`
- valeurs par defaut: `http://localhost:5173`, `https://localhost:7173`

## Monitoring des erreurs

- integration Sentry API
- breadcrumbs HTTP + contexte requete
- capture differenciee selon type d'erreur

## Bonnes pratiques recommandees

- ne jamais versionner de vrais secrets
- utiliser un `JWT_SECRET` long et aleatoire
- durcir CORS en production (origines exactes)
- activer et surveiller Sentry en environnements non-dev
