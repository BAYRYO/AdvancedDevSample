# Deploiement de la documentation

La documentation est publiee automatiquement sur GitHub Pages via `.github/workflows/docs.yml`.

## Conditions de declenchement

- `push` sur `main` avec changements:
  - `docs/**`
  - `mkdocs.yml`
  - `.github/workflows/docs.yml`
- execution manuelle (`workflow_dispatch`)

## Prerequis GitHub

Dans le repository GitHub:

1. `Settings > Pages`
2. Source: `GitHub Actions`

## Pipeline de publication

1. checkout
2. setup Python 3.12
3. installation dependances docs (`docs/requirements.txt`)
4. build strict (`mkdocs build --strict`)
5. upload artefact `site`
6. deploiement `actions/deploy-pages`

## URL de publication

Pour `BAYRYO/AdvancedDevSample`:

- `https://bayryo.github.io/AdvancedDevSample/`

## Verification locale avant push

```bash
python -m pip install -r docs/requirements.txt
mkdocs build --strict
mkdocs serve
```

Corriger toutes les erreurs de build strict avant merge.
