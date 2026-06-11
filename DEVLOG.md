# Kodama Development Log

**Project Start Date**: 2026-01-13  
**Current Stage**: ✅ MVP Complete  

---

## Architecture

### Backend (ASP.NET Core 8.0)
```
Kodama.sln
├── Kodama.API/              # Entry point, DI registration, SignalR Hub mapping
├── Kodama.Application/      # Business logic, interface definitions
├── Kodama.Infrastructure/   # SignalR implementation, HostedService
├── Kodama.Domain/           # Domain models, value objects, enums
└── Kodama.Shared/           # Shared DTOs (netstandard2.1)
```

### Client (Unity 6000.3.1f1 LTS + URP)
```
Kodama.Client/
└── Assets/
    ├── Scripts/
    │   ├── Core/            # GameManager, ModuleRegistry, EventBus
    │   ├── Network/         # NetworkManager, SignalRClient
    │   └── Render/          # RenderManager
    └── Plugins/
        └── Kodama.Shared.dll
```

---

## Architecture Decision Records (ADR)

### ADR-001: Clean Architecture
**Decision**: Use Clean Architecture instead of traditional 3-tier architecture  
**Reasoning**:
- Domain layer completely independent, easy to unit test
- Follows SOLID principles, especially Dependency Inversion
- Easy to extend and maintain

### ADR-002: ID Reference Instead of Object Reference
**Decision**: Entities reference each other via int ID, not direct object references  
**Reasoning**:
- Independent lifecycle: ID query returns null after object deletion
- Avoid circular references
- Serialization safe: DTO only needs to pass ID

### ADR-003: Deferred Deletion Pattern
**Decision**: Collect IDs to delete during Tick, delete all at once after iteration  
**Reasoning**:
- Avoid exception when modifying collection during iteration
- Zero-allocation optimization: most Ticks have no deletions, toRemove stays null

### ADR-004: Zero-Allocation Hot Path
**Decision**: Eliminate all heap allocations in Tick loop  
**Reasoning**:
- Avoid GC pause stuttering
- Support 100K+ Agent scale
- Follows Data-Oriented Design principles

**Implementation**:
- Return `Dictionary.ValueCollection` instead of `IEnumerable<T>` (avoid Boxing)
- `Position` uses `record struct` (value type, stack allocation)
- Custom `NeighboursEnumerator` struct (Duck Typing pattern)

### ADR-005: GPU Instancing Rendering
**Decision**: Use `Graphics.DrawMeshInstanced` instead of GameObject pool  
**Reasoning**:
- Single Draw Call renders 1023 instances
- Very low CPU overhead, suitable for large-scale scenes

### ADR-006: Guid → int
**Decision**: Entity ID from Guid to int  
**Reasoning**:
- Single server simulation, no need for distributed ID
- int is 4x faster than Guid, cache-friendly
- Reference: UE5 MassEntity uses int Entity Index

### ADR-007: MessagePack Serialization
**Decision**: Replace JSON with MessagePack  
**Reasoning**:
- Binary serialization, 70% smaller payload
- No .proto definition needed (compared to Protobuf)
- Good Unity compatibility

### ADR-008: DOD Migration — Sparse-Set SoA Agent Storage (2026-06-11)
**Decision**: Replace the `Dictionary<int, Agent>` object model with `AgentStore`,
a sparse-set, structure-of-arrays store with per-state dense sets; delete the
`Agent` class entirely.  
**Reasoning**:
- Component arrays (Q/R, state, inventory, harvest target) indexed by entity id —
  cache-linear iteration, zero per-agent heap objects
- Per-state sparse sets let `AgentBehaviourSystem` process each FSM state as a
  batch over a dense `int` span, and make HUD state counts O(1) (previously a
  per-agent switch over all 10K agents every snapshot)
- State segments are snapshotted into a pre-allocated scratch buffer at tick
  start, so transitions during processing are safe and each agent is stepped
  exactly once per tick (preserving the old per-agent-switch semantics)
- Dropped the per-move `Dictionary<Position, HashSet<Agent>>` index maintenance
  (10K updates/tick) — it had no consumers; the position index is kept for
  resources only

**Measured after migration** (Debug build, in-process logging):
- 10,000 agents, 217 resources, full FSM cycle (Tree matter accumulating)
- Steady-state tick: **0.16–0.38 ms** (was: <1 ms claimed; now consistently ~0.25 ms)
- Steady-state allocation: **0 bytes/tick** (warm-up only: first 2 ticks)

**Also in this change**: removed two abandoned half-finished files that did not
compile (`SimulationManager`, stub `AgentBehaviourSystem`) — the build was red
before this migration and is green after.

---

## Performance Results

| Metric | Value |
|--------|-------|
| Agent Count | 10,000 |
| Server Tick Time | ~0.25 ms steady state (0.16–0.38 ms, post-DOD migration) |
| Server GC Allocation | 0 bytes/tick (steady state) |
| Client FPS | 60 (GPU Instancing) |
| Network Protocol | MessagePack (Binary) |

---

## Key Learnings

### Boxing Trap (C# Performance Critical)
```csharp
// ❌ Interface return causes Boxing
public IEnumerable<Agent> GetAllAgents() => _agents.Values;

// ✅ Return concrete type, avoid Boxing
public Dictionary<Guid, Agent>.ValueCollection GetAllAgents() => _agents.Values;
```

### Duck Typing Enumerator
```csharp
// C#'s foreach only needs GetEnumerator() + Current + MoveNext()
public struct NeighboursEnumerator
{
    private int _index;
    public Position Current => /* ... */;
    public bool MoveNext() => ++_index < 6;
    public NeighboursEnumerator GetEnumerator() => this;
}
// Zero allocation foreach!
```

### GPU Instancing
```csharp
Matrix4x4[] matrices = new Matrix4x4[count];
for (int i = 0; i < count; i++)
    matrices[i] = Matrix4x4.TRS(positions[i], rotation, scale);

Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count);
// One call renders all!
```

---

## Technical Highlights

1. **Server-Authoritative Architecture**: Backend authoritative, client pure renderer
2. **Zero-Allocation Hot Path**: Steady-state 0 bytes GC allocation
3. **GPU Instancing**: Single Draw Call renders 10K+ entities
4. **MessagePack Binary Protocol**: 70% network transfer compression
5. **Professional HUD**: Real-time performance monitoring
