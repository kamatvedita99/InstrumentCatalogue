# Instrument Catalogue API — Extended Features & V2 Roadmap

These are features that were designed and discussed but deliberately deferred to keep v1 scope clean. Each one has a clear design rationale documented here so they can be picked up without re-designing from scratch.

---

## Search Enhancements

### Search Filters
`SearchAsync` currently takes a plain `searchTerm`. A `SearchInstrumentFilter` object should be added:

```csharp
public class SearchInstrumentFilter
{
    public string SearchTerm { get; set; } = string.Empty;
    public InstrumentType? Type { get; set; }
    public InstrumentStatus? Status { get; set; } = InstrumentStatus.Active;
    public string? Exchange { get; set; }
    public int PageSize { get; set; } = 20;
    public string? Cursor { get; set; }
}
```

`Status` defaults to `Active` — most consumers want active instruments. Pass `null` explicitly for all statuses.

### Trigram Similarity Search
Currently using `ILIKE '%term%'` which is a full table scan. For production scale add `pg_trgm` extension and GIN index:

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX idx_instruments_name_trgm ON instruments USING GIN (name gin_trgm_ops);
CREATE INDEX idx_symbol_xref_symbol_trgm ON symbol_x_ref USING GIN (symbol gin_trgm_ops);
```

Then use `similarity()` or `%` operator for ranked fuzzy results.

---

## Vendor Field Mapping (V2)

A `vendor_field_mapping` table to map vendor-specific field names to canonical names in our system. Supports JSONB overrides for context-specific resolution (e.g. Bloomberg `PX_LAST` with `PRICE_TYPE=DIRTY` maps to `dirty_price`).

```csharp
public class VendorFieldMapping
{
    public Guid MappingId { get; set; }
    public int InterfaceId { get; set; }
    public string? InstrumentType { get; set; }    // scoped per type or null for all
    public string VendorFieldName { get; set; }
    public JsonDocument? Overrides { get; set; }   // {"PRICE_TYPE":"DIRTY"}
    public string CanonicalName { get; set; }
    public string DataType { get; set; }
    public bool IsActive { get; set; }
}
```

Enables generic ingestion layer — new vendor = new mapping rows, zero code changes.

---

## Market Data Service (Separate Service)

Catalogue is reference data only. Market data lives in a separate service using `instrument_id` as the join key:

- Last traded price
- Bid / ask spread
- Daily OHLCV
- Intraday NAV for ETFs
- Real-time dirty price for bonds

High write volume — separate DB, separate Redis cache strategy, separate deployment.

---

## VendorInterface Filter on Search

Allow consumers to filter instruments by which vendor interface covers them:

```
GET /instruments?vendorInterfaceId=1
```

Heavy join through `vendor_interface_symbol_x_ref`. Useful for ops teams auditing vendor coverage. Deferred due to query complexity.

---

## Symbol Audit Trail

Full audit log of who changed a symbol's validity or primary status and when. Currently only `LastUpdatedAtUtc` is tracked.

```csharp
public class SymbolXRefAudit
{
    public Guid AuditId { get; set; }
    public Guid SymbolXRefId { get; set; }
    public string Action { get; set; }        // Created, Closed, PrimaryChanged
    public DateTime ChangedAtUtc { get; set; }
    public string? Notes { get; set; }
}
```

---

## ETag / Conditional Requests on Snapshot

Add `If-None-Match` support to `GET /instruments/snapshot` so consumers can check if data changed before downloading 50k instruments:

```
GET /instruments/snapshot
If-None-Match: "abc123etag"

→ 304 Not Modified if nothing changed
→ 200 with full payload if changed
```

---

## Value Objects for Exchange, Currency, Country

Currently stored as plain strings with API-layer validation. A full DDD implementation would use value objects that self-validate against ISO standards at construction time:

- `Country` — ISO 3166-1 alpha-2
- `Currency` — ISO 4217
- `Exchange` — MIC codes (ISO 10383)

---

## TestContainers Integration Tests

Currently unit tests with mocked repositories. Add TestContainers to spin up real Postgres for integration tests:

```csharp
[Collection("Database")]
public class InstrumentRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    // spins up real Postgres, runs schema, tears down after
}
```

---
## Observability & Monitoring

### Planned (pre-July 23, high priority)
- **Serilog** — structured JSON logging, replaces default ASP.NET Core logging. Foundation for everything else.
- **Correlation IDs** — unique ID per request propagated through all log entries, so a single request can be traced end-to-end across all log lines.
- **Health check endpoints** — `/health` (is the process alive) and `/health/ready` (are dependencies — Postgres, Redis — reachable). Standard expectation for any deployed service.
- **Polly telemetry** — Polly v8 has built-in telemetry events (retry attempts, circuit state changes). Hook into these to log when the circuit opens/closes.

### Deferred (post-deployment, requires infrastructure)
- **Prometheus metrics** — expose `/metrics` endpoint, track request latency (p50/p99), cache hit/miss ratio, circuit breaker state.
- **Grafana dashboards** — visualize Prometheus metrics. Target dashboards: Resolve endpoint latency, Redis cache hit rate, circuit breaker state over time.
- **ELK stack** (Elasticsearch + Logstash + Kibana) — centralized log aggregation and search. Serilog has a direct Elasticsearch sink. Kibana for log visualization and alerting.

### Rationale for deferral
Infrastructure setup (Docker Compose with Prometheus + Grafana + ELK) is a substantial effort best done alongside the AWS deployment phase. Serilog + correlation IDs + health checks deliver the most interview value for the least setup cost and are achievable before July 23.


## Rate Limiting
- Protect the Resolve endpoint from OMS/market pricer overload
- ASP.NET Core built-in AddRateLimiter middleware
- Token bucket or sliding window algorithm
- Different limits per endpoint — Resolve gets higher limit than admin endpoints
- Deferred pending observability and deployment work

## Azure Support

Currently targeting AWS. Azure equivalents:
- EC2 → Azure App Service / AKS
- RDS → Azure Database for PostgreSQL
- S3 → Azure Blob Storage
- SQS → Azure Service Bus

---

*Last updated: June 2026*
*Owner: Vedita Kamat*
