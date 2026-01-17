# Kodama 开发日志 (Development Log)

**项目启动日期**: 2026-01-13  
**当前阶段**: Phase 2 - The Loop (核心循环) 🔄 进行中  
**下一阶段**: Phase 3 - The Swarm (规模化)

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

## 📋 Phase 2 进度追踪

| 任务 | 状态 | 完成日期 | 备注 |
|------|------|---------|------|
| 2.1 Domain 实体设计 | ✅ Completed | 2026-01-17 | Agent, Resource, Tree + Position 值对象 |
| 2.2 WorldState 容器 | ✅ Completed | 2026-01-17 | 双字典索引 + BFS 搜索 |
| 2.3 Agent FSM 实现 | ✅ Completed | 2026-01-17 | 7 状态完整循环 |
| 2.4 SimulationLoop 重构 | ✅ Completed | 2026-01-17 | 延迟删除 + 轴坐标快照 |
| 2.5 后端测试验证 | ✅ Completed | 2026-01-17 | 3 Agent 正确寻路采集返回 |
| 2.6 Unity 客户端更新 | ⏳ Pending | — | Hex 坐标转换 + 多 Agent 渲染 |

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

### ADR-006: Phase 2 坐标与移动设计
**日期**: 2026-01-14  
**决策**: 后端只用离散轴坐标 (Q, R)，不记录连续位置  
**理由**:
- 后端无渲染需求，离散坐标足够
- 前端负责 Hex → World 转换和插值
- 关注点分离：后端管逻辑，前端管表现

**决策**: Phase 2 使用即时移动（每 Tick 移动一格）  
**理由**:
- YAGNI：Phase 2 目标是验证核心循环，不是视觉打磨
- 延迟决策：精确移动插值等 Phase 3 有真实需求再设计
- 复用 Phase 1 的 Lerp 追赶逻辑

### ADR-007: ID 引用而非对象引用
**日期**: 2026-01-17  
**决策**: 实体间通过 Guid 引用，不直接持有对象引用  
**理由**:
- 生命周期独立：对象删除后 ID 查询返回 null，优雅处理
- 避免循环引用：Agent → Resource, Resource → Agent (Owner)
- 序列化安全：DTO 只需传递 ID
- 强制通过服务层操作：限制直接访问权限

### ADR-008: 延迟删除模式
**日期**: 2026-01-17  
**决策**: Tick 中收集待删除 ID，遍历结束后统一删除  
**理由**:
- 避免遍历时修改集合异常
- 零分配优化：大多数 Tick 无删除，toRemove 保持 null
- 比 ToList() 复制整个集合更高效

### ADR-009: WorldState 双字典索引
**日期**: 2026-01-17  
**决策**: 同时维护 ID 字典和 Position 字典  
**理由**:
- ID 查询 O(1)：通过 HarvestingResourceId 快速获取 Resource
- 空间查询 O(1)：通过 Position 快速获取该格子的所有实体
- 移动时需同步更新两个字典（通过 MoveAgent 方法封装）

### ADR-010: 独占采集机制
**日期**: 2026-01-17  
**决策**: Resource 同时只能被一个 Agent 采集（Owner 字段）  
**理由**:
- 技术简单：不需要处理多 Agent 竞争
- 行为有趣：Agent 自然分散到不同资源点
- 符合 Swarm 美学：分散采集形成"光流"效果
- YAGNI：Phase 3 需要共享采集再扩展

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

### ID 引用 vs 对象引用
```csharp
// ❌ 对象引用
public class Agent { public Resource Target; }
// 问题：Target 删除后悬空引用、循环引用、序列化困难

// ✅ ID 引用
public class Agent { public Guid? TargetResourceId; }
// 优点：生命周期独立、查不到就是没了、序列化安全
```

### 延迟删除模式
```csharp
// ❌ 遍历时删除
foreach (var agent in agents) {
    if (agent.IsDead) agents.Remove(agent); // 💥 异常
}

// ✅ 延迟删除
List<Guid>? toRemove = null;
foreach (var agent in agents) {
    if (agent.IsDead) {
        toRemove ??= new List<Guid>();
        toRemove.Add(agent.Id);
    }
}
if (toRemove != null) {
    foreach (var id in toRemove) worldState.RemoveAgent(id);
}
```

### Tick-based 模拟 vs 事件驱动
- **事件驱动**：适合"偶尔发生"的场景（UI 点击）
- **Tick 驱动**：适合"持续更新"的场景（1000 个 Agent 移动）
- 在模拟中，每 Tick 都有大量实体需要更新，遍历比事件更高效

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

### EF Core 兼容模式
```csharp
// 无参构造函数 + 对象初始化器
private Entity() { } // EF Core 用
public static Entity Create(...) {
    return new Entity { Prop1 = val1, Prop2 = val2 };
}
```

---

## 🔄 更新日志

### 2026-01-17
- ✅ Phase 2 后端完成
- **Domain 层**：
  - 创建 Agent, Resource, Tree 实体
  - 统一 EF Core 兼容模式（无参构造函数 + 对象初始化器）
  - Agent 添加 Inventory, HarvestingResourceId
  - Resource 实现 Claim/Extract/Release 机制
- **Application 层**：
  - 创建 WorldState（双字典索引 + BFS 搜索）
  - 创建 AgentBehaviorService（完整 FSM）
  - 重构 SimulationLoop（延迟删除 + 轴坐标快照）
- **测试验证**：
  - 3 个 Agent 正确执行采集循环
  - 状态机流转正常
  - 资源耗尽后正确清理
- **开发环境**：
  - 修复 .gitignore（保留 Unity NuGet 包）
  - 安装 NuGetForUnity + Rider 集成

### 2026-01-14 (下午)
- Phase 2 架构设计讨论
- 确认坐标系统：后端离散 (Q,R)，前端离散+连续
- 确认移动方案：Phase 2 即时移动，Phase 3 再优化
- 记录 ADR-006

### 2026-01-14 (上午)
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

**当前位置**: Phase 2 后端完成，准备更新客户端  
**下一步**: Unity 客户端渲染更新

### Phase 2 后端已完成

**核心实体**：
- ✅ Agent：状态机 + 库存 + 目标追踪
- ✅ Resource：独占采集 + 耗尽检测
- ✅ Tree：物质存储
- ✅ Position：六边形坐标 + 距离计算

**业务逻辑**：
- ✅ WorldState：双字典索引（ID + Position）+ BFS 搜索
- ✅ AgentBehaviorService：完整 FSM（7 状态）
- ✅ SimulationLoop：延迟删除模式 + 轴坐标快照

**测试结果**：
- ✅ 3 个 Agent 从 Tree (0,0) 出发
- ✅ 正确寻路到 6 个 Resource
- ✅ 采集后返回 Tree 存放
- ✅ 状态机循环正常

### Phase 2 客户端待实现

1. **Hex 坐标转换**：
   - 轴坐标 (Q, R) → 世界坐标 (x, y, z)
   - Pointy-topped 公式

2. **多 Agent 渲染**：
   - 对象池管理
   - 根据 Snapshot 更新所有 Agent 位置

3. **插值优化**（可选）：
   - 当前：简单 Lerp 追赶
   - 未来：基于速度的平滑插值

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
