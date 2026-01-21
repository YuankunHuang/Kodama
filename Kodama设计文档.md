# Project Design Document: KODAMA

**Version:** 2.0 (Engineering Showcase Edition)  
**Type:** Distributed Real-Time Simulation / Infrastructure Showcase  
**Platform:** Unity 6 + ASP.NET Core 8  
**Target Role:** Senior Simulation & Infrastructure Engineer

---

## 1. 项目愿景 (Project Vision)

### 1.1 核心概念 (High Concept)

**"Kodama"** 是一个基于 **Server-Authoritative（服务器权威）** 架构的分布式生态仿真系统。它模拟了一个由数万个自主代理（Agents）构成的"光之森林"生态。

**核心工程目标**：构建一个**可观测、可验证、高性能**的分布式仿真沙盒，展示在以下领域的深厚造诣：
- **High Concurrency (高并发)**：单服承载 10,000+ 动态代理
- **Data-Oriented Design (面向数据设计)**：零分配 Tick 循环
- **Real-Time Networking (实时网络)**：Server-Authoritative 状态同步

### 1.2 故事背景：光之协议 (The Protocol of Light)

在旧世界的数据荒原中，最后一个休眠的服务器集群被唤醒。作为管理员，你启动了"光之协议"。

你的任务是通过投放初始的逻辑单元（Kodama），采集散落在内存深处的碎片（Matter），修复损坏的节点（Trees），让光之网络指数级扩张，最终点亮整个数字宇宙。

### 1.3 体验目标 (Experience Goals)

| 维度 | 目标 |
|------|------|
| **Visual** | 极简几何美学 + 极致的 Shader/VFX 表现（Bloom、Trail、Emission） |
| **Technical** | 10,000+ 动态代理 @ 60Hz，零分配稳态运行，系统内部状态完全可观测 |
| **Emotional** | 体验从混沌到秩序的"造物"成就感 |

---

## 2. 视觉与美术规范 (Visual & Art Direction)

**关键词：** `Bioluminescence` (生物荧光), `Minimalist` (极简), `Cyber-Organic` (赛博有机), `Dark Mode` (深色模式)

### 2.1 视角与相机 (Camera)

- **类型**：Fixed Isometric / Top-Down（固定上帝视角）
- **行为**：支持平滑缩放，随世界边界扩张调整最大高度

### 2.2 资产与模型 (Assets)

| 资产 | 风格 | 说明 |
|------|------|------|
| **Kodama (代理)** | 低多边形球体 | 顶点位移 Shader 模拟软体/水滴呼吸感 |
| **Trees (节点)** | 抽象晶体簇 / 发光几何柱体 | 中心能量源 |
| **Terrain (地图)** | 深色六边形地块 | 边缘微弱发光线 |

**原则**：**无纹理 (No Textures)**，完全依赖几何体形状 + 着色器。

### 2.3 特效与后处理 (VFX & Post-Processing)

| 效果 | 说明 |
|------|------|
| **Bloom** | 高强度，让亮色物体在深色背景上产生光晕 |
| **Trails** | Kodama 移动时留下光尾，形成流动的光线网络 |
| **Color Palette** | 背景 #050510 / Kodama #FFFFFF / 资源 #00FFFF / 母树 #FFD700 |

---

## 3. 核心机制 (Core Mechanics)

### 3.1 地图系统 (Map System)

- **结构**：无限扩展的六边形网格（Axial Coordinate System）
- **状态**：
  - `Void (虚空)`：不可通行，黑暗
  - `Active (激活)`：可通行，被点亮
- **扩张逻辑**：种植者在边缘牺牲变为新树时，周围半径 N 的格子从 Void 变为 Active

### 3.2 代理系统 (Agent System - The "Kodama")

所有逻辑在后端运行，前端仅负责插值渲染。

| 类型 | 颜色 | 状态机 |
|------|------|--------|
| **Gatherer (采集者)** | 白色 | `Idle` → `FindResource` → `MoveToTarget` → `Collect` → `ReturnToBase` → `Deposit` |
| **Planter (种植者)** | 绿色 | `Idle` → `FindEdge` → `MoveToEdge` → `Plant (Die & Spawn Tree)` |
| **Cleaner (净化者)** | 蓝色 | `Patrol` → `ChaseCorruption` → `Purify` |

### 3.3 资源循环 (Economy Loop)

| 资源 | 来源 | 用途 |
|------|------|------|
| **Lumen (光)** | 树木每秒产出 | OpEx：Agent 生存消耗 |
| **Matter (物质)** | 地图随机刷新矿点 | CapEx：生产 Agent / 升级树木 |

---

## 4. 系统架构 (System Architecture)

### 4.1 技术栈 (Tech Stack)

| 层级 | 技术 |
|------|------|
| **Backend** | ASP.NET Core 8.0 (C#) |
| **Frontend** | Unity 6 (C#) |
| **Communication** | SignalR (WebSockets) + JSON/MessagePack |
| **Infrastructure** | Docker Compose, GitHub Actions |

### 4.2 后端核心模块 (Backend Modules)

#### 4.2.1 Simulation Loop (Hosted Service)

- **Fixed Timestep**：严格固定步长（如 100ms），游戏逻辑时间与执行时间解耦
- **Time Budget Control**：Tick 超时自动降级或报警
- **Catch-up & Reset**：落后时追赶，落后过多时重置时间

#### 4.2.2 World State (状态管理)

- **Data-Oriented Design**：Agent 使用 `struct`，避免引用类型分散分配
- **Zero-Allocation Tick**：使用预分配 List + `Clear()` 重用，避免每帧 `new`
- **Spatial Indexing**：按位置索引 Agent/Resource，支持 O(1) 邻居查询

#### 4.2.3 Agent Behaviour Service

- **上帝视角处理**：系统批量处理同状态的 Agent，而非让每个 Agent 自己做决定
- **状态机驱动**：每个 Agent 根据当前状态执行对应逻辑

### 4.3 前端核心模块 (Client Modules)

#### 4.3.1 Dumb Client

- 客户端只负责渲染快照，不做任何游戏逻辑
- 实现 Snapshot Buffer，处理网络抖动（Jitter）

#### 4.3.2 GPU Instancing

- 使用 `Graphics.DrawMeshInstanced` 或 `RenderMeshIndirect`
- 单 Draw Call 渲染 10,000+ 单位

#### 4.3.3 Debug Visualization

- **Grid View**：绘制 Spatial Hash Grid 网格线
- **Path Gizmos**：实时绘制 Agent 寻路路径
- **Server Ghost**：半透明显示服务器真实位置，对比客户端插值位置

### 4.4 架构决策记录 (ADRs)

| 决策 | 选择 | 理由 |
|------|------|------|
| 通信协议 | SignalR (WebSocket) | Web 兼容性，快速开发，MessagePack 弥补带宽 |
| 状态同步 | Server-Authoritative | 防作弊，状态一致性 |
| Agent 数据结构 | struct + List 重用 | 零分配，Cache-friendly |
| 时间步长 | Fixed Timestep | 确定性模拟，可回放 |

---

## 5. 性能目标 (Performance Targets)

| 指标 | 目标 |
|------|------|
| **Agent 规模** | 10,000+ @ 60Hz |
| **Tick 耗时** | < 5ms (稳态) |
| **GC 分配** | < 1KB per Tick (稳态) |
| **RTT 延迟** | < 50ms (Localhost), < 100ms (Cloud) |
| **客户端 FPS** | 60 FPS @ 10,000 单位 |

---

## 6. 开发里程碑 (Development Milestones)

### Milestone 1: Architecture Skeleton

- 搭建 .NET 8 + SignalR + Unity 通信闭环
- 产出：C4 架构图初稿

### Milestone 2: Core Loop

- 实现资源采集循环
- 产出：后端 Tick 运行，前端能接收 Snapshot

### Milestone 3: Performance Optimization

- 引入 Spatial Indexing
- 重构为零分配模式
- 产出：10,000 Agent 压力测试通过

### Milestone 4: Visualization

- GPU Instancing 渲染
- Shader/VFX 美术打磨
- 产出：Demo 视频

### Milestone 5: Polish & Packaging

- Debug Visualization
- README / Benchmarks 文档
- 产出：可部署的完整 Demo

---

## 7. 扩展方向 (Future Extensions)

| 方向 | 说明 |
|------|------|
| **Interest Management** | 只同步玩家视野内的 Agent，支持 100k+ 规模 |
| **Spatial Hashing** | 实现 O(1) 邻居查询，优化碰撞/寻路 |
| **Distributed Simulation** | 多服务器分担负载 |
| **Deterministic Replay** | 锁定随机种子，支持录像回放 |
