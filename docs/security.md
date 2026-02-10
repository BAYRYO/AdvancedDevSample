# Securite

## Authentification

- scheme `JwtBearer`
- validation issuer/audience/signature/expiration
- `ClockSkew = 0`
- secret JWT obligatoire (>= 32 caracteres)

## Autorisation

Politiques configurees:

- `AdminOnly`
- `UserOrAdmin`

Protection actuelle:

- `DELETE /api/products/{id}` -> admin
- `DELETE /api/categories/{id}` -> admin
- `/api/users/*` -> admin

## Refresh tokens

- token random 64 bytes (base64)
- hash SHA-256 stocke en base
- rotation sur `/api/auth/refresh`
- revocation des tokens actifs au login

## Rate limiting

Politique `login`:

- `5` requetes / minute / IP
- statut `429` en depassement

Appliquee sur:

- `POST /api/auth/login`

## Headers de securite

Middleware `SecurityHeadersMiddleware` ajoute:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: no-referrer`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- `Cross-Origin-Opener-Policy: same-origin`
- `Cross-Origin-Resource-Policy: same-origin`
- HSTS en HTTPS hors `Development`
- CSP stricte (specifique pour `/swagger` et `/scalar`, encore plus stricte ailleurs)

## CORS

Politique `Frontend`:

- origines via `Cors:AllowedOrigins`
- valeurs par defaut locales: `http://localhost:5173`, `https://localhost:7173`
- en Docker Compose, `http://localhost:8080` est injecte

## Telemetrie securite

- exceptions capturees dans Sentry
- breadcrumbs HTTP sur requete/reponse
- erreurs metier vs techniques distingues dans les reponses API
