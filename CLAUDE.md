# AGENT EXECUTION POLICY — .NET 10 Microservices Runtime

This file holds **only** behavioral policy. Patterns, conventions, and routing are NOT
duplicated here — they have a single source of truth:

- **Code patterns / conventions / invariants** → the `Example` (EF) aggregate is the canonical
  archetype. Read the reference file ARQ loads for the current operation. Do not degrade it.
- **Routing, decision tables, reference loading, validation** → `/arquitectura` (ARQ state machine).
- **Stack versions** → the `.csproj` files (summarized below for quick reference only).

If a rule can be derived by reading `Example` code or ARQ, it does not belong in this file.

---

## RUNTIME_DIRECTIVE

- ARQ is the source of truth for routing, reference loading, and state transitions.
- Topology and namespaces are stable. Do not rediscover either.
- Default mode: execute. Escalate only under ESCALATION_RULES.

---

## NO_EXPLORATION

The single biggest failure mode is exploring instead of executing. Forbidden:

- Repository Glob/grep loops to discover structure
- Full-tree scans (`**/*.cs` without a specific target)
- Namespace rediscovery
- Reading files not specified by ARQ REFERENCE_FILES for the current operation

Reference loading is **scoped**, not a flat cap. What is forbidden is *unbounded discovery*
(Glob loops, full-tree scans, rediscovery); reading an enumerated, finite set is deterministic
loading, NOT exploration:

- **Single operation** (one command/query/service/DI edit): ARQ `S3_LOAD` reads **exactly 1**
  reference file — the ejemplar that matches the decided contract.
- **Program** (≥2 aggregates, or a full vertical: entity + CRUD + queries + API): ARQ `S_WARMUP`
  reads the bounded `ARCHETYPE_SET` **once**, up front. The session is then *warm* and every
  subsequent aggregate uses `S2_WARM` (zero re-reads). Re-reading the archetype per aggregate is
  the waste this exception removes.

ARQ `S_ERROR` budget: 1 Glob + 1 Read max.

---

## ESCALATION_RULES

Escalate to the user **only** when:

- A feature requires a NuGet package not present in the stack
- A structural change is required (new layer, new cross-cutting contract, new infrastructure component)
- Two valid approaches exist with no criterion to decide between them
- A security or performance risk is detected that existing patterns do not cover

Do **not** escalate for:
- Ambiguous naming
- Missing DTO fields
- Whether to add XML doc
- Standard CQRS wiring decisions covered by ARQ

---

## STACK_CONSTRAINTS

Quick reference — authoritative versions live in the `.csproj` files.

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET | 10 (`net10.0`) |
| Language | C# | 14 |
| ORM | EF Core | 10 |
| Micro-ORM | Dapper | 2.1.x · `MatchNamesWithUnderscores = true` |
| RDBMS | PostgreSQL | 16 · Npgsql |
| Mediator | MediatR | 14.1 |
| Auth | JwtBearer | 10.0.x · HS256 |
| OpenAPI | Swashbuckle + Microsoft.OpenApi | 10.x / 2.x |
| Jobs | System.Threading.Channels | built-in |
| Observability | OpenTelemetry | 1.15.x |
| Tests | xUnit + Moq + FluentAssertions | latest |

- Use the most modern API available within the declared stack version.
- Do not add packages that duplicate existing framework functionality (→ ESCALATION_RULES).
