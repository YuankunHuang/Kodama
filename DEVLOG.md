# Kodama 开发日志 (Development Log)

**项目启动日期**: 2026-01-13  
**当前阶段**: Phase 0 - Clean Architecture 基础搭建  
**预计完成**: 2026-01-27 (2 周)

---

## 📋 Phase 0 进度追踪

| 任务 | 状态 | 完成日期 | 备注 |
|------|------|---------|------|
| 0.1 创建解决方案结构 | ✅ Completed | - | 4 个项目：Domain, Application, Infrastructure, API |
| 0.2 配置项目依赖 | ✅ Completed | - | 严格遵守依赖方向 |
| 0.3 实现 Domain 实体 | ⏳ Pending | - | Agent, HexCell, Position |
| 0.4 定义 Application 接口 | ⏳ Pending | - | IAgentRepository, ISimulationService |
| 0.5 配置 DI 容器 | ⏳ Pending | - | Program.cs 依赖注入 |
| 0.6 验证架构正确性 | ⏳ Pending | - | 依赖方向检查 |

**图例**: ⏳ Pending | 🔄 In Progress | ✅ Completed | ❌ Blocked

---

## 🎯 当前任务

### Phase 0.1: 创建 Clean Architecture 解决方案结构

**目标**: 建立 4 层架构的项目骨架

**执行步骤**:
1. 验证 .NET SDK 版本（需要 8.0+）
2. 创建解决方案文件 `Kodama.sln`
3. 创建 4 个项目：
   - `Kodama.Domain` (类库)
   - `Kodama.Application` (类库)
   - `Kodama.Infrastructure` (类库)
   - `Kodama.API` (Web API)
4. 配置项目依赖关系
5. 清理默认生成的示例文件

**关键学习点**:
- Clean Architecture 的依赖方向：`API -> Infrastructure -> Application -> Domain`
- 为什么 Infrastructure 不直接依赖 Domain（依赖倒置原则）

---

## 📝 技术决策记录 (ADR)

### ADR-001: 采用 Clean Architecture
**日期**: 2026-01-13  
**决策**: 使用 Clean Architecture 而非传统三层架构  
**理由**:
- Domain 层完全独立，便于单元测试
- 符合 SOLID 原则，特别是依赖倒置
- 易于扩展和维护（未来可能需要支持多种数据库）

### ADR-002: 使用 .NET 8
**日期**: 2026-01-13  
**决策**: 目标框架为 .NET 8（而非 .NET 9）  
**理由**:
- LTS 版本，生产环境更稳定
- 社区资源和文档更丰富
- 与设计文档中的技术栈一致

---

## 🐛 问题与解决方案

### 问题记录模板
### 2026-01-13 可空引用类型警告
**现象**: `CurrentPosition` 在构建结束前必须包含非空值
**原因**: C# 8.0+ 的可空引用类型检查，无参构造函数未初始化属性
**解决方案**: 使用 `= null!` 抑制警告，因为 EF Core 会在反序列化时初始化
**学到的教训**: 
- 理解 EF Core 的对象创建机制
- 掌握 null-forgiving operator 的使用场景
- 区分"业务代码创建对象"和"ORM 框架创建对象"

---

## 💡 学习笔记

### Clean Architecture vs Unity 架构对比

| 概念 | Unity | Clean Architecture |
|------|-------|-------------------|
| 依赖管理 | `GetComponent<T>()` | 构造函数注入 |
| 生命周期 | `Awake/Start/Update` | `IHostedService` |
| 数据访问 | 直接访问 `GameObject` | Repository 模式 |
| 业务逻辑 | `MonoBehaviour` 中混杂 | Domain 层纯粹 |

---

## 📚 参考资源

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft: Clean Architecture with ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)

---

## 🔄 更新日志

### 2026-01-13
- 项目启动
- 完成设计文档评审
- 制定 Phase 0 学习计划
- 创建开发日志文件

---

## 📌 下次开发提醒

**当前位置**: Phase 0.1 - 等待创建解决方案结构  
**下一步**: 执行 `dotnet new sln` 命令创建解决方案  
**需要准备**: 确保已安装 .NET 8 SDK

---

## 🎓 导师批注区

_此区域由技术导师填写关键反馈和建议_

### 架构审查检查点
- [ ] Domain 层是否有外部依赖？（必须为零）
- [ ] 依赖方向是否正确？
- [ ] 是否使用了依赖注入而非 `new` 关键字？
- [ ] 实体是否包含业务逻辑（富领域模型）？

---

**最后更新**: 2026-01-13 10:21 UTC  
**更新者**: Technical Mentor
