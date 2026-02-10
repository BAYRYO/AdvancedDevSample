# Monitoring

## Objectif

Le projet expose maintenant des metriques Prometheus via OpenTelemetry, avec une stack locale
Prometheus + Grafana preconfiguree pour suivre les SLO.

## Instrumentation API

L'API expose:

- `GET /metrics` (Prometheus scrape endpoint)
- `GET /health/live`
- `GET /health/ready`

Configuration OTLP optionnelle:

- variable d'environnement `OTEL_EXPORTER_OTLP_ENDPOINT`
- ou `OpenTelemetry:Otlp:Endpoint` dans `appsettings`

## Stack locale

Fichiers:

- `monitoring/docker-compose.yml`
- `monitoring/prometheus/prometheus.yml`
- `monitoring/prometheus/alert.rules.yml`
- `monitoring/grafana/provisioning/**`
- `monitoring/grafana/dashboards/api-observability.json`

Lancement:

```bash
docker compose -f monitoring/docker-compose.yml up -d
```

Acces:

- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000` (`admin` / `admin`)

## SLO monitores automatiquement

Regles Prometheus incluses:

1. `AdvancedDevSampleHigh5xxRatio` (>1% sur 10 min)
2. `AdvancedDevSampleHighLatencyP95` (>800ms sur 15 min)
3. `AdvancedDevSampleNoTraffic` (aucun trafic sur 10 min)

## Verification rapide

1. demarrer l'API localement (`dotnet run --project AdvancedDevSample.Api`)
2. verifier `http://localhost:5069/metrics`
3. verifier que la cible `advanceddevsample-api` est `UP` dans Prometheus
4. ouvrir le dashboard `AdvancedDevSample API Observability` dans Grafana
