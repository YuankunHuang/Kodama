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
├── Kodama.Domain/           # Agent, Resource, Tree entities
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

Server starts at `http://localhost:5059`

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

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         SERVER                              │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ WorldState  │→ │ SimulationLoop│→ │ SignalRBroadcaster│  │
│  │ (10k Agents)│  │   (20 Hz)    │  │   (MessagePack)   │  │
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
| Guid → int ID | Reduce snapshot size, improve serialization performance |
| JSON → MessagePack | Binary serialization, smaller and faster |
| Duck Typing Enumerator | Avoid GC allocation from Dictionary.Values |
| GPU Instancing | Single draw call for 10K+ entities |

## Troubleshooting

**Q: Client can't connect to server?**

Ensure the backend window shows "Now listening on: http://localhost:5059"

**Q: Can't see agents?**

Check Unity console for "Connected to server" message.

**Q: Backend error?**

Ensure port 5059 is not occupied by another process.

## License

Private Project
