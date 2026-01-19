# Project Design Document: KODAMA (Project Sim-Forest)

**Version:** 1.1 (Engineering Showcase Edition) **Type:** Distributed Real-Time Simulation / Infrastructure Showcase **Target Role:** Senior/Staff Simulation & Infrastructure Engineer

## 1. 项目愿景 (Project Vision)

### 1.1 核心概念 (High Concept)

**"Kodama"** 是一个基于 **Server-Authoritative（服务器权威）** 架构的分布式生态仿真系统。它模拟了一个由数千个自主代理（Agents）构成的“光之森林”生态。 **核心工程目标**：不仅仅是创造一个唯美的画面，而是构建一个**可观测、可验证、高性能**的分布式仿真沙盒，用于展示在 **High Concurrency (高并发)**、**Data-Oriented Design (面向数据设计)** 以及 **Microservices Architecture (微服务架构)** 领域的深厚造诣。

### 1.2 故事背景：光之协议 (The Protocol of Light)

在旧世界的数据荒原中，最后一个休眠的服务器集群被唤醒。作为管理员，你启动了“光之协议”。你的任务是通过投放初始的逻辑单元（Kodama），采集散落在内存深处的碎片（Matter），修复损坏的节点（Trees），让光之网络指数级扩张，最终点亮整个数字宇宙。

### 1.3 体验目标 (Experience Goals)

- **Visual:** 极简几何美学 + 极致的 Shader/VFX 表现。
- **Technical (Updated):**
  - **Massive Scale:** 展示单服承载 **5,000+** 动态代理的负载能力。
  - **Observability:** 系统的内部状态（如空间划分、寻路网格、服务器Tick耗时）对开发者完全透明。
- **Emotional:** 体验从混沌到秩序的“造物”成就感。

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

## 4. 系统架构 (System Architecture) - **核心展示区**

### 4.1 技术栈 (Tech Stack)

- **Backend:** ASP.NET Core 8.0 (C#).
- **Frontend:** Unity 6 (C#).
- **Communication:** SignalR (WebSockets) + MessagePack (Zero-allocation serialization).
- **Infra:** Docker Compose, GitHub Actions.

### 4.2 后端核心模块 (Backend Modules)

1. **Simulation Loop (Hosted Service):**
   - 严格的时间预算控制 (Time Budgeting)。如果 Tick 耗时超过阈值 (e.g., 50ms)，触发自动降级或报警。
2. **Spatial Partitioning (空间分区):**
   - 实现 **Spatial Hash Grid**。
   - **[Showcase Point]**: 对比测试“暴力遍历 (O(N^2))”与“空间哈希 (O(N))”的性能差异，并生成图表。
3. **Data Structures (数据结构):**
   - 使用 `Span<T>` 和 `ArrayPool<T>` 进行内存管理，追求 **Zero-Allocation**。

### 4.3 架构决策文档 (ADRs & Visualization) - **[NEW]**

为了体现架构师思维，项目中必须包含以下文档产出：

- **C4 Model Diagrams:** 绘制 Context, Container, Component 三层架构图，清晰展示 Unity Client, Sim Server, Redis, Dashboard 之间的关系。
- **Decision Records (决策记录):**
  - *Why SignalR vs UDP?* (解释：为了Web兼容性与快速开发 vs 牺牲部分延迟，以及如何应用 MessagePack 弥补带宽)。
  - *Why Deterministic?* (解释：锁帧同步的必要性与浮点数处理策略)。

------

## 5. 前端架构 (Client Architecture)

### 5.1 表现层 (Presentation Layer)

- **Dumb Client:** 客户端只负责渲染快照。
- **Interpolation:** 实现 Snapshot Buffer，处理网络抖动 (Jitter)。

### 5.2 开发者调试视图 (Debug Visualization) - **[NEW]**

为了证明“所见即所得”的控制力，客户端必须包含一个 **"Debug Overlay"** 开关：

- **Grid View:** 在游戏世界中画出后端的 Spatial Hash Grid 网格线。
- **Path Gizmos:** 实时画出选中 Agent 的服务器寻路路径。
- **Server Ghost:** 用半透明红色线框显示 Agent 在服务器上的真实位置，与客户端插值位置做对比（直观展示 Lag Compensation）。

### 5.3 性能优化 (Optimization)

- **GPU Instancing:** 必须使用。
- **Benchmark Mode:** 客户端内置“压力测试”按钮，一键生成 10,000 个静态单位，展示 FPS 依然稳定。

------

## 6. 工程化展示 (Engineering Showcase) - **[重构章节]**

本章节取代原本的“管理与监控”，旨在集中展示**硬核工程能力**。

### 6.1 性能基准测试 (Hardcore Metrics)

在 README 或专门的 `BENCHMARKS.md` 中展示以下数据：

- **Throughput:** "单核支持 5,000 Agents @ 20Hz Tick Rate."
- **Latency:** "平均 RTT < 50ms (Localhost), < 100ms (AWS)."
- **Memory:** "GC Allocation < 1KB per Tick (Steady State)." —— **这是杀手级指标。**
- **Optimization Comparison:** 附带优化前后的 Profiler 截图对比（例如：改为 Struct 后的内存变化）。

### 6.2 实时监控面板 (The "God Console")

Web Dashboard 不仅是游戏控制台，更是**服务器健康监视器**：

- **Real-time Metrics:** 使用图表库 (Chart.js/ECharts) 实时绘制 Tick Duration (ms) 曲线。
- **Heatmap:** 在网页上绘制 Agent 密度热力图，证明后端空间算法的正确性。

### 6.3 自动化与质量 (CI/CD)

- **Build Pipeline:** GitHub Actions 自动构建 Docker 镜像。
- **Unit Tests:** 针对核心算法（如空间哈希、碰撞检测）编写单元测试，并显示覆盖率。

------

## 7. 开发路线图 (Development Roadmap)

### Phase 1: Architecture Skeleton (3 Days)

- 搭建 .NET 8 + SignalR + Unity 通信闭环。
- **产出:** C4 架构图初稿。

### Phase 2: Core Loop & Visualization (1 Week)

- 实现资源采集循环。
- **产出:** Unity 客户端的 Debug Gizmos (网格/路径显示)。

### Phase 3: Performance & Optimization (2 Weeks) - **[Critical]**

- 引入 Spatial Hash Grid。
- 重构代码以使用 `Span<T>` 和对象池。
- **产出:** 压力测试，录制 "10,000 Agents" 的演示视频，截取 Profiler 数据。

### Phase 4: Polish & Portfolio Packaging (1 Week)

- Bloom/VFX 美术打磨。
- 撰写 README，整理 Benchmarks 图表。
- 部署 Demo。

------

### 给 AI 助手的 Prompt 示例 (Updated)

- **架构:** "我需要为 Kodama 绘制 C4 模型图。请根据 Mermaid 语法，帮我生成 System Context 和 Container 两个层级的代码。"
- **性能:** "我正在优化 C# 的 Spatial Grid。请帮我把这个使用 `List<T>` 的类重构为使用 `ArrayPool` 和 `ref struct` 的零分配版本。"
- **测试:** "请为我的 `CollisionSystem.cs` 写一个单元测试，验证在高密度重叠下的边界情况。"