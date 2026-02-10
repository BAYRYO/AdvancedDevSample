# Deploiement de la documentation

La documentation est publiee via `.github/workflows/docs.yml`.

## Declencheurs

- `push` sur `main` avec changements sur:
  - `docs/**`
  - `mkdocs.yml`
  - `.github/workflows/docs.yml`
- `workflow_dispatch`

## Prerequis GitHub

Dans le repository:

1. `Settings > Pages`
2. Source: `GitHub Actions`

## Pipeline

1. checkout
2. setup Python 3.12
3. install dependances (`docs/requirements.txt`)
4. `mkdocs build --strict`
5. upload artefact `site`
6. deploy via `actions/deploy-pages`

## URL

`https://bayryo.github.io/AdvancedDevSample/`

## Verification locale

```bash
python3 -m pip install -r docs/requirements.txt
python3 -m mkdocs build --strict
python3 -m mkdocs serve
```
