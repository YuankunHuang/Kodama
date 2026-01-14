# Kodama (Project Sim-Forest)

分布式实时模拟系统 / God Game

## 技术栈

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Unity 6 (LTS) with URP
- **Communication**: SignalR (WebSocket)
- **Architecture**: Server-Authoritative + Dumb Client

## 项目结构

```
Kodama/
├── Kodama.API/              # 入口点，DI 注册，SignalR Hub 映射
├── Kodama.Application/      # 业务逻辑，接口定义，DTOs
├── Kodama.Infrastructure/   # 基础设施实现（SignalR, HostedService）
├── Kodama.Domain/           # 领域模型，值对象，枚举
└── KodamaClient/            # Unity 客户端（待创建）
```

## 快速开始

### 运行后端

```bash
cd Kodama
dotnet run --project Kodama.API
```

服务器将启动在 `http://localhost:5059`

SignalR Hub 端点: `ws://localhost:5059/gamehub`

### 当前状态 (Phase 1)

- [x] SignalR Hub 搭建
- [x] 模拟循环 (10Hz Tick)
- [x] Agent 圆周运动
- [x] Snapshot 广播
- [ ] Unity 客户端连接
- [ ] 插值渲染

## 开发路线图

| Phase | 目标 | 状态 |
|-------|------|------|
| Phase 1 | 后端 MVP + 联调 | 🔄 进行中 |
| Phase 2 | 资源采集循环 | ⏳ 待开始 |
| Phase 3 | 500+ Agent 规模化 | ⏳ 待开始 |
| Phase 4 | 视觉打磨 + 部署 | ⏳ 待开始 |

## 协议

私有项目
