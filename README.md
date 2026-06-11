# Kodama (Project Sim-Forest)

**Distributed Real-Time Simulation System** — 10,000 Agent Resource Collection Simulation

![Demo](docs/demo.gif)

## Highlights

- **10,000 Agents** real-time simulation, server tick < 1ms
- **Zero GC allocation** in hot path, stable 60 FPS client
- **Server-authoritative architecture**, client is pure renderer
- **Real-time HUD monitoring**, professional-grade data visualization

## Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | ASP.NET Core 8.0, Clean Architecture |
| **Frontend** | Unity 6000.3.1f1 LTS, URP |
| **Communication** | SignalR (WebSocket) + MessagePack |
| **Rendering** | GPU Instancing, ShaderGraph |
| **Architecture** | Server-Authoritative + Dumb Client |

## Project Structure

```
Kodama/
├── Kodama.API/              # Entry point, DI registration, SignalR Hub
├── Kodama.Application/      # SimulationLoop, AgentBehaviourService, WorldState
├── Kodama.Infrastructure/   # SignalR Broadcaster, HostedService
├── Kodama.Domain/           # AgentStore (sparse-set SoA), Resource, Tree
├── Kodama.Shared/           # DTOs (MessagePack, netstandard2.1)
└── Kodama.Client/           # Unity client (URP + GPU Instancing)
```

## Requirements

| Component | Requirement |
|-----------|-------------|
| OS | Windows 10/11 |
| .NET | [.NET 8.0 Runtime or SDK](https://dotnet.microsoft.com/download/dotnet/8.0) |
| GPU | DirectX 11 compatible |

## Quick Start

### One-Click Launch (Recommended)

**Double-click `RunDemo.bat`**

The script will automatically:
1. Start the backend server (new window)
2. Launch the Unity client

### Manual Start

#### 1. Start Backend

```bash
cd Kodama
dotnet run --project Kodama.API
```

Server starts at `http://localhost:5000`

#### 2. Start Unity Client

Run `Kodama.Client/Build/Kodama.exe`

Or open `Kodama.Client/` in Unity Editor and play `Assets/Scenes/Main.unity`

### Controls

| Key | Function |
|-----|----------|
| H | Toggle HUD |
| R | Restart Simulation |
| Space | Pause/Resume |
| -/+ | Slow Down/Speed Up |
| 0 | Reset Speed |

## Data-Oriented Core

The server's hot path runs on data-oriented storage rather than object graphs:

- **Sparse-set SoA agent store** ([`Kodama.Domain/Entities/AgentStore.cs`](Kodama.Domain/Entities/AgentStore.cs)) —
  dense/sparse index pairs with parallel component arrays for position, state,
  inventory, and harvest target (ECS-style storage). There is no `Agent` class:
  agents exist only as indices into these arrays, so the tick path touches no
  per-agent heap objects.
- **Per-state dense sets** — each FSM state owns its own sparse set, so
  [`AgentBehaviourSystem`](Kodama.Application/Services/AgentBehaviourSystem.cs)
  processes each state as a batch over a dense `int` span (state segments are
  snapshotted into a pre-allocated scratch buffer per tick, so each agent is
  stepped exactly once), and the HUD's per-state counts are O(1) reads.
- **Position-hashed resource index** ([`Kodama.Application/States/WorldState.cs`](Kodama.Application/States/WorldState.cs)) —
  resources are indexed by hex cell (`Dictionary<Position, HashSet<Resource>>`)
  for O(1) cell lookups. Resources and the base Tree stay as plain objects:
  there are ~200 of them versus 10,000 agents, and they are not the bottleneck.
- **Acknowledged debt:** nearest-resource search is currently a linear scan over
  available resources; at 10K agents, profiling showed it was not the bottleneck,
  so it stays simple until measurements say otherwise.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         SERVER                              │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ AgentStore  │→ │ SimulationLoop│→ │ SignalRBroadcaster│  │
│  │ (10k SoA)   │  │   (20 Hz)    │  │   (MessagePack)   │  │
│  └─────────────┘  └──────────────┘  └─────────┬─────────┘  │
└───────────────────────────────────────────────│─────────────┘
                                                ↓ WebSocket
┌───────────────────────────────────────────────│─────────────┐
│                         CLIENT                │             │
│  ┌─────────────┐  ┌──────────────┐  ┌────────┴────────┐   │
│  │RenderManager│← │ Interpolation│← │  NetworkManager │   │
│  │(GPU Instancing)│ │  (60 FPS)   │  │   (SignalR)    │   │
│  └─────────────┘  └──────────────┘  └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Performance Metrics

| Metric | Value |
|--------|-------|
| Agent Count | 10,000 |
| Server Tick Time | < 1 ms |
| Server GC Allocation | 0 bytes/tick |
| Client FPS | 60 (GPU Instancing) |
| Network Protocol | MessagePack (Binary) |

## Agent State Machine

```
     ┌─────────────────────────────────────────┐
     ↓                                         │
  [Idle] → [FindingResource] → [MovingToResource]
                                      │
                                      ↓
  [Depositing] ← [ReturningToBase] ← [Collecting]
       │
       └──→ [Idle] (loop)
```

## Development Log

See [DEVLOG.md](DEVLOG.md)

## Architecture Decision Records (ADR)

| Decision | Reason |
|----------|--------|
| Sparse-set SoA storage (`AgentStore`) | ECS-style parallel arrays + per-state dense sets; zero per-agent allocations, cache-friendly iteration; replaced the original `Dictionary<int, Agent>` object model |
| Position-hashed resource index | O(1) hex-cell lookups for resources instead of world scans |
| Guid → int ID | Reduce snapshot size, improve serialization performance |
| JSON → MessagePack | Binary serialization, smaller and faster |
| Duck Typing Enumerator | Avoid GC allocation from Dictionary.Values |
| GPU Instancing | Single draw call for 10K+ entities |
| Linear nearest-resource scan (kept) | Profiled as non-bottleneck at 10K agents; complexity deferred until measurements demand it |

## Troubleshooting

**Q: Client can't connect to server?**

Ensure the backend window shows "Now listening on: http://localhost:5000"

**Q: Can't see agents?**

Check Unity console for "Connected to server" message.

**Q: Backend error?**

Ensure port 5000 is not occupied by another process.

## License

Private Project
