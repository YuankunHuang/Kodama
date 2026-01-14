# Kodama 开发日志 (Development Log)

**项目启动日期**: 2026-01-13  
**当前阶段**: Phase 1 - The Pulse (MVP) ✅ 已完成  
**下一阶段**: Phase 2 - The Loop (核心循环)

---

## 📋 Phase 1 进度追踪

| 任务 | 状态 | 完成日期 | 备注 |
|------|------|---------|------|
| 1.1 后端 Clean Architecture 搭建 | ✅ Completed | 2026-01-14 | 5 个项目：Domain, Application, Infrastructure, API, Shared |
| 1.2 SimulationLoop (10Hz) | ✅ Completed | 2026-01-14 | BackgroundService + 圆周运动 |
| 1.3 SignalR Hub 广播 | ✅ Completed | 2026-01-14 | JSON 序列化 SnapshotData |
| 1.4 Unity 客户端架构 | ✅ Completed | 2026-01-14 | GameManager + ModuleRegistry + EventBus |
| 1.5 SignalR 客户端连接 | ✅ Completed | 2026-01-14 | NetworkManager + SignalRClient |
| 1.6 插值渲染 | ✅ Completed | 2026-01-14 | RenderManager + 双缓冲快照 + Lerp |

**图例**: ⏳ Pending | 🔄 In Progress | ✅ Completed | ❌ Blocked

---

## 🏗️ 当前架构

### 后端 (ASP.NET Core 8.0)
```
Kodama.sln
├── Kodama.API/              # 入口点，DI 注册，SignalR Hub 映射
├── Kodama.Application/      # 业务逻辑，接口定义
├── Kodama.Infrastructure/   # SignalR 实现，HostedService
├── Kodama.Domain/           # 领域模型，值对象，枚举
└── Kodama.Shared/           # 共享 DTO (netstandard2.1)
```

### 客户端 (Unity 6000.3.1f1 LTS + URP)
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

## 📝 技术决策记录 (ADR)

### ADR-001: 采用 Clean Architecture
**日期**: 2026-01-13  
**决策**: 使用 Clean Architecture 而非传统三层架构  
**理由**:
- Domain 层完全独立，便于单元测试
- 符合 SOLID 原则，特别是依赖倒置
- 易于扩展和维护

### ADR-002: 使用 .NET 8
**日期**: 2026-01-13  
**决策**: 目标框架为 .NET 8（LTS）  
**理由**: 稳定、社区资源丰富

### ADR-003: Phase 1 客户端架构选择
**日期**: 2026-01-14  
**决策**: 使用传统 MonoBehaviour 而非 ECS/DOTS  
**理由**:
- Phase 1 目标是快速联调
- ECS 学习曲线与"快速验证"目标冲突
- 保留后续迁移可能性（Phase 3+）

### ADR-004: Shared DLL 共享 DTO
**日期**: 2026-01-14  
**决策**: 创建 Kodama.Shared 项目 (netstandard2.1)  
**理由**:
- 后端和 Unity 共享相同的 DTO 定义
- 类型安全，单一来源
- 避免手动同步代码

### ADR-005: EventBus 模块间通信
**日期**: 2026-01-14  
**决策**: 使用 EventBus 发布/订阅模式  
**理由**:
- NetworkManager 和 RenderManager 解耦
- 避免直接依赖

---

## 🐛 问题与解决方案

### 2026-01-14 JSON 反序列化大小写问题
**现象**: `SnapshotData.Agents` 为 null  
**原因**: System.Text.Json 默认大小写敏感，后端输出 camelCase，客户端 DTO 是 PascalCase  
**解决方案**: 添加 `PropertyNameCaseInsensitive = true`  

### 2026-01-14 插值时间基准不一致
**现象**: Agent 位置跳跃/不平滑  
**原因**: 混用服务器时间戳和客户端时间  
**解决方案**: 插值完全基于客户端时间 (`Time.time`)  
**学到的教训**: 插值的起点、终点、中间点必须同源！

---

## 💡 学习笔记

### 插值原则
```
Lerp(A, B, t)

t = (当前时间 - 起点时间) / 总时长

关键：起点时间、当前时间、总时长 必须同源！
❌ 混用服务器时间 + 客户端时间 = 无意义
✅ 全部用客户端时间 = 正确插值
```

### Unity 静态变量陷阱
- 静态变量在 Play Mode 切换时不会重置
- 解决方案：`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`

---

## 🔄 更新日志

### 2026-01-14
- ✅ Phase 1 全部完成
- 后端：SimulationLoop + SignalR 广播
- 客户端：SignalR 连接 + 插值渲染
- 创建 Kodama.Shared 项目共享 DTO
- 实现 EventBus 模块间通信

### 2026-01-13
- 项目启动
- 完成设计文档评审
- 创建 Clean Architecture 骨架

---

## 📌 下次开发提醒

**当前位置**: Phase 1 完成 ✅  
**下一步**: Phase 2 - The Loop (核心循环)  
- 引入资源和树
- Agent 寻找最近资源
- 移动并运回

---

## 🎓 导师批注区

### Phase 1 架构审查 ✅
- [x] Domain 层无外部依赖
- [x] 依赖方向正确：API → Infrastructure → Application → Domain
- [x] 使用依赖注入
- [x] DTO 使用 struct + 属性
- [x] 插值实现正确（同源时间基准）
- [x] EventBus 解耦模块通信

**评价**: 架构扎实，代码清晰。特别是 MonoBehaviourUtil 的对象池设计和 EventBus 的类型安全检查，展现了专业水平。

---

**最后更新**: 2026-01-14  
**更新者**: Technical Mentor
