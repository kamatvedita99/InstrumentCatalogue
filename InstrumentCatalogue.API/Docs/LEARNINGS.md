# Instrument Catalogue API — Learnings & Design Decisions

A running document of concepts learned, decisions made, and the reasoning behind them. Useful for interview prep and continuing in a new session.

---

## Architecture

### Clean Architecture — 4 Layer Structure
```
API           → entry point, controllers, middleware, DI wiring
Application   → business logic, services, DTOs, validators, mappers
Infrastructure → data access, EF Core, Dapper, repositories
Core          → domain models, interfaces, enums, read models, common types
```

**Rule:** arrows point inward only. Core knows nothing. Everything knows Core.

**Why:** swapping Postgres for SQL Server means changing Infrastructure only. Core and Application don't change. This is Dependency Inversion — the D in SOLID.

**Dependency references:**
- API → Application + Infrastructure
- Application → Core
- Infrastructure → Core
- Core → nothing

---

### Why Repository Interfaces in Core?

We use both Dapper (reads) and EF Core (writes). The interface hides which tool is used. Application layer calls `IInstrumentRepository.GetByIdAsync` without knowing if it's Dapper or EF Core underneath.

Also enables mocked repositories in unit tests without spinning up a real database.

**Future:** add TestContainers for integration tests against real Postgres — documented in ExtendedFeatures.md.

---

### Read Models vs Domain Models

**Domain models** (`Instrument`, `BondRefData`) — designed for business operations. Full navigation properties, all fields, used for writes and complex logic.

**Read models** (`InstrumentSearchResult`) — lightweight projections for specific queries. No navigation properties, only the fields a particular view needs. Live in `Core/ReadModels/`.

**Why separate:** search query never loads a full Instrument with all navigation properties. A flat projection mapped directly from a Dapper query is faster and purpose-built.

---

## Domain Design Decisions

### Symbology Table (SymbolXRef)
An instrument can have many identifiers — ISIN, CUSIP, TICKER, BBGID, RIC. Instead of columns on the instrument table, we have a cross-reference table.

**Key insight:** adding a new identifier type (e.g. SEDOL, FIGI) = one INSERT into `Symbology`. Zero schema changes, zero code changes.

**Temporal history:** `ValidFrom` and `ValidTo` on each symbol row. ISINs can change when companies redomicile. Old ISIN remains resolvable for historic positions. `ValidTo = 9999-12-31` sentinel date = current symbol.

**`IsPrimary`** — nullable bool. An instrument can have NSE ticker and BSE ticker. `IsPrimary = true` marks which to display by default. Nullable because not all symbology types need a primary (ISIN is globally unique — no concept of primary).

---

### Vendor / Interface Separation
Bloomberg is a vendor. Bloomberg Terminal and Bloomberg B-PIPE are two different interfaces — different formats, different latencies, different SLAs. Storing just "Bloomberg" loses that granularity.

`VendorInterfaceSymbolXRef` — many-to-many between a symbol and the vendor interfaces that reported it. Provenance tracking. "Bloomberg B-PIPE and Refinitiv Elektron both confirmed ISIN INE002A01018."

---

### Shared Primary Key Pattern (Extension Tables)
`EquityRefData`, `BondRefData`, `EtfRefData` use `InstrumentId` as both PK and FK.

**Why:** one equity will always have exactly one `EquityRefData` row. Shared PK enforces this at the database level. A separate UUID PK that nobody ever uses is just noise.

---

### Sentinel Date Pattern
`ValidTo = '9999-12-31'` instead of `NULL` for "currently active" records.

**Why:** NULL in range queries causes subtle bugs. `BETWEEN ValidFrom AND ValidTo` silently excludes NULL rows in most databases. Sentinel date means `WHERE ValidTo = '9999-12-31'` always works cleanly with no null handling.

Used in: `SymbolXRef`, `InstrumentStatusHistory`.

---

### BondType vs BondStructure
`BondType` — the category: Government, Corporate, Municipal, Agency.

`BondStructure` — the structural feature: Standard, ZeroCoupon, Convertible, Callable, Puttable.

**Why separate:** a Convertible bond is usually a Corporate bond. ZeroCoupon can be Government or Corporate. Mixing them in one enum conflates category with feature.

---

### Market Data is Out of Scope
Catalogue is reference data only. `dirty_price`, `last_price`, `NAV` are market data — they change every second during market hours.

Mixing them in the catalogue would mean 100,000+ writes per day instead of 100. Cache invalidation becomes impossible. Audit trail gets polluted with price noise.

**Market data service** (v2) — separate service, uses `InstrumentId` as FK. Documented in ExtendedFeatures.md.

---

## API Design Decisions

### Endpoint Map
```
GET  /api/v1/instruments                     → filtered paginated list
GET  /api/v1/instruments/{id}                → full instrument + ref data
GET  /api/v1/instruments/{id}/symbols        → all symbols
GET  /api/v1/instruments/{id}/status-history → status timeline
GET  /api/v1/instruments/resolve             → symbol → instrument lookup
GET  /api/v1/instruments/search              → fuzzy name/symbol search
GET  /api/v1/instruments/snapshot            → bulk export

PATCH /api/v1/instruments/{id}               → update base fields
PATCH /api/v1/instruments/{id}/ref-data      → update type-specific fields
PATCH /api/v1/instruments/{id}/status        → status transition
POST  /api/v1/instruments                    → create instrument
POST  /api/v1/instruments/{id}/symbols       → add symbol
PATCH /api/v1/instruments/{id}/symbols/{sid} → close symbol

GET   /api/v1/vendors
POST  /api/v1/vendors
PATCH /api/v1/vendors/{id}
GET   /api/v1/vendors/{id}/interfaces
POST  /api/v1/vendors/{id}/interfaces
PATCH /api/v1/vendors/{id}/interfaces/{iid}
```

---

### PUT vs PATCH
**PUT** — replace entire resource. Send all fields.
**PATCH** — update specific fields only. Send only what changed.

We use PATCH for instruments. You'd never resend 20 fields to update one. Status transition is PATCH — only changing status. Symbol closing is PATCH — only setting ValidTo.

---

### Single Search Endpoint — Not God Endpoint
`GET /instruments` uses `InstrumentFilter` with nested type-specific filters:

```csharp
public class InstrumentFilter
{
    public InstrumentType? Type { get; set; }
    public string? Exchange { get; set; }
    public string? Currency { get; set; }
    public InstrumentStatus? Status { get; set; }
    public BondSearchFilter? Bond { get; set; }    // ignored if Type != Bond
    public EquitySearchFilter? Equity { get; set; }
    public EtfSearchFilter? Etf { get; set; }
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = 20;
}
```

Adding a new bond field = add one field to `BondSearchFilter`. Nothing else changes.

---

### Cursor Pagination vs Offset
**Offset:** `OFFSET 500 LIMIT 100` — DB scans and discards 500 rows. Gets slower at depth. Rows can shift if new data is inserted.

**Cursor:** `WHERE instrument_id > 'last-seen-id' LIMIT 100` — DB seeks directly to the right position. Consistent at any depth. Stable against inserts.

**`PagedResult<T>`** — just two fields. `Items` and `NextCursor`. `NextCursor == null` means last page. No `TotalCount`, no `TotalPages` — not needed with cursor pagination.

---

### Search — How It Works
Searches across `instruments.name` AND `symbol_x_ref.symbol` simultaneously via JOIN.

**Display symbol priority (COALESCE fallback chain):**
1. Symbol that matched the search term
2. Primary symbol (`is_primary = true`)
3. Any current symbol
4. Instrument name (last resort)

**Why:** "Instrument is nothing without a symbol." Every search result shows a symbol consistently. `MatchedOnSymbol` boolean tells the UI whether to highlight the symbol or name.

**SQL pattern:**
```sql
SELECT i.*, 
  COALESCE(
    MAX(CASE WHEN s.symbol ILIKE '%term%' THEN s.symbol END),
    MAX(CASE WHEN s.is_primary = true THEN s.symbol END),
    MAX(s.symbol),
    i.name
  ) as display_symbol
FROM instruments i
LEFT JOIN symbol_x_ref s ON s.instrument_id = i.instrument_id
  AND s.valid_to = '9999-12-31'
WHERE i.name ILIKE '%term%' OR s.symbol ILIKE '%term%'
GROUP BY i.instrument_id, i.name, ...
```

---

### Bulk Snapshot vs GetAll
`GetAllAsync` — paginated, cursor-based, for browsing. 2,500 calls to get 50k instruments.
`GetSnapshotAsync` — returns `IEnumerable<Instrument>` for streaming. For systems that seed a local cache on startup. One call, everything.

---

## C# Patterns Learned

### async/await
- `async` tells the compiler the method can be paused and resumed
- `await` frees the thread during IO — not parallel processing
- Never use `.Result` or `.Wait()` — causes deadlocks
- Always return `Task` or `Task<T>` from async methods

### Cancellation Tokens
- Pass `CancellationToken cancellationToken = default` on every async method
- Pass it down to every async call inside the method
- `OperationCanceledException` — catch, log at Info level, rethrow with `throw` (not `throw ex`)
- `throw` preserves stack trace. `throw ex` resets it.
- Manual `IsCancellationRequested` checks only needed in CPU-bound loops

**How cancellation actually works (network layer):**

When a client disconnects — closes the browser tab, drops WiFi, or explicitly cancels — the TCP connection closes. ASP.NET Core detects this TCP disconnect and signals `HttpContext.RequestAborted`, which is a `CancellationToken` automatically bound to your controller method parameter.

```
Client disconnects
  → TCP connection closes
    → ASP.NET Core detects disconnect
      → HttpContext.RequestAborted cancelled
        → CancellationToken in controller cancelled
          → Token passed to service cancelled
            → Token passed to repository cancelled
              → Npgsql sends cancellation to Postgres
                → DB query stops
```

ASP.NET Core automatically injects `HttpContext.RequestAborted` into controller `CancellationToken` parameters — no manual wiring needed.

**HTTP/2:** uses explicit `RST_STREAM` frame to signal cancellation. ASP.NET Core handles this identically.

**Dapper with CancellationToken** — pass via `CommandDefinition`, not directly:
```csharp
var command = new CommandDefinition(
    sql,
    new { Id = instrumentId },
    cancellationToken: cancellationToken
);
var result = await connection.QuerySingleOrDefaultAsync<Instrument>(command);
```

Without cancellation tokens — Postgres keeps running the query, returns results, server processes them, tries to write to a closed response stream. Wasted CPU, memory, and DB connections.

### DI Lifetimes
- `AddScoped` — new instance per HTTP request
- `AddTransient` — new instance per object creation
- `AddSingleton` — one shared instance for app lifetime
- **Captive dependency bug:** injecting Transient into Singleton makes it effectively Singleton

### IEnumerable vs ICollection vs IQueryable
- `IEnumerable` — in-memory, lazy evaluation, streaming
- `ICollection` — in-memory, count known, add/remove
- `IQueryable` — EF Core, translated to SQL, WHERE clause happens in DB

### null! Pattern
```csharp
public Instrument Instrument { get; set; } = null!;
```
For required navigation properties EF Core will always populate. Tells compiler "trust me, this won't be null at runtime."

### Structured Logging
```csharp
_logger.LogError(ex, "Failed for portfolio {PortfolioId}", portfolioId);
```
Named placeholders `{PortfolioId}` not string interpolation `$"..."`. Kibana/Serilog indexes these as queryable properties.

---

## Database Patterns

### Composite Index Column Order
`(instrument_id, trade_date)` better than `(trade_date, instrument_id)` when filtering on `instrument_id` first. First column = seek, second column = range scan.

### Partial Index
```sql
CREATE INDEX idx_symbology_current ON symbol_x_ref(symbol, symbology_id)
WHERE valid_to = '9999-12-31';
```
Only indexes current rows. Smaller, faster for the most common query pattern.

### N+1 Problem
```csharp
// Bad — 1 query + N queries
var instruments = db.Instruments.ToList();
foreach (var i in instruments)
    var price = db.Prices.Where(p => p.InstrumentId == i.Id).First();

// Good — 1 query with JOIN
var instruments = db.Instruments.Include(i => i.Prices).ToList();
```

### Dapper vs EF Core Split
- **Dapper** — raw SQL, fast reads, complex queries (search, filtered lists)
- **EF Core** — writes, simple reads, navigation property loading
- Repository interface hides which is used — Application layer doesn't care

### Atomic Status Transitions
```csharp
await using var transaction = await _dbContext.Database.BeginTransactionAsync();
// 1. Set ValidTo on old status row
// 2. Insert new status row
await transaction.CommitAsync();
```
Both operations succeed or both roll back. Never leave status history in half-updated state.

---

## Exception Handling & Response Envelope (built 20 June 2026)

### Why a uniform envelope at all
Without it, a successful response returns the raw DTO directly while an error response had its own ad-hoc shape — meaning a client (or future frontend) needs different parsing logic depending on success vs failure. The envelope makes the *outer* shape always identical; only the *inner* `Data` shape varies per endpoint, same as it always would.

```csharp
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsError { get; set; }
    public ErrorDetail? ErrorDetail { get; set; }

    public static ApiResponse<T> Success(T data) =>
        new() { Data = data, IsError = false };

    public static ApiResponse<T> Fail(ErrorDetail error) =>
        new() { Data = default, IsError = true, ErrorDetail = error };
}

public class ErrorDetail
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
}
```

**`Fail` returns `ApiResponse<T>`, not `ApiResponse<ErrorDetail>`** — a real bug caught during build. The generic type must match the rest of the class so `Fail` can be used in the same generic context as `Success`. `Data = default`, not `null` — `T` could theoretically be a value type, `default` is the type-agnostic safe choice.

**Status code lives on the HTTP response itself** (`context.Response.StatusCode`), never duplicated inside the JSON body — one source of truth, not two that could disagree.

**`Errors` is `Dictionary<string, List<string>>`, not a flat list or a single-value dictionary.** FluentValidation can report multiple failures on the *same* field (e.g. ShortCode failing both `NotEmpty` and the uppercase regex at once) — a `Dictionary<string, string>` would silently overwrite one failure with another. This is the same shape ASP.NET Core's own built-in `ValidationProblemDetails` uses, for the same reason.

```csharp
var errorDict = ex.Errors
    .GroupBy(x => x.PropertyName)
    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
```
`Select` was wrong here at first — `Select` transforms each element into a new sequence; it can't be used to populate a pre-built dictionary via side effects (`.Add(...)` returns nothing meaningful). `ToDictionary` exists specifically to convert a sequence directly into a dictionary — no manual loop, no pre-created empty dictionary needed.

### Exception hierarchy

**`NotFoundException` — lives in `Application/Exceptions`.**
```csharp
public abstract class NotFoundException : Exception
{
    public string EntityName { get; init; } = string.Empty;
    protected NotFoundException(string message) : base(message) { }
}

public class NotFoundException<TKey> : NotFoundException
{
    public TKey Id { get; init; }
    public NotFoundException(string entityName, TKey id)
        : base($"{entityName} with Id: {id} was not found")
    {
        EntityName = entityName;
        Id = id;
    }
}
```
**Why a non-generic base class exists at all:** you cannot write `catch (NotFoundException<TKey> ex)` in a middleware — `TKey` isn't a real type there, it's a placeholder that doesn't exist outside the generic class. The non-generic base lets the middleware catch the whole family (`NotFoundException<int>` for Vendor, `NotFoundException<Guid>` for Instrument later) with one `catch (NotFoundException ex)` block.

**Why this stays in `Application`, not `Core`:** the *decision* that "null means not found" is made in the service layer (`if (vendor == null) throw new NotFoundException<int>(...)`), never in `Infrastructure`. The repository just faithfully reports what the database returned, including nothing. Lowest layer that genuinely needs to throw it = Application.

**`ConflictException` — lives in `Core/Exceptions`,** a different home from `NotFoundException`, and the reasoning is the actual lesson here, not the conclusion alone:
```csharp
public class ConflictException : Exception
{
    public string ClientMessage { get; init; }
    public ConflictException(string clientMessage, Exception? innerException)
        : base("A conflict occurred while saving data. It may already exist or violate a business constraint.", innerException)
    {
        ClientMessage = clientMessage;
    }
}
```
**Why `Core`, not `Application` (where it first lived):** `CatalogueDbContext.SaveChangesAsync()` override in `Infrastructure` needs to throw this directly — that's the one place a unique-constraint violation is actually detected, since EF Core doesn't validate uniqueness in memory before talking to Postgres, only `SaveChangesAsync()`'s round trip to the database can discover it. `Infrastructure` is not allowed to depend on `Application` (arrows point inward only). `Core` is the only layer both `Application` and `Infrastructure` can see without violating the dependency rule. **Lesson:** the "lowest layer that genuinely needs it" test gives different answers depending on *which* layer actually throws the exception, not just on whether it's conceptually "an exception, so maybe Core." `NotFoundException` and `ConflictException` look like siblings but landed in different layers for a real, mechanical reason.

**No `innerException` default value — deliberate, not an oversight.** Every caller is forced to explicitly write `innerException: ex` or `innerException: null`, so a developer is forced to consciously decide whether one exists, rather than a default silently letting that question go unconsidered. Consistent with using `init` over `set` throughout this session — preferring "compiler forces a conscious decision" over "something convenient happens automatically."

**`Message` vs `ClientMessage` — two audiences, deliberately separated.** `Message` (inherited, via `base(...)`) is the fixed, generic, developer-facing description, logged in full internally, never shown to a client (could leak schema/internal detail). `ClientMessage` is written by hand at each throw site — short, safe, specific enough to act on ("A vendor with this short code already exists."), since "conflicts are not straightforward" and a hand-written message beats anything auto-derived from raw database error text.

**What was deliberately rejected for `ConflictException`, and why each rejection was correct:**
- *Parsing `pgEx.ConstraintName` to extract entity/field names* — rejected. String-splitting an index name assumes every index forever follows one exact naming convention; a composite index (`VendorInterface.(VendorId, Name)`) breaks the assumption silently. Worse: code *inside* exception-handling logic that can itself throw a *new*, unrelated exception (`IndexOutOfRangeException` from a malformed split) risks masking the original problem entirely — one of the nastiest bug categories to debug.
- *A proactive existence-check query before every write* — rejected on a deliberate, named performance tradeoff: "I am okay if client is ambiguous for an action like that but not slow down my db performance." A single round trip beats a more specific error message bought with a second query, for every write, forever.
- *Logging the raw `PostgresException` separately inside `CatalogueDbContext`* — rejected as genuine duplication. The middleware already logs the full exception (including any attached `InnerException`) once, at the top of the pipeline. Logging twice — once deep in the data layer, once in the middleware — makes logs noisier without adding information.
- *`EntityName` / `ConflictingField` as structured properties* — rejected. Once both the parsing path and the extra-query path were ruled out, nothing safe was left to reliably populate them. Kept the class lean rather than carrying two unused-in-practice properties "just in case."

### Middleware — final shape
```csharp
catch (ValidationException ex)       // 400, FluentValidation
catch (NotFoundException ex)         // 404, LogWarning — not LogError, a missing resource is not a system failure
catch (ConflictException ex)         // 409, reads ex.ClientMessage, never ex.Message
catch (Exception ex)                 // 500, generic message, full detail only in the log
```
Order matters: more specific exception types must be caught before the general `catch (Exception ex)` — C# evaluates catch blocks top to bottom and the first matching type wins.

### Known follow-ups, not yet fixed (tracked here so the separate bugfix commit doesn't lose them)
- `nameof(vendor)` / `nameof(vendorInterface)` (lowercase, local variable) vs `nameof(VendorInterface)` (capitalized, type name) used inconsistently across `VendorService` — cosmetic, produces inconsistent casing in `NotFoundException` messages, doesn't affect correctness.
- `UpdateVendorInterfaceAsync` receives `vendorId` from the route but never validates that the fetched `VendorInterface.VendorId` actually matches it — currently `PATCH /vendors/1/interfaces/5` and `PATCH /vendors/999/interfaces/5` behave identically as long as interface `5` exists. This check is needed specifically because `VendorInterface` is the only resource so far with a parent-child route shape (`{vendorId}/interfaces/{id}`) — `Vendor` and `Symbology` are top-level resources with only one id in their route, so this specific gap cannot occur for them. **This exact pattern will recur for Instrument's nested resources (symbols, status history) and should be applied deliberately each time, not assumed away.**

---

## Timeline — Personal Deadlines to Finish Project 1 (Catalogue API)

Last updated: 20 June 2026. This table exists so "am I on track" is a 10-second check against a date, not a feeling. Mark each row done with the actual date you finished it — if a date slips, don't delete it, just write the real date next to it so drift is visible and honest, not hidden.

**Target: Catalogue API fully built, tested, and deployed live on AWS by 23 July 2026.**

| # | Deliverable | Deadline | Done on |
|---|---|---|---|
| 1 | `ITimeStampAudit` applied to Vendor/Symbology/VendorInterface mappers | 20 Jun | 20 Jun — applied across every Core model with timestamps (Vendor, Symbology, VendorInterface, Instrument, InstrumentStatusHistory, EtfRefData, VendorInterfaceSymbolXRef), broader than originally scoped |
| 2 | FluentValidation + GlobalExceptionMiddleware verified end-to-end in Swagger | 20 Jun | Middleware rebuilt 20 Jun (see Exception Handling & Response Envelope section below); live Swagger verification still pending, planned for tonight 20 Jun |
| 3 | Standard API response shape decided and applied | 20 Jun | 20 Jun — `ApiResponse<T>` envelope built and retrofitted into every existing controller endpoint |
| 4 | VendorInterface update endpoint built | 20 Jun | 20 Jun |
| 5 | Symbology GetById + update endpoints built | 22 Jun | 20 Jun — done a day early |
| 6 | Full manual test pass — every existing endpoint verified through Swagger | 23 Jun | 20 Jun — Symbology and VendorInterface fully tested live; Vendor's GET/Create/Update still untested (see below). Found and fixed 3 real bugs (see "Live Swagger testing pass" entry below); 1 known issue (Symbology GetAll has no ORDER BY) still open. |
| 7 | Instrument create-transaction design session completed (Instrument + ref data + SymbolXRef, all-or-nothing) | 25 Jun | 20 Jun — done during a commute conversation, 5 days early. See full design in "Instrument creation design session" entry below. |
| 8 | CreateInstrumentAsync built and working | 28 Jun | 28 Jun — on the deadline, but the path there ran longer than planned: rows 7 onward (GUID architecture reversal, factory pattern build-then-removal, the explicit transaction design) all landed in this same stretch rather than being separable, single-day tasks the way the original row-by-row dates implied. |
| 9 | GetByIdAsync + GetAllAsync with `InstrumentFilter` built | 1 Jul | 28 Jun — both built 3 days early. Honest caveat: "built" here means written and lightly smoke-tested, not thoroughly verified — see "Built, needs a real verification pass" in What's Next above. Don't let the early date read as "fully proven," only "code exists and runs without throwing." |
| 10 | Resolve endpoint built with Redis caching, cursor pagination on GetAll implemented | 6 Jul | Cursor pagination on GetAll: done, 28 Jun, ahead of schedule. Resolve endpoint + Redis caching: **not started** — this row was originally one combined deadline for two genuinely separate pieces of work; splitting it going forward (see row 10a/10b below) rather than marking the whole row done when only half of it is. |
| 10a | Resolve endpoint built (no caching yet) | 3 Jul | |
| 10b | Redis caching added to Resolve | 6 Jul | |
| 11 | Basic xUnit + Moq test project covering Vendor/Symbology services | 8 Jul | |
| 12 | Instrument search endpoint + status history endpoint | 12 Jul | |
| 13 | Swagger docs polished, README with architecture diagram written | 14 Jul | |
| 14 | API Dockerized, runs correctly in a container locally | 17 Jul | |
| 15 | Deployed to AWS — RDS for Postgres, container hosting live | 21 Jul | |
| 16 | **Live URL confirmed working end-to-end from outside your machine** | **23 Jul** | |

**Honest note on drift:** rows 1–6 were originally spread across 20–23 June, but a multi-day illness pushed real progress on them to 20 June, where most landed in a single long session. Rows 7 onward have not been re-dated yet — decide after tonight's testing pass whether the original 25 Jun start for Instrument design still holds or needs to shift a few days later to absorb the lost time honestly, rather than pretending the illness didn't cost anything.

**If you are reading this on any given day and a row's deadline has already passed without a "Done on" date — that's the honest signal you're behind, not a vague feeling. Use it to decide whether to compress the next few rows or extend the final date, but don't quietly ignore it.**

---

## Timeline — Project 2: Frontend (React + Auth + RBAC)

Starts only after Project 1 row 16 above has a real "Done on" date — not folded into Project 1's deadline, so finishing the backend stays an honest, unmoved milestone regardless of how frontend goes. Frontend is the least predictable phase of either project, since it's a genuinely new skill area with zero prior experience, unlike everything in Project 1 which deepened existing strengths — so these dates are looser by design, and slipping here says nothing about Project 1's discipline holding.

**Target: full-stack app with working auth and role-based access, deployed, by 13 August 2026.**

| # | Deliverable | Deadline | Done on |
|---|---|---|---|
| 1 | React + Tailwind fundamentals — comfortable with components, state, basic routing | 28 Jul | |
| 2 | Login page wired to a JWT-issuing endpoint on the backend | 30 Jul | |
| 3 | Role model decided and enforced server-side (admin / trader / viewer) — not just hidden UI buttons | 1 Aug | |
| 4 | Instrument list screen calling the real GetAllAsync/filter endpoint | 4 Aug | |
| 5 | Create + update instrument forms wired to the real backend, respecting role permissions | 7 Aug | |
| 6 | Role-based UI fully working end-to-end — admin sees create/update, viewer sees read-only | 9 Aug | |
| 7 | Frontend deployed (static hosting or container) and pointed at the live backend | 11 Aug | |
| 8 | **Full-stack app confirmed working end-to-end from a fresh browser, no local setup** | **13 Aug** | |

If row 1 alone — the React fundamentals — is still incomplete past 28 Jul, that's the single most useful early warning in this whole table, since everything below it depends on that foundation being genuinely solid first, the same way Instrument depended on the cleanup list being genuinely done first.

---

## Request Flow — Navigation Aid

Written 20 Jun, after a session where the sheer number of files (DTOs, validators, mappers, exceptions, services, repositories, controllers) started feeling hard to navigate, despite every individual piece being well understood. The fix wasn't simplifying the architecture — the layering is correct and earned every boundary it has tonight. The fix is writing the *path* through the layers down once, so it doesn't need to be re-derived from scratch every time.

**The full path for a typical write — e.g. `POST /api/v1/vendors`:**

1. **Controller** receives the request, calls `ValidateAndThrowAsync` against the matching `IValidator<T>`
2. **Validator** (FluentValidation) checks the request — throws `ValidationException` if invalid, caught later by the middleware
3. **Controller** calls the **Service**
4. **Service** calls the **Mapper** to convert the request DTO into a Core domain model
5. **Service** calls the **Repository** to persist it
6. **Repository** saves via EF Core (`AddAsync`/`Update` + `SaveChangesAsync`) — `CatalogueDbContext`'s override may throw `ConflictException` here if a unique constraint is violated
7. **Service** calls the **Mapper** again, this time domain model → response DTO
8. **Controller** wraps the response DTO in `ApiResponse<T>.Success(...)` and returns it
9. If any exception was thrown anywhere in steps 2–6, **`GlobalExceptionMiddleware`** catches it, builds the matching `ApiResponse<T>.Fail(...)`, and that's what the client actually receives instead

**The same path, for a read with a possible "not found" — e.g. `GET /api/v1/vendors/{id}`:**

1. **Controller** calls the **Service** directly (no validator needed — a GET has no body to validate)
2. **Service** calls the **Repository**, which runs a Dapper query, returns `null` if no row matches
3. **Service** checks for `null` and throws `NotFoundException<TKey>` if so — this is the one decision point only the Service layer makes, never the Repository
4. If found, **Service** calls the **Mapper** to convert domain model → response DTO
5. **Controller** wraps it in `ApiResponse<T>.Success(...)`
6. If `NotFoundException` was thrown in step 3, **`GlobalExceptionMiddleware`** catches it and returns `ApiResponse<T>.Fail(...)` with a 404 instead

**Reuse this template for every new feature** (Instrument, starting tomorrow) rather than re-deriving the path from scratch — the layer order never changes, only which specific exception might fire at which step.

---

## What's Next — immediate, in order

**Genuinely done, proven by live testing, not just on paper:**
- Create — full flow, all layers, tested live multiple times, one real data-integrity bug found and fixed (duplicate ISIN constraint) via that testing.
- GUID architecture — `Instrument`/`SymbolXRef`/`InstrumentStatusHistory`/`VendorInterfaceSymbolXRef` all generate ids in C#; the "empty migration" question is fully resolved (nothing was needed at the DB level, only the C#-side fix, already complete).
- `IRefDataMapper` factory pattern — removed entirely, replaced with a simpler if-chain. Don't reintroduce it without a concrete, demonstrated reason the if-chain isn't sufficient.

**Built, needs a real verification pass before trusting it (next session's first priority):**
1. **Thoroughly test `GetByIdAsync`** — ran once without throwing, but the VendorInterface/Symbology nesting and the multi-query stitching logic haven't been deliberately exercised against varied real data yet (an instrument with 0 symbols, multiple symbols, multiple vendor interfaces per symbol).
2. **Thoroughly test `GetAllAsync` with `InstrumentFilter` + cursor pagination** — built this session, base filter conditions + all 3 nested type filters + cursor logic all written, but only spot-checked, not run through a real multi-page scenario yet. Specifically verify: cursor round-trips correctly across at least 2 pages with real data; each Bond/Equity/ETF filter field actually narrows results correctly; `Status` defaults to Active correctly when unspecified.

**Known, deliberate, explicitly-scoped-out gaps — not bugs, don't "fix" without a real reason to revisit:**
- Ref-data (`BondRefData`/`EquityRefData`/`EtfRefData`) has no vendor provenance and can't hold multiple vendors' conflicting values for the same field — see the full writeup above for why this was designed in detail and then deliberately not built, given the actual goal (portfolio project, not a production system).
- `GetAll` deliberately excludes vendor/interface filtering and combined name+symbol search — both belong to the already-designed `Search` endpoint instead, not duplicated here.

**Not started:**
3. `ResolveSymbolAsync` — the most-called endpoint conceptually (Holdings Engine/OMS would hit this on every trade event), sub-50ms target, Redis caching belongs here specifically.
4. `Search` endpoint — the combined name+symbol fuzzy search design (`InstrumentSearchResult`) already exists on paper from weeks ago, not yet built.
5. Redis caching — deferred specifically to the resolve endpoint, not GetAll/GetById.
6. Basic xUnit + Moq test project covering Vendor/Symbology services (still hasn't been started at all, despite being on this list for a while).
7. Dockerize + AWS deploy.
8. Frontend phase (separate project, see Project 2 timeline above).

**For a fresh session picking this up:** read the "Week 2 — Migration resolved, IRefDataMapper removed, GetById built, GetAll + cursor pagination built" entry in the Session Decisions Log above in full before writing any new code — it contains the real reasoning behind several non-obvious decisions (the compound cursor, the partial unique index on symbols, the deliberate ref-data provenance gap) that would otherwise need to be re-derived from scratch.

---



### Entity Configuration
One configuration class per entity. Lives in `Infrastructure/Configuration/`. Auto-discovered via `ApplyConfigurationsFromAssembly`.

```csharp
public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("vendors");
        builder.HasKey(v => v.VendorId);
        builder.Property(v => v.VendorId).ValueGeneratedOnAdd();
    }
}
```

### Key decisions in configurations
- Enums stored as strings — `builder.Property(x => x.Type).HasConversion<string>()`
- Sentinel date default — `builder.Property(x => x.ValidTo).HasDefaultValueSql("'9999-12-31'::date")`
- Shared PK — `builder.HasKey(e => e.InstrumentId)` with no `ValueGeneratedOnAdd`
- Decimal precision — `builder.Property(x => x.Amount).HasPrecision(18, 4)`
- Snake case — `optionsBuilder.UseSnakeCaseNamingConvention()` in DbContext
- Configure relationship from ONE side only — both sides causes duplicate FK conflicts

### EF Core CRUD operations
```csharp
// Read
await _context.Set<T>().FindAsync(new object[] { id }, ct);        // by PK, checks cache
await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id, ct); // any condition

// Write
await _context.AddAsync(entity, ct);   // marks as Added — no DB yet
_context.Update(entity);               // marks as Modified — synchronous, no IO
_context.Remove(entity);               // marks as Deleted — synchronous, no IO
await _context.SaveChangesAsync(ct);   // generates SQL and hits DB — async

// After SaveChangesAsync — PK is populated on entity
return entity.VendorId;
```

### Why Update() is synchronous
Change tracking is in-memory — no IO involved. Only `SaveChangesAsync` hits the database.

### FindAsync vs FirstOrDefaultAsync
- `FindAsync` — PK only, checks change tracker cache first, faster
- `FirstOrDefaultAsync` — any condition, always hits DB, more flexible

### Set<T>() vs named DbSet
- `Set<T>()` — use in generic classes where T is a type parameter
- `_context.Vendors` — use in specific repositories, more readable
- Both are equivalent

### Generic Repository — when to use and when not to
**Use when:** pure CRUD, all entities share same operations, early stage project.
**Do not use when:** domain-specific query needs, complex filtering, different PKs per entity.
**In this project:** specific interfaces because each has unique methods that do not fit a generic contract.

---

## Session Decisions Log

A running log of key decisions made each session. Check this before starting a new session to avoid re-debating settled decisions.

---

### Week 1 — Domain design

**Models:**
- Extension tables use shared PK pattern — InstrumentId is both PK and FK
- No navigation property from extension tables back to Instrument
- InstrumentType stored as string in DB — not integer enum
- ValidTo = 9999-12-31 sentinel date — not NULL
- LastUpdatedAtUtc — application sets this, no DB default
- CreatedAtUtc — HasDefaultValueSql("now()") as safety net

**Symbology:**
- SymbolXRef is the hub — maps symbol to instrument via symbology type and vendor interface
- VendorInterfaceSymbolXRef — many-to-many junction between symbol and vendor interfaces
- IsPrimary nullable bool — not all symbology types need a primary designation
- Composite unique constraint on (InstrumentId, SymbologyId, ValidFrom)

**Enums:**
- BondType — Government, Corporate, Municipal, Agency (categories only)
- BondStructure — Standard, ZeroCoupon, Convertible, Callable, Puttable (features only)
- Kept separate — a ZeroCoupon bond can be Government or Corporate

**Market data:**
- Out of scope for v1 — dirty price, last price, NAV are market data not reference data

---

### Week 1 — API design

**Endpoints:**
- Single GET /instruments with nested filter object — not separate endpoints per type
- GET /instruments/resolve — symbol to instrument lookup, Redis cached
- GET /instruments/search — fuzzy search across name and symbol, COALESCE display symbol
- GET /instruments/snapshot — bulk export, IEnumerable for streaming
- PATCH not PUT for updates
- PATCH /instruments/{id}/status — separate from update, writes to status history atomically

**Search behaviour:**
- Searches name AND symbol_x_ref.symbol via LEFT JOIN
- Display symbol priority: matched symbol, primary symbol, any symbol, instrument name
- MatchedOnSymbol boolean — tells UI which field to highlight
- Deduplication via GROUP BY

**Pagination:**
- Cursor-based only — no offset, no total count, no page numbers
- PagedResult<T> has two fields only: Items and NextCursor

---

### Week 1 — EF Core configurations

**Relationships — configure from one side only:**
- Instrument to EquityRefData — InstrumentConfiguration
- Instrument to SymbolXRef — InstrumentConfiguration
- Vendor to VendorInterface — VendorInterfaceConfiguration
- SymbolXRef to VendorInterfaceSymbolXRef — SymbolXRefConfiguration

**Indexes:**
- Instrument — index on Type only for v1
- SymbolXRef — partial index on Symbol WHERE valid_to = 9999-12-31
- InstrumentStatusHistory — partial index on InstrumentId WHERE valid_to = 9999-12-31
- Symbology.TypeCode — unique index
- Vendor.Name and Vendor.ShortCode — unique indexes
- VendorInterface.(VendorId, Name) — composite unique index

**DbSets:**
- Extension tables NOT added as DbSets — accessed via Instrument navigation
- InstrumentStatusHistory IS a DbSet — queried directly

---

### Week 1 (cont.) — Exception handling & response envelope (20 Jun)

- `ApiResponse<T>` envelope with `Success`/`Fail` static factories — applied to every existing endpoint, not deferred to a later retrofit pass
- `ErrorDetail.Errors` is `Dictionary<string, List<string>>`, grouped by field — chosen specifically because FluentValidation can report multiple failures on the same field at once
- Status code lives only on the HTTP response, never duplicated inside the JSON body
- `NotFoundException` (abstract base + generic `NotFoundException<TKey>`) stays in `Application/Exceptions` — only the service layer decides "null means not found"
- `ConflictException` moved to `Core/Exceptions` — `Infrastructure`'s `CatalogueDbContext.SaveChangesAsync()` override needs to throw it directly, and `Infrastructure` cannot depend on `Application`
- `ConflictException` carries `ClientMessage` (hand-written per throw site) separate from `Message` (generic, base, logged in full) — deliberately no auto-derived detail from raw Postgres error text
- Rejected: parsing `pgEx.ConstraintName`/`ColumnName` to auto-populate entity/field (too fragile, risks throwing from inside exception-handling code itself); a proactive existence-check query before every write (explicit performance tradeoff — one round trip beats a more specific message); logging the raw Postgres exception separately in the DbContext (duplicates what the middleware already logs once)
- `ConflictException`'s `innerException` parameter has no default — every caller must explicitly pass `ex` or `null`, consistent with preferring `init` over `set` throughout this session
- `NotFoundException` logged via `LogWarning`, not `LogError` — a missing resource is an expected outcome, not a system failure
- Identified, not yet fixed: `VendorInterface` update doesn't verify the route's `vendorId` actually matches the fetched interface's `VendorId` — only resource so far with a parent-child route shape, so only place this specific gap can occur; will recur for Instrument's nested resources and needs deliberate handling each time, not assumed away

---

### Week 1 (cont.) — Instrument creation design session (20 Jun, commute conversation, no code yet)

**`CreateInstrumentRequest` shape — settled:**
```csharp
public class CreateInstrumentRequest
{
    public string Name { get; set; } = string.Empty;
    public InstrumentType Type { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public CreateBondRefDataRequest? BondDetails { get; set; }
    public CreateEquityRefDataRequest? EquityDetails { get; set; }
    public CreateEtfRefDataRequest? EtfDetails { get; set; }

    public int VendorInterfaceId { get; set; }
    public ICollection<CreateInstrumentSymbolRequest> Symbols { get; set; } = new List<CreateInstrumentSymbolRequest>();
}

public class CreateInstrumentSymbolRequest
{
    public string SymbologyTypeCode { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool? IsPrimary { get; set; }
}
```

**One nested request, not split across multiple calls.** Splitting creation across requests recreates the exact partial-state risk a transaction exists to prevent — you'd be manually doing, across multiple non-atomic round trips, what one transaction does safely in one.

**Nested ref-data block per type, not a flat object with unused-null fields.** Comparing the two real JSON shapes side by side (not reasoning abstractly) made this obvious: a flat shape leaves every Equity-creation request carrying unused Bond/ETF nulls, and nothing stops a caller sending two contradictory detail blocks at once. Nesting groups each type's fields explicitly.

**Three nullable ref-data properties + a FluentValidation `.Must()` rule, NOT an inheritance hierarchy (`CreateBondInstrumentRequest : CreateInstrumentRequest`, etc).** The inheritance version is more strict at the type level, but requires a custom `JsonConverter`/discriminator to deserialize correctly, plus extra Swagger configuration for per-type docs — real, ongoing infrastructure cost solving a problem a five-line validation rule already solves at request-validation time. **Lesson, stated by the user and worth keeping verbatim:** "Inheritance should be used where there is a direct is-a relationship. Our case did not. We should separate deserialization concerns from validation concerns." Deserialization's job is turn bytes into an object; validation's job is verify the object makes business sense — don't make one do the other's job.
```csharp
RuleFor(r => r)
    .Must(r => r.Type switch
    {
        InstrumentType.Bond   => r.BondDetails != null && r.EquityDetails == null && r.EtfDetails == null,
        InstrumentType.Equity => r.EquityDetails != null && r.BondDetails == null && r.EtfDetails == null,
        InstrumentType.ETF    => r.EtfDetails != null && r.BondDetails == null && r.EquityDetails == null,
        _ => false
    })
    .WithMessage("Exactly one ref-data block must be provided, and it must match the declared Type.");
```

**Symbology and VendorInterface are assumed to already exist — Instrument creation only reads/validates them, never creates them.** Matches how these tables actually behave (small, curated, seeded/managed separately, not growing one-for-one with instruments). Both must be validated (existence checked) *before* the write starts; invalid `symbologyTypeCode` or `vendorInterfaceId` → `NotFoundException`, request fails loudly, nothing partially created.

**Multiple symbols allowed per create request — "at least one, but any number more," not "exactly one."** Realistic: a single vendor often reports both ISIN and CUSIP for the same instrument in the same feed at the same moment. FluentValidation rule needed: `Symbols` collection must be non-empty.

**Exactly one vendor per create request — `VendorInterfaceId` lives at the top level, not per-symbol.** Different vendors send genuinely different shaped data; a single request representing two vendors' views at once recreates the same structural awkwardness as the Bond/Equity/ETF problem, one layer down. Matches the real ingestion shape too — one upstream process per vendor feed, each only ever knowing its own vendor's data at call time. If two vendors independently report the same brand-new instrument, that's two separate calls: the first genuinely creates it, the second uses the enrichment endpoint (below), not a second create.

**Create endpoint owns "the first of everything." Smaller, already-planned endpoints own "everything after the first."**
`POST /api/v1/instruments` — creates Instrument + matching ref-data row + first symbol(s) + first vendor mapping(s) + initial `InstrumentStatusHistory` row, all atomically, because an instrument with zero of any of these is exactly the kind of incomplete, unusable row this project has avoided since the very first symbology design session ("instrument is nothing without a symbol").
`POST /instruments/{id}/symbols` (already in the original endpoint map) — adds a *second, third, etc.* symbol to an instrument that already exists. Never touches instrument-creation logic.
A future, smaller endpoint for "attach a new vendor's coverage to an existing symbol" similarly handles the "Refinitiv starts reporting an ISIN Bloomberg already established" case — not the create endpoint's job.

**`InstrumentStatusHistory`'s first row is NOT part of the request — the service writes it silently, automatically, as a consequence of creation.** Nothing for a caller to send (Status=Active, ValidFrom=today, ValidTo=sentinel are all derivable). But the row still must be written, in the same transaction — without it, the sentinel-date "current status" query (`WHERE valid_to = '9999-12-31'`) returns nothing for a brand-new instrument, recreating the same incomplete-row problem as zero symbols.

**GUID generation for `InstrumentId`: database generates it via existing `gen_random_uuid()` default — same as every other table. No exception for Instrument.**
Real back-and-forth worth keeping, because the final answer is better than the first-offered one: initially considered generating the `Guid` in C# (`Guid.NewGuid()`) before any DB call, specifically to avoid two round trips (insert Instrument, read back the id, then insert dependents) and avoid needing an explicit transaction at all — `SaveChangesAsync()`'s implicit transaction would have been enough. **Rejected**, for two reasons, both raised by the user, neither prompted: (1) GUID-fragmentation tradeoff (random v4 GUIDs as PKs cause index fragmentation at scale, discussed back in Week 1's PK-type decisions) would be silently introduced for one table only, while every other table keeps the DB-generated default — inconsistent, and a real reviewer would correctly ask why. (2) **The actual deciding factor:** this system is read-heavy, write-light (Holdings Engine/OMS hit `GetById`/`Resolve`/`Search` constantly; `CreateInstrumentAsync` fires rarely). The round-trip savings being traded away were never going to add up to anything measurable given real traffic shape — there was nothing real to gain by breaking consistency. GUID collision across machines, raised as a separate distributed-systems concern, is a genuine non-issue at any realistic scale (122 random bits in a v4 GUID — collision probability requires ~2.7 billion GUIDs/sec sustained for ~700 years to reach 50% odds) — not a tradeoff, a settled fact.
**Mechanical consequence:** Instrument creation needs an *explicit* transaction (`BeginTransactionAsync()`/`CommitAsync()`), not just `SaveChangesAsync()`'s implicit one — insert Instrument first, read back the generated `InstrumentId`, then construct and insert ref-data/symbols/history using that id. Same explicit pattern already documented for `InstrumentStatusHistory` status transitions, just spanning more tables.

**Still open, deliberately deferred to the actual build session, not decided yet:** exact validation order (Symbology checked before or after VendorInterface; what happens if one passes and the other fails); exact write order inside the transaction beyond "Instrument first" (ref-data vs symbols vs history ordering); whether the parent-child id-match lesson from `VendorInterface` applies to any of Instrument's own nested writes within this same transaction.

---

### Week 1 (cont.) — Live Swagger testing pass, real bugs found (20 Jun, night)

First time the envelope, both exceptions, and the full validation chain were exercised against a real running API rather than just read in code. Roughly 1 hour, not the planned 30 minutes — worth it.

**Confirmed working correctly, no changes needed:**
- `ApiResponse<T>` envelope shape correct on both success and failure across Symbology Get/GetAll/Create/Update
- `ValidationException` → 400 with field-grouped errors — verified on Create and Update for both Symbology and VendorInterface
- `NotFoundException` → 404 — verified on Symbology GetById and Update with a non-existent id
- `ConflictException` → 409 — verified on Symbology Create (duplicate TypeCode) and VendorInterface Create (duplicate VendorId+Name)
- `GetVendorInterfaceByIdAsync`'s two-parameter repository overload (`WHERE vendor_id = @vendor_id AND vendor_interface_id = @vendor_interface_id`) already correctly scopes lookups by parent — the earlier-logged "VendorInterface update doesn't verify vendorId match" concern turned out to be **already solved at the SQL level** for both Get and Update; only the *exception message* was misleading, not the logic (see below)

**Real bug found and fixed — Symbology GetAll has no `ORDER BY`.**
No explicit ordering on `SELECT ... FROM symbologies` means Postgres returns rows in physical/insert order, not a guaranteed or predictable order — observed as id 1 appearing last, after ids 2–8, because it was seeded separately from the rest. Not random, not a sort bug exactly, but a real fragility: relying on undefined order means a future vacuum, reindex, or more inserts could silently change the order with no warning. **Not yet fixed in code** — needs an explicit `ORDER BY` added (decide alphabetical-by-TypeCode vs by-SymbologyId before fixing).

**Real bug found and fixed — `CreateVendorInterfaceAsync` with a non-existent `VendorId` threw an unhandled exception.**
Inserting a `VendorInterface` with `VendorId = 100` (no such vendor) hit the foreign key constraint at the database level — a different SQL state (`23503`, FK violation) than the unique-violation catch (`23505`) already handles, so it fell through to the generic 500 catch-all. Considered, then rejected, catching `23503` in `CatalogueDbContext.SaveChangesAsync()` the same way as the unique violation — rejected specifically because, unlike `ConflictException`, a database-level FK-violation catch genuinely cannot say anything more useful than "referenced resource not found," and a vague message defeats the purpose of having `NotFoundException` at all. **Fixed instead with a proactive check in `VendorService.CreateVendorInterfaceAsync`** — `GetVendorByIdAsync` first, throw `NotFoundException<int>` immediately if null, before ever constructing or saving the `VendorInterface`. Accepted the extra round trip deliberately: unlike the Vendor-ShortCode-conflict tradeoff (round trip rejected, ambiguity accepted), here skipping the check produces a genuinely worse outcome (unhandled 500) for a check that's nearly free (indexed PK lookup on a tiny table) — the two situations only looked similar on the surface.

**Real bug found and fixed — `NotFoundException` message for VendorInterface was misleading, even though the underlying logic was already correct.**
`GetVendorInterfaceByIdAsync(vendorId=1, vendorInterfaceId=4)` where interface 4 exists but belongs to vendor 7: the SQL correctly returns no row (scoped by both ids), correctly throws `NotFoundException`, but the auto-generated message said `"VendorInterface with Id: 4 was not found"` — true in the narrow sense, but misleading, since interface 4 *does* exist, just not for vendor 1. **Fixed by adding an optional `message` override to `NotFoundException<TKey>`'s constructor**, defaulting to `null` so every existing call site is unaffected:
```csharp
public NotFoundException(string entityName, TKey id, string? message = null)
    : base(message ?? $"{entityName} with Id: {id} was not found")
```
Rejected adding more structured parameters (e.g. a `parentEntity`/`parentId` pair) — same reasoning as keeping `ConflictException` lean: don't grow a class's constructor to anticipate every future phrasing need when an optional override handles the one real case that needs it. Applied at both `GetVendorInterfaceByIdAsync` and `UpdateVendorInterfaceAsync`: `$"{nameof(VendorInterface)} with Id: {vendorInterfaceId} was not found for {nameof(Vendor)} with Id: {vendorId}."` — caught and fixed a real copy-paste slip mid-fix (`nameof(Vendor)` used where `nameof(VendorInterface)` was needed) before it shipped.

**Observed, considered, deliberately left as-is — empty-body PATCH succeeds as a no-op.**
Sending `UpdateSymbologyRequest` / `UpdateVendorRequest` with every field null/omitted still returns 200, with no actual column changes, since every "if provided" check evaluates false. Considered making this a validation error ("at least one field must be provided") — rejected: this isn't a malformed or contradictory request, it's a harmless no-op, the same category as calling a light switch's "set" function with the light's current state. Real production APIs commonly allow this. Worth revisiting only if `LastUpdatedAtUtc` ever becomes load-bearing for something like a "recently changed" view, where a no-op silently bumping it would be misleading — currently purely informational, so not a concern yet.

---

### Week 1 (cont.) — Factory pattern, GUID architecture reversal, Create flow assembled end-to-end (code)

**Factory pattern for ref-data mapping — built, registered, used.**
`IRefDataMapper` interface (one method, `Map(CreateInstrumentRequest) → IInstrumentRefData`), three implementing classes (`BondRefDataMapper`, `EquityRefDataMapper`, `EtfRefDataMapper`), registered as a `Dictionary<InstrumentType, IRefDataMapper>` via `AddSingleton` in `Program.cs` — correct lifetime, confirmed by checking statelessness: each mapper only reads its input and constructs a new object, no shared mutable state, unlike repositories (`Scoped`, because they hold a per-request `DbContext`).
`IInstrumentRefData` — deliberately empty marker interface. `BondRefData`/`EquityRefData`/`EtfRefData` share zero real members (no common field), so the interface exists purely so the factory has *something* concrete to return and so callers know what kind of object they're holding, not to express shared behaviour. Confirmed this is still strictly better than returning `object` (same eventual cast required either way, but the named interface documents intent) even though it's "just a marker."
Rejected the validator's `.Must()` switch-expression style as "violating OCP" — re-examined and kept the switch, because `InstrumentType` is a deliberately closed, rarely-extended enum (3 fundamental asset classes), and OCP's cost (more indirection) only pays for itself against genuinely open-ended extension points, not closed sets where adding a case is a rare, deliberate domain event anyway.

**GUID generation — full reversal, with the reasoning kept since the earlier decision was correct *given what was known then*.**
Originally decided (documented further up): every table, including `Instrument`, lets the database generate its `Guid` via `gen_random_uuid()`, specifically *for consistency* with every other table, after rejecting a C#-generated id as introducing an unjustified one-off inconsistency.
**What changed:** building the actual `CreateInstrumentAsync` repository method surfaced a real, concrete cost the earlier design session didn't anticipate — `_dbContext.AddAsync(instrument, ...)` pulls in the *entire* attached object graph (ref-data, symbols) in one call, but `BondRefData.InstrumentId` couldn't be set yet because `Instrument.InstrumentId` didn't exist until *after* a first save — forcing a detach/reattach, two-step save dance that was, in the user's words, something that "sucks" and isn't real production practice for this *specific* shape (parent with multiple dependent children needing the same generated id, all created atomically together).
**Resolution:** `Instrument.InstrumentId` (and, per the user's broader consistency call, every other Guid-keyed table — `SymbolXRef`, `InstrumentStatusHistory`, `VendorInterfaceSymbolXRef`) now defaults to `Guid.NewGuid()` in the C# model itself, with `.ValueGeneratedNever()` added to each entity's EF Core configuration. A narrower test was offered first ("only `Instrument` and `SymbolXRef` actually need this, since nothing depends on the other two tables' ids") and deliberately overridden in favour of the broader, uniform rule — explicit tradeoff: simpler mental model ("all Guid tables work the same way, always") accepted over "minimal, justified-only-where-strictly-needed" change.
**Consequence for `InstrumentMapper`:** zero changes needed. `ToDomain` was already correctly attaching everything via navigation properties; it just couldn't *work* until the parent's id existed upfront. The mapper wasn't wrong, the model was incomplete for what the mapper was already trying to do.
**Consequence for `InstrumentRepository.CreateAsync`:** the two-step save / detach-reattach pattern is gone. Now: one `AddAsync` + `SaveChangesAsync()` for the whole `Instrument` graph (ref-data, symbols, status history all attached via navigation properties, all correctly get their FK set automatically by EF Core's change tracker once the parent id is known upfront), then a second `AddRangeAsync` + `SaveChangesAsync()` for `VendorInterfaceSymbolXRef` rows (still can't be attached via navigation property — no such navigation exists on `Instrument`, the relationship genuinely belongs to the symbol-to-vendor-interface link, not the instrument). Both saves kept inside one explicit `BeginTransactionAsync()`/`CommitAsync()` — confirmed this still matters: rollback genuinely undoes an *already-returned-successfully* first `SaveChangesAsync()` as long as both calls sit inside the same uncommitted transaction, directly addressing the user's concern about a partial failure (e.g. bad `vendorInterfaceId`) leaving an orphaned `Instrument` row that would then hit a unique-constraint violation on retry.

**Pending migration consequence (not yet run):** removing the database-level `gen_random_uuid()` defaults requires a new EF Core migration before any of this can actually execute against the real Postgres schema — tracked in What's Next above as the first item of the next session.

**`InstrumentStatusHistory`'s initial row — stayed in the mapper, not moved to the service, after real back-and-forth.**
Considered moving it on the basis of "mapper transforms already-known data, service drives business logic" — examined honestly: by that rule, more than just status history would need to move (the ref-data type-switch and symbol-building arguably "decide" things too), so applying it consistently would be a much bigger restructuring than intended. Settled, based on common real-world .NET practice rather than an invented rule: mappers stay structural/stable (a Bond *has* BondRefData is a structural fact, unlikely to ever need independent testing), while genuinely independently-variable business rules (what status a *new* instrument starts with — could plausibly differ for e.g. a future bulk-import path) belong in the service. Net effect: leave ref-data/symbol mapping in `InstrumentMapper` as-is; move status-history construction to `InstrumentService` next session. Not done yet — tracked in What's Next.

**`SymbologyRepository`'s batched lookup — `ANY(@type_codes)`, not one query per symbol.**
A single create request can carry multiple symbols (e.g. ISIN + CUSIP from the same vendor at once — confirmed realistic, not hypothetical). `WHERE type_code = ANY(@codes)` checks one value against every element of an array in one round trip, equivalent to chaining `OR` per code but scaling to any count without writing one. Service compares `requested.Count` vs `found.Count`; if they differ, `Except()` finds exactly which codes are missing, named explicitly in the `NotFoundException` message — **all-or-nothing**, deliberately: explicitly decided (after weighing it) that silently creating an instrument with only the *valid* symbols, dropping the invalid one, would recreate exactly the kind of silent partial state ("instrument is nothing without a symbol") this project has avoided everywhere else. One invalid code fails the entire create, before any database write happens.

**`VendorInterface` resolved by name pair, not id — a real, caught inconsistency, not a late add-on.**
`CreateInstrumentRequest.VendorInterfaceId` (int) was the original design. Caught mid-build: upstream market data vendors don't know internal database ids — exactly the same reasoning already applied to `SymbologyTypeCode` being a string, just missed for VendorInterface the first time. Fixed to `VendorName` + `InterfaceName` (two separate fields, matching the two separate real columns — rejected a single combined string like `"Bloomberg/B-PIPE"` as needing fragile splitting). Required a new repository method, `GetVendorInterfaceByNamesAsync`, joining `vendors` and `vendor_interfaces` on both name fields — caught and fixed a real bug while writing it: `QuerySingleAsync` (throws on zero rows) used where `QuerySingleOrDefaultAsync` (returns null) was needed, matching the nullable return type's actual contract; also added table aliases on every column to avoid ambiguous-column errors once the JOIN brought two tables with overlapping column names (`created_at_utc` etc.) together.

---

### Week 2 — Migration resolved, IRefDataMapper removed, GetById built, GetAll + cursor pagination built (28 Jun)

**The "empty migration" mystery from the GUID reversal — resolved, no action needed.**
Generating a migration after adding `.ValueGeneratedNever()` produced an empty `Up()`/`Down()`. Traced through rather than assumed broken: checked the model snapshot (already correctly showed no `.ValueGeneratedOnAdd()` on the changed columns) and the real Postgres column directly via pgAdmin — `instrument_id` had **no database-level default at all**, unlike `valid_to` which genuinely does (`DEFAULT '9999-12-31'::date`, written explicitly in the original config). Conclusion: the original migration never gave Guid PKs an explicit SQL default in the first place — EF Core's "database-generated" assumption for Guids didn't require one. Nothing to remove at the database level; the C#-side fix (`= Guid.NewGuid()` + `ValueGeneratedNever()`) was already complete and sufficient on its own. Empty migration safely discarded via `dotnet ef migrations remove` (confirmed: only pops the most recent migration, never touches earlier history).

**`IRefDataMapper` factory pattern — fully removed, replaced with a direct if-chain.**
Built and used for real in `InstrumentMapper.ToDomain`/`ToResponse` — then caught, independently, that it wasn't delivering its core promise: `ToDomain` still needed its own switch on `Type` immediately after the factory call, purely to cast `IInstrumentRefData` back to the concrete type for navigation-property assignment. The factory added a real cost (interface, 3 classes, DI registration) without removing the one thing it was supposed to remove (a switch on `Type` inside `ToDomain`). Removed entirely: `IRefDataMapper.cs` deleted, `Program.cs` registration deleted, `BondRefDataMapper`/`EquityRefDataMapper`/`EtfRefDataMapper` converted to plain `static` classes with `static ToDomain(...)`/`static ToResponse(...)` methods, `InstrumentMapper` itself converted to fully `static` (no more injected dependency). `ToDomain` and `ToResponse` both now use one direct if-chain calling the right static method by name — fewer files, less indirection, identical correctness. **Lesson kept generally:** a pattern can look right in design and still not earn its cost once actually used — the only way to know is building it for real and using it, the same as the GUID reversal.

**Real bug found via live testing — `SymbolXRef.ValidFrom` left at C# default (`0001-01-01`).**
`InstrumentMapper.ToDomain`'s symbol-building lambda set `SymbologyId`, `Symbol`, `IsPrimary` but never `ValidFrom` (unlike the parallel `InstrumentStatusHistory` construction, which did set it correctly) — `ValidTo` looked fine only because it has a DB-level default that fires on insert; `ValidFrom` has no such default, it was always expected to come from C#. Fixed: `ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)` added to the lambda.

**Real data-integrity gap found via live testing — duplicate ISINs across different instruments were possible.**
Ran the same Create payload 3 times (testing, not realizing duplicates would actually persist) — 3 separate `Instrument` rows were created, all sharing the same ISIN, because the existing unique index (`InstrumentId, SymbologyId, ValidFrom`) includes `InstrumentId`, so a *new* instrument with a *repeated* symbol value never collides with it. Real discussion, several wrong turns kept for the reasoning: first considered putting the constraint directly on `Instrument` (wrong — uniqueness is about the identifier, not the instrument); user correctly pushed back that "two different instruments having the same SymbologyId+Symbol" sounded wrong in isolation, until walked through with the actual real-world meaning (an ISIN is *defined* to be globally unique to one instrument, by the ISO 6166 standard itself — this isn't a database opinion, it's what an ISIN structurally *is*). Also surfaced and resolved: ISINs *can* legitimately change (already handled by the existing temporal `ValidFrom`/`ValidTo` pattern) — so the new unique index must be a **partial** index, scoped to `WHERE valid_to = '9999-12-31'`, so a closed-out historical row never competes with the new current row for uniqueness. Walked through a concrete two-row example (closed `sxr-001`, new `sxr-002`, same ISIN) to make the filter's purpose visible rather than abstract. Final index: `(SymbologyId, Symbol) WHERE valid_to = '9999-12-31'`, unique. Migration written, **applied only after manually deleting the 3 duplicate test rows first** (a unique index cannot be created over data that already violates it) — confirmed via re-test that a 4th attempt to create the same ISIN now correctly fails with `ConflictException`.

**`GetByIdAsync` — built independently (not walked through line-by-line this session), reviewed and confirmed correct.**
Queries `instruments`, then *all three* ref-data tables (reading whichever one returns a row, rather than switching on `Type` first) — a reasonable, slightly different approach than discussed, sidesteps needing `Type` known ahead of the ref-data query. Queries all matching symbols (joined to `symbologies` in one query, no per-symbol round trip), all status history rows, and all `VendorInterfaceSymbolXRef` rows (joined to `vendor_interfaces`/`vendors`) for this instrument's symbols, each as its own separate, simple query — explicitly **not** using Dapper's `QueryMultipleAsync`/`splitOn` multi-mapping (which caused real, lengthy confusion mid-session and was deliberately abandoned in favour of separate queries + in-memory `GroupBy`/`ToDictionary` stitching, done in the repository). A `DateOnlyTypeHandler` was added independently to handle a real Dapper-vs-Postgres `DateOnly` mapping gap not previously encountered. **Not yet thoroughly tested** — ran once, didn't throw, full correctness (especially the VendorInterface/Symbology nesting) still needs a real verification pass.

**Real, deliberately-deferred gap surfaced and explicitly scoped out of v1 — ref-data has no provenance, and can't hold multiple vendors' conflicting versions.**
Long, honest back-and-forth: user correctly identified that `BondRefData`/`EquityRefData`/`EtfRefData` are single rows with no record of which vendor's numbers they hold, and no way to store two vendors' differing values for the same field (unlike symbols, which already handle multi-vendor correctly via `VendorInterfaceSymbolXRef`). A real fix was designed in detail — versioned ref-data rows with `ValidFrom`/`ValidTo` and a primary-vendor concept, the same temporal pattern already proven for `SymbolXRef`/`InstrumentStatusHistory` — but explicitly **not built**, on the user's own call: given the actual goal (portfolio project to demonstrate skill and get hired, not a production system that needs to be fully complete), the time cost of correctly redesigning 3 tables' relationships, EF Core configs, the mapper, and the response shape wasn't judged worth it relative to what it would add to the portfolio's demonstrated value. Documented here explicitly so this isn't silently forgotten or rediscovered as a "bug" later — it's a known, deliberate v1 scope boundary, same status as the multi-vendor ref-data design already sitting in `ExtendedFeatures.md`.

**`InstrumentFilter` + `BondFilter`/`EquityFilter`/`EtfFilter` — fully designed field-by-field, then actually built.**
Real design pass, not just listing every available field: for each candidate field, explicitly asked "is this genuinely useful as an exact-match filter, or does it need to be a range, or does it not belong as a filter at all" —
- **Excluded as filters** (continuous numeric fields, exact-match filtering rarely useful): Bond's `FaceValue`/`CouponRate`, Equity's `LotSize`/`ParValue`/`SharesOutstanding`.
- **Range, not exact** (deliberately, field by field, not a blanket rule): `ListedDateFrom`/`ListedDateTo` (base filter), Bond's `MaturityBefore`/`MaturityAfter` ("what's maturing soon" is a real, common query), ETF's `ExpenseRatioMax` ("cheaper than X%").
- **Exact, not range**: Bond's `IssueDate`, ETF's `InceptionDate` — both represent "find this one specific issuance/launch event," not a date someone browses a range of.
- **Fuzzy (`ILIKE`), not exact**: `Name` — instrument names are rarely searched for exact matches.
- **Deliberately excluded from `GetAll` entirely, kept for the already-designed `Search` endpoint instead**: combined name-+-symbol fuzzy search (this already exists as its own designed feature, `InstrumentSearchResult`, from weeks earlier — re-implementing it inside `GetAll` would duplicate it), and vendor/interface filtering (requires its own join through `vendor_interface_symbol_x_ref`, judged closer in spirit to a search-style join-heavy operation than a direct field filter).
- **`Status` defaults to `Active`** (matching `Symbology`'s precedent) — and a real distinction surfaced and corrected mid-session: the mandatory `INNER JOIN instrument_status_history ish ON ish.valid_to = '9999-12-31'` fetches the *current* status row (whatever it is), which is **not** the same thing as filtering for Active — that's a separate, additional `WHERE ish.instrument_status = @status` condition. Initially conflated, caught and corrected before it became a silent bug.

**Dynamic SQL construction — the `conditions`/`parameters`/`joins` list pattern, extended to 3 conditional JOINs.**
Same incremental-list-building pattern as Vendor/Symbology's earlier filters, now handling optional `JOIN`s (added to their own list, inserted between `FROM` and `WHERE`) alongside optional `WHERE` conditions, for whichever one of Bond/Equity/ETF filter is populated — each conditionally adding its own join, never joining tables that aren't actually needed for the given request.

**Cursor pagination — built for real, with the actual reasoning behind why a single-column cursor doesn't work.**
Real, important correction mid-session: an early instinct to use `InstrumentId` alone as the cursor was traced through with a concrete counter-example (a GUID can sort lower as a raw value while still belonging to a row that comes *later* in true `created_at_utc` order) — proving a single-column cursor would silently skip or duplicate rows once the sort order is based on `CreatedAtUtc`, not `InstrumentId`, since the cursor condition must match whatever columns `ORDER BY` actually uses. Resolved with a compound cursor: `InstrumentCursorPayload(DateTime CreatedAtUtc, Guid InstrumentId)`, base64-JSON-encoded via `CursorHelper`, decoded with explicit handling for both real failure modes (bad base64 → `FormatException`, valid base64 but invalid JSON → `JsonException`), both mapped to a single `InvalidCursorException` rather than leaking a generic, confusing error. SQL cursor condition uses Postgres row-comparison syntax, `(i.created_at_utc, i.instrument_id) < (@cursor_created_at, @cursor_instrument_id)` (flipped to `<` since final order is `DESC`, newest first) — correctly expresses "strictly earlier in created_at_utc, OR equal and instrument_id also earlier," avoiding the single-column gap. Pagination itself uses the standard **fetch-N+1, trim-and-flag** pattern: query `LIMIT @pageSize + 1`; if more than `pageSize` rows come back, trim the last one and build `NextCursor` from the last *kept* row; otherwise `NextCursor` stays null, correctly signalling "no more pages" with zero extra round trips or count queries needed.
**Confirmed correct, not assumed:** sorting and pagination were questioned as possibly-separable concerns — resolved honestly: they're not separable *here* specifically because there's only one fixed sort order in this design (not a caller-selectable one), and cursor pagination is mathematically meaningless without a stable, deterministic order to be "after" relative to. A caller-selectable sort would be a genuinely separate, harder feature, not built.

**Ref-data fetch for the paginated list — separate batched query by design, not `QueryMultipleAsync`.**
Explicitly reasoned through and confirmed as the *correct* shape, not a fallback: unlike `GetById` (where the single instrument id is known before any query runs), `GetAll`'s matching instrument ids are only known *after* the first paginated query returns — so a second query, `WHERE instrument_id = ANY(@ids)`, scoped to exactly those ids, is the right tool; `QueryMultipleAsync` doesn't fit here because all statements in that kind of batch are sent together upfront, before any result is known, which doesn't work when one query's output determines the next query's filter values.

**Process note — this session ran long enough (spanning the GUID migration resolution through full GetAll+pagination) that response quality was reported as degrading; work is continuing in a fresh session from this point, using this document as the handoff.**

---



- InstrumentFilter fields — deferred until building GetAll endpoint
- Redis cache invalidation strategy — deferred until caching layer is actually built (NOT dropped — confirmed 19 Jun: lives inside the Instrument phase, specifically on the resolve endpoint, sub-50ms target for Holdings Engine/OMS consumers)
- Cursor pagination on GetAllAsync — confirmed 19 Jun: implemented as part of the Instrument phase, not deferred further — it's part of what makes that endpoint correct, not a later optimization
- JWT scope names — deferred until auth layer (now scoped inside the Frontend phase, weeks 6–8)
- Seed data strategy for vendors and symbology types
- Frontend stack confirmed 19 Jun: React + Tailwind (not raw CSS) specifically to avoid box-model/layout friction, basic JWT auth, role-based UI (admin / trader / viewer) enforced server-side not just hidden in UI
- Whether the same parent-child id-match check needed for `VendorInterface` will be needed for every nested Instrument resource (symbols, status history) — almost certainly yes, confirm and apply during Instrument design session, don't rediscover it then

---

*Owner: Vedita Kamat*
*Started: June 2026*
*Repo: https://github.com/kamatvedita99/InstrumentCatalogue*

