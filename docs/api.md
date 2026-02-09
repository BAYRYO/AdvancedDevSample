# API

Base URL locale: `http://localhost:5069`

## Authentification

Les endpoints proteges utilisent un token JWT Bearer.

## Endpoints produits

- `POST /api/products`: creer un produit
- `GET /api/products`: rechercher/lister les produits
- `GET /api/products/{id}`: detail produit
- `PUT /api/products/{id}`: mise a jour
- `DELETE /api/products/{id}`: suppression
- `PUT /api/products/{id}/price`: changement de prix
- `POST /api/products/{id}/discount`: appliquer une reduction
- `DELETE /api/products/{id}/discount`: retirer une reduction
- `GET /api/products/{id}/price-history`: historique des prix
- `POST /api/products/{id}/activate`: activer
- `POST /api/products/{id}/deactivate`: desactiver

## Endpoints categories

- `POST /api/categories`: creer une categorie
- `GET /api/categories`: lister
- `GET /api/categories/{id}`: detail
- `PUT /api/categories/{id}`: mise a jour
- `DELETE /api/categories/{id}`: suppression

## Explorer l'API

- Swagger UI: `/swagger`
- Scalar: `/scalar/v1`
