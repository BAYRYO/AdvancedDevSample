# Deploiement de la documentation

La documentation est publiee automatiquement via GitHub Actions sur GitHub Pages.

## Workflow

Fichier : `.github/workflows/docs.yml`

Declencheurs :

- push sur `main` avec changements sur la documentation
- execution manuelle (`workflow_dispatch`)

## Prerequis GitHub

Dans `Settings > Pages` du repository :

- Source: `GitHub Actions`

## URL de publication

Si le repository est `BAYRYO/AdvancedDevSample`, la documentation sera disponible sur :

`https://bayryo.github.io/AdvancedDevSample/`
