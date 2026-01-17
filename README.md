# Kodama (Project Sim-Forest)

分布式实时模拟系统 / God Game

## 技术栈

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Unity 6000.3.1f1 LTS with URP
- **Communication**: SignalR (WebSocket) + JSON
- **Architecture**: Server-Authoritative + Dumb Client

## 项目结构

```
Kodama/
├── Kodama.API/              # 入口点，DI 注册，SignalR Hub 映射
├── Kodama.Application/      # 业务逻辑，接口定义
├── Kodama.Infrastructure/   # 基础设施实现（SignalR, HostedService）
├── Kodama.Domain/           # 领域模型，值对象，枚举
├── Kodama.Shared/           # 共享 DTO (netstandard2.1, Unity 兼容)
└── Kodama.Client/           # Unity 6 客户端 (URP)
```

## 快速开始

### 1. 运行后端

```bash
cd Kodama
dotnet run --project Kodama.API
```

服务器将启动在 `http://localhost:5059`

SignalR Hub 端点: `ws://localhost:5059/gamehub`

### 2. 运行 Unity 客户端

1. 用 Unity 6000.3.1f1 打开 `Kodama.Client/`
2. 打开场景 `Assets/Scenes/Main.unity`
3. 点击 Play

### 3. 更新共享 DLL（修改 DTO 后）

```bash
dotnet build Kodama.Shared
# 然后复制 DLL 到 Unity:
# Kodama.Shared/bin/Debug/netstandard2.1/Kodama.Shared.dll
# → Kodama.Client/Assets/Plugins/Kodama.Shared.dll
```

## 当前状态

### Phase 1 ✅ 已完成
- [x] SignalR Hub 搭建
- [x] 模拟循环 (10Hz Tick)
- [x] Agent 圆周运动
- [x] Snapshot 广播
- [x] Unity 客户端连接
- [x] 插值渲染

### Phase 2 🔄 进行中（后端完成）
- [x] Domain 实体（Agent, Resource, Tree）
- [x] WorldState 容器（双字典索引 + BFS）
- [x] Agent FSM（7 状态完整循环）
- [x] SimulationLoop 重构（延迟删除 + 轴坐标）
- [x] 后端测试验证（3 Agent 正确采集）
- [ ] Unity 客户端更新（Hex 坐标转换 + 多 Agent 渲染）

## 开发路线图

| Phase | 目标 | 状态 |
|-------|------|------|
| Phase 1 | 后端 MVP + 联调 | ✅ 已完成 |
| Phase 2 | 资源采集循环 | 🔄 进行中（后端完成） |
| Phase 3 | 500+ Agent 规模化 | ⏳ 待开始 |
| Phase 4 | 视觉打磨 + 部署 | ⏳ 待开始 |

## 协议

私有项目
