# Roadmap

## Horizon court terme (0-30 jours)

1. enrichir les tests de parcours frontend (auth + CRUD principaux)
2. augmenter la couverture sur scenarios erreurs API/infra
3. affiner UX (etats vides, feedback d'erreur, pagination)

## Horizon moyen terme (30-90 jours)

1. ajouter tests end-to-end naviguant frontend + API
2. durcir la configuration production (CORS, headers, secrets rotation)
3. enrichir dashboards et alertes selon SLI/SLO critiques

## Horizon long terme (90+ jours)

1. versionnement API explicite et politique de deprecation
2. verification systematique des signatures Cosign et attestations SBOM en phase de deploiement
3. strategie multi-environnements (staging/prod) documentee

## Definition d'excellence

- couverture test elevee sur flux critiques
- observabilite exploitable en incident
- documentation toujours alignee code/workflows
- securite automatisee et surveillee en continu
