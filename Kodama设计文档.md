# Project Design Document: KODAMA (Project Sim-Forest)

**Version:** 1.0 **Type:** Distributed Real-Time Simulation / Infrastructure Showcase **Target Role:** Simulation & Infrastructure Engineer

## 1. 项目愿景 (Project Vision)

### 1.1 核心概念 (High Concept)

**"Kodama"** 是一个基于 **Server-Authoritative（服务器权威）** 架构的分布式生态仿真系统。 它模拟了一个由数千个自主代理（Agents）构成的“光之森林”生态。虽然其前端表现为唯美、治愈的 2.5D 生物发光世界，但其核心旨在展示**高并发状态同步**、**空间分区算法**、**资源竞争解决**以及**自动化扩张逻辑**等后端工程能力。

### 1.2 故事背景：光之协议 (The Protocol of Light)

在旧世界的数据荒原中，最后一个休眠的服务器集群被唤醒。作为管理员，你启动了“光之协议”。你的任务是通过投放初始的逻辑单元（Kodama），采集散落在内存深处的碎片（Matter），修复损坏的节点（Trees），让光之网络指数级扩张，最终点亮整个数字宇宙。

### 1.3 体验目标 (Experience Goals)

- **Visual:** 极简几何美学 + 极致的 Shader/VFX 表现（生物荧光、呼吸感）。
- **Technical:** 展示数千个单位流畅运行的宏大感（Swarm Intelligence）。
- **Emotional:** 从孤独的微光到辉煌的光之洪流，体验“复利增长”和“造物”的成就感。

------

## 2. 视觉与美术规范 (Visual & Art Direction)

**关键词：** `Bioluminescence` (生物荧光), `Minimalist` (极简), `Cyber-Organic` (赛博有机), `Dark Mode` (深色模式).

### 2.1 视角与相机 (Camera)

- **类型：** Fixed Isometric / Top-Down (固定上帝视角)。
- **行为：** 支持平滑缩放。随着世界边界扩张，相机最大高度增加，允许观察宏观光流。

### 2.2 资产与模型 (Assets)

- **风格：** **无纹理 (No Textures)**，完全依赖几何体形状 + 着色器。
- **Kodama (代理):** 低多边形球体 (Low Poly Sphere)。使用顶点位移 Shader 模拟软体/水滴的呼吸感。
- **Trees (节点):** 抽象的晶体簇或发光的几何柱体。
- **Terrain (地图):** 深色六边形 (Hexagon) 地块。边缘有微弱的发光线。

### 2.3 特效与后处理 (VFX & Post-Processing)

- **Bloom:** **必须开启且高强度**。让所有亮色物体在深色背景上产生光晕。
- **Trails:** 每个 Kodama 移动时留下长长的光尾（使用 Unity Trail Renderer），形成流动的光线网络。
- **Color Palette:**
  - **背景:** 深午夜蓝 (#050510).
  - **Kodama:** 纯白高亮 (#FFFFFF).
  - **资源:** 青色 (#00FFFF).
  - **生命/母树:** 暖金色 (#FFD700).

------

## 3. 核心机制 (Core Mechanics)

### 3.1 地图系统 (Map System)

- **结构：** 无限扩展的六边形网格 (Axial Coordinate System)。
- **状态：**
  - **Void (虚空):** 不可通行，黑暗。
  - **Active (激活):** 可通行，被点亮。
- **扩张逻辑：** 当“种植者”在边缘牺牲自己变为新树时，周围半径 N 的 Hex 格子从 Void 变为 Active（伴随浮起动画）。

### 3.2 代理系统 (Agent System - The "Kodama")

所有逻辑在后端运行，前端仅负责插值渲染。

- **Gatherer (采集者 - 白色):**
  - **FSM (状态机):** `Idle` -> `FindResource` -> `MoveToTarget` -> `Collect` -> `ReturnToBase` -> `Deposit`.
  - **特性:** 数量最多，速度快，负责搬运 Matter。
- **Planter (种植者 - 绿色):**
  - **FSM:** `Idle` -> `FindEdge` -> `MoveToEdge` -> `Plant (Die & Spawn Tree)`.
  - **消耗:** 生产成本极高。
- **Cleaner (净化者 - 蓝色):**
  - **FSM:** `Patrol` -> `ChaseCorruption` -> `Purify`.

### 3.3 资源循环 (Economy Loop)

1. **Lumen (光):** 树木每秒自动产出。作为 OpEx (运营成本) 被所有 Agent 每秒消耗。光不足 -> Agent 死亡。
2. **Matter (物质):** 地图随机刷新矿点。需由 Gatherer 搬运。作为 CapEx (资本支出) 用于生产新 Agent 或升级树木。

------

## 4. 系统架构 (System Architecture) - **关键部分**

### 4.1 技术栈 (Tech Stack)

- **Backend:** ASP.NET Core 8.0/9.0 (C#).
- **Frontend:** Unity 6 (C#).
- **Communication:**
  - **Real-time:** SignalR (WebSockets) + MessagePack/Protobuf (二进制压缩)。
  - **Meta:** REST API (Login, Config).
- **Database:** Redis (Hot Data/GameState), PostgreSQL (User Data/Replay Logs).
- **Infra:** Docker, GitHub Actions.

### 4.2 后端核心模块 (Backend Modules)

1. **Simulation Loop (Hosted Service):**
   - 固定 Tick Rate (e.g., 10Hz or 20Hz)。
   - 每帧执行：`UpdateAgents()` -> `CheckCollisions()` -> `RegenerateResources()` -> `BroadcastSnapshot()`.
2. **Spatial Partitioning (空间分区):**
   - 实现 **Spatial Hash Grid** 或 **QuadTree**。
   - 用于 O(1) 或 O(log n) 复杂度查询“我周围有哪些单位”以及“最近的资源在哪里”。
3. **Concurrency Control (并发控制):**
   - 使用 `Interlocked` 或 Redis Distributed Lock 处理资源竞争（防止两个 Agent 吃掉同一个资源）。

### 4.3 网络协议设计 (Protocol Design)

为了节省带宽，Snapshot 数据结构应极为精简：

C#

```
// 伪代码示例
struct SnapshotPacket {
    long Timestamp;
    List<AgentData> Agents; // 包含 ID, Position(x,y), StateEnum
    List<EventData> Events; // 包含 "TreeSpawned", "EffectTriggered"
}
```

------

## 5. 前端架构 (Client Architecture)

### 5.1 表现层 (Presentation Layer)

- **Dumb Client:** 客户端不计算任何游戏逻辑（不判断谁吃到了资源），只负责“画”。
- **Interpolation (插值):** 服务器 10Hz，客户端 60Hz。
  - 使用 `Vector3.Lerp` 在上一个快照和当前快照之间平滑过渡。
  - 实现 **Entity Interpolation Buffer**。

### 5.2 性能优化 (Optimization)

- **GPU Instancing:** 渲染数千个相同的 Kodama 和 Hex 地块，必须使用 `DrawMeshInstanced` 或相关的 Shader 技术。
- **Object Pooling:** 严格的对象池管理，杜绝运行时的 `Instantiate` 和 `Destroy`。

------

## 6. 管理与监控 (Admin & Infra)

### 6.1 Web Dashboard (God Console)

一个独立的 Web 页面 (Vue/React)，用于展示你的 Full-stack/Infra 能力。

- **Live Charts:** 实时显示 QPS, Agent Count, Memory Usage.
- **Tunables:** 滑块控制 `GlobalGravity` (引力), `SpawnRate` (刷新率)。拖动滑块，Unity 内世界物理规则实时改变。

------

## 7. 开发路线图 (Development Roadmap)

### Phase 1: The Pulse (MVP) - 预计耗时: 3天

- **目标:** 后端跑通一个 Agent 的圆形运动，Unity 端平滑渲染。
- **产出:** Hello World 级别的联调。

### Phase 2: The Loop (核心循环) - 预计耗时: 1周

- **目标:** 引入资源和树。
- **逻辑:** Agent 可以在后端寻找最近资源，移动，并运回。
- **视觉:** 简单的发光球体和方块。

### Phase 3: The Swarm (规模化) - 预计耗时: 2周

- **目标:** 500+ Agents 并发。
- **技术:** 引入空间哈希网格，优化带宽 (Protobuf)。
- **视觉:** 加入 Trail Renderer 和 Bloom，实现“光流”效果。

### Phase 4: The Polish (打磨) - 预计耗时: 1周

- **目标:** 视觉升级与 Web 控制台。
- **内容:** 加入“光之协议”的故事包装，音效，以及 Docker 部署。

------

### 如何使用这份文档？

- **发给 Claude/ChatGPT:** "请根据这份设计文档的 Phase 1 要求，为我生成 ASP.NET Core 的 Agent Class 基础代码和 Unity 端的插值移动脚本。"
- **发给美术/Shader AI:** "基于 Visual Direction 章节，帮我写一个 Unity URP Shader Graph 节点的描述，实现‘边缘发光的果冻球体’效果。"