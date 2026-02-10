# Monitoring

## Exposition API

L API expose:

- `GET /metrics` (Prometheus via OpenTelemetry)
- `GET /health/live`
- `GET /health/ready`

```mermaid
flowchart LR
  API[API AdvancedDevSample] --> MET[/metrics/]
  API --> LIVE[/health/live/]
  API --> READY[/health/ready/]
```

## Instrumentation

OpenTelemetry configure:

- traces: ASP.NET Core + HttpClient
- metriques: ASP.NET Core + HttpClient + exporter Prometheus
- export OTLP optionnel via `OTEL_EXPORTER_OTLP_ENDPOINT` ou `OpenTelemetry:Otlp:Endpoint`

```mermaid
flowchart LR
  REQ[Requetes HTTP] --> OTel[Kit OpenTelemetry]
  OTel --> TR[Traces]
  OTel --> M[Metriques]
  TR --> OTLP[Endpoint OTLP optionnel]
  M --> PROM[/metrics pour Prometheus]
```

## Stack locale

Fichiers:

- `docker-compose.yml`
- `monitoring/prometheus/prometheus.yml`
- `monitoring/prometheus/alert.rules.yml`
- `monitoring/grafana/provisioning/**`
- `monitoring/grafana/dashboards/api-observability.json`

Lancement:

```bash
docker compose --profile monitoring up -d
```

Acces:

- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000` (`admin` / `admin`)

## Alertes par defaut

- `AdvancedDevSampleHigh5xxRatio`
- `AdvancedDevSampleHighLatencyP95`
- `AdvancedDevSampleNoTraffic`

```mermaid
flowchart TD
  P[Collecte Prometheus] --> R[Regles d enregistrement et alertes]
  R --> A1[High5xxRatio]
  R --> A2[HighLatencyP95]
  R --> A3[NoTraffic]
  A1 --> N[Canal d alerte]
  A2 --> N
  A3 --> N
```

## Verification rapide

1. lancer la stack monitoring
2. verifier `http://localhost:5069/metrics`
3. verifier la cible API en `UP` dans Prometheus
4. ouvrir le dashboard Grafana `AdvancedDevSample API Observability`

```mermaid
flowchart LR
  S[Demarrer le profil monitoring] --> M[Verifier /metrics]
  M --> T[Cible API UP dans Prometheus]
  T --> G[Ouvrir le dashboard Grafana]
```
