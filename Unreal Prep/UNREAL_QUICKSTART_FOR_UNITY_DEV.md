# Unreal Engine 5 快速上手指南
## 为 Unity/C# 开发者编写 | 目标：StadiumSim 项目

**读者画像**：精通 Unity C#，熟悉 Server-Authoritative 架构、Data-Oriented Design、零分配优化。即将开始 UE5 C++ + MassEntity 开发。

**阅读时间**：60-90 分钟（建议分段阅读）

**目标**：读完后能直接开始 StadiumSim 开发，不需要再看其他教程。

---

## 目录

1. [思维转换：Unity → Unreal 的关键差异](#1-思维转换unity--unreal-的关键差异)
2. [C++ 生存指南：Unity C# 开发者需要知道的](#2-c-生存指南unity-c-开发者需要知道的)
3. [UE5 核心概念速查](#3-ue5-核心概念速查)
4. [MassEntity 深度解析](#4-massentity-深度解析)
5. [StadiumSim 实战路线图](#5-stadiumsim-实战路线图)
6. [常见坑 & 避雷指南](#6-常见坑--避雷指南)
7. [速查表 & 备忘](#7-速查表--备忘)

---

## 1. 思维转换：Unity → Unreal 的关键差异

### 1.1 哲学差异

| 维度 | Unity | Unreal |
|------|-------|--------|
| **设计哲学** | 轻量级，自己搭建 | 重量级，框架齐全 |
| **默认假设** | 你要做 2D/手游/独立游戏 | 你要做 3A/大型项目 |
| **代码风格** | 简洁灵活，自由度高 | 严格约定，Epic 风格 |
| **编译速度** | 热重载秒级 | C++ 编译分钟级（习惯它） |

### 1.2 命名映射

| Unity | Unreal | 说明 |
|-------|--------|------|
| `GameObject` | `AActor` | 场景中的实体 |
| `MonoBehaviour` | `UActorComponent` | 挂在 Actor 上的组件 |
| `ScriptableObject` | `UDataAsset` | 纯数据容器 |
| `Prefab` | `Blueprint Class` | 可实例化的预制件 |
| `Scene` | `Level` / `World` | 场景/关卡 |
| `Project Settings` | `DefaultEngine.ini` + Editor | 项目配置 |
| `Assembly Definition` | `Module` | 代码模块化单元 |
| `Package Manager` | `Plugins` | 第三方插件 |

### 1.3 生命周期对比

```
Unity                          Unreal
─────                          ──────
Awake()                   →    Constructor (不要在这里写逻辑！)
OnEnable()                →    BeginPlay()
Start()                   →    BeginPlay()
Update()                  →    Tick(float DeltaTime)
FixedUpdate()             →    Tick() + 自己实现 Fixed Timestep
OnDisable()               →    EndPlay()
OnDestroy()               →    ~Destructor (C++ 析构)
```

**关键差异**：
- Unreal 的 `Tick()` 默认每帧调用，类似 Unity 的 `Update()`
- 没有 `FixedUpdate()` 等效物，需要自己实现固定步长
- **构造函数不要写游戏逻辑**！Unreal 的对象可能在编辑器中就被构造

### 1.4 坐标系统

| 维度 | Unity | Unreal |
|------|-------|--------|
| **单位** | 1 单位 ≈ 1 米（约定） | 1 单位 = 1 厘米（强制） |
| **前方** | +Z | +X |
| **上方** | +Y | +Z |
| **左手/右手** | 左手坐标系 | 左手坐标系 |

**实际影响**：
- Unity 里 `scale = 1` 的物体，在 Unreal 里要 `scale = 100`
- 朝向计算从 `transform.forward (+Z)` 变成 `GetActorForwardVector() (+X)`

---

## 2. C++ 生存指南：Unity C# 开发者需要知道的

### 2.1 你已经会的（C# → C++ 直译）

```cpp
// ============ 变量声明 ============
// C#: int count = 10;
int32 Count = 10;                    // UE 风格用 int32 而非 int

// C#: string name = "Agent";
FString Name = TEXT("Agent");        // UE 字符串类型

// C#: List<int> numbers = new List<int>();
TArray<int32> Numbers;               // UE 动态数组

// C#: Dictionary<string, int> map = new Dictionary<string, int>();
TMap<FString, int32> Map;            // UE 字典

// ============ 类定义 ============
// C#: public class Agent : MonoBehaviour { }
UCLASS()
class STADIUMSIM_API ASpectator : public AActor
{
    GENERATED_BODY()
public:
    // 成员变量和函数
};

// ============ 属性 ============
// C#: [SerializeField] private float speed;
UPROPERTY(EditAnywhere, Category = "Movement")
float Speed;

// C#: public float Speed { get; private set; }
// C++ 没有属性语法，用 getter/setter 或直接公开
float GetSpeed() const { return Speed; }
```

### 2.2 核心差异：指针与内存

**C# 的世界**：一切都是引用，GC 自动管理内存

**C++ 的世界**：你要区分三种情况

```cpp
// 1. 栈分配（值类型，离开作用域自动销毁）
FVector Position = FVector(0, 0, 0);   // 类似 C# 的 struct
int32 Count = 10;

// 2. 堆分配（需要手动管理，或用 UE 智能指针）
// 原始指针（危险，尽量避免）
AActor* RawPtr = GetWorld()->SpawnActor<AActor>(...);
// 什么时候删除？谁负责删除？容易出错！

// 3. UE 托管指针（推荐！）
UPROPERTY()
AActor* ManagedPtr;  // 被 UPROPERTY 标记的指针由 UE GC 管理
```

**黄金法则**：
1. **优先用值类型**：`FVector`, `FRotator`, `FTransform` 等都是值类型
2. **UObject 派生类用 `UPROPERTY()`**：这样 UE 的 GC 会管理它
3. **非 UObject 用 `TSharedPtr` / `TUniquePtr`**：UE 的智能指针

### 2.3 头文件 vs 源文件

**C# 思维**：一个类一个文件，声明和实现在一起

**C++ 规则**：声明和实现分离

```cpp
// ========== Spectator.h (头文件 - 声明) ==========
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Spectator.generated.h"  // UE 反射必须

UCLASS()
class STADIUMSIM_API ASpectator : public AActor
{
    GENERATED_BODY()
    
public:
    ASpectator();
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;
    
private:
    UPROPERTY()
    float Health;
    
    void ProcessAI();  // 只声明，不实现
};

// ========== Spectator.cpp (源文件 - 实现) ==========
#include "Spectator.h"

ASpectator::ASpectator()
{
    PrimaryActorTick.bCanEverTick = true;  // 启用 Tick
}

void ASpectator::BeginPlay()
{
    Super::BeginPlay();  // 调用父类
    // 初始化逻辑
}

void ASpectator::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
    ProcessAI();
}

void ASpectator::ProcessAI()
{
    // 实现细节
}
```

### 2.4 UE 反射系统：UCLASS / UPROPERTY / UFUNCTION

```cpp
// UCLASS - 让类被 UE 反射系统识别
UCLASS(Blueprintable)  // Blueprintable = 可在蓝图中创建子类
class AMyActor : public AActor { ... };

// UPROPERTY - 让成员变量被 UE 识别
UPROPERTY(EditAnywhere)       // 编辑器可见可编辑
UPROPERTY(VisibleAnywhere)    // 编辑器可见不可编辑
UPROPERTY(BlueprintReadWrite) // 蓝图可读写
UPROPERTY(Transient)          // 不序列化（临时数据）

// UFUNCTION - 让函数被 UE 识别
UFUNCTION(BlueprintCallable)  // 蓝图可调用
UFUNCTION(BlueprintPure)      // 蓝图纯函数（无副作用）
```

**记住**：没有 `UPROPERTY()` 的指针不会被 GC 追踪，可能变成悬空指针！

### 2.5 常用类型速查

| C# | C++ (Standard) | C++ (Unreal) | 说明 |
|----|----------------|--------------|------|
| `int` | `int` | `int32` | 32位整数 |
| `float` | `float` | `float` | 单精度浮点 |
| `double` | `double` | `double` | 双精度浮点 |
| `bool` | `bool` | `bool` | 布尔 |
| `string` | `std::string` | `FString` | 动态字符串 |
| `Vector3` | — | `FVector` | 3D 向量 |
| `Quaternion` | — | `FQuat` | 四元数 |
| `Transform` | — | `FTransform` | 变换 |
| `List<T>` | `std::vector<T>` | `TArray<T>` | 动态数组 |
| `Dictionary<K,V>` | `std::unordered_map` | `TMap<K,V>` | 哈希表 |
| `HashSet<T>` | `std::unordered_set` | `TSet<T>` | 哈希集合 |
| `Guid` | — | `FGuid` | 唯一标识符 |

---

## 3. UE5 核心概念速查

### 3.1 游戏框架层次

```
UGameInstance          // 整个游戏生命周期（类似 Unity 的 DontDestroyOnLoad 单例）
    │
    └── UWorld         // 当前加载的世界（包含所有 Level）
         │
         └── AGameMode // 游戏规则（仅服务器）
              │
              └── APlayerController // 玩家输入控制器
                   │
                   └── APawn / ACharacter // 玩家控制的实体
```

**对于 StadiumSim**：你主要关心 `UWorld` 和 `AActor`，不需要玩家控制。

### 3.2 Actor & Component

```cpp
// Actor = 场景中的实体（类似 Unity GameObject）
UCLASS()
class ASpectator : public AActor
{
    GENERATED_BODY()

public:
    ASpectator()
    {
        // 添加组件
        MeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
        RootComponent = MeshComponent;
    }

private:
    UPROPERTY(VisibleAnywhere)
    UStaticMeshComponent* MeshComponent;
};
```

**但是！StadiumSim 不用这种方式！**

10 万个 `AActor` = 10 万个对象 = 内存爆炸 + 性能灾难

我们用 **MassEntity**——下一节详解。

### 3.3 蓝图 vs C++

| 场景 | 推荐 |
|------|------|
| 快速原型 / UI | 蓝图 |
| 性能关键代码 | C++ |
| 系统架构 | C++ |
| 美术/设计师调参 | 蓝图继承 C++ 基类 |

**StadiumSim 策略**：
- 核心 Processor 用 C++
- 调试/测试可以用蓝图快速迭代

---

## 4. MassEntity 深度解析

### 4.1 什么是 MassEntity？

MassEntity 是 UE5 的 **ECS-like 框架**，专为大规模 Agent 设计。

| ECS 术语 | MassEntity 术语 | 对应物 |
|----------|-----------------|--------|
| Entity | Entity (FMassEntityHandle) | 一个 ID，不是对象 |
| Component | Fragment (FMassFragment) | 纯数据 struct |
| System | Processor (UMassProcessor) | 处理逻辑 |
| Archetype | Archetype | 具有相同 Fragment 组合的实体集合 |

**核心思想**（你已经懂了）：
- Entity 只是一个 ID
- Fragment 是纯数据（类似你后端的 `struct Agent`）
- Processor 批量处理同类型 Entity（类似你的 `AgentBehaviourService`）

### 4.2 Fragment 定义

```cpp
// ========== SpectatorFragments.h ==========
#pragma once

#include "MassEntityTypes.h"
#include "SpectatorFragments.generated.h"

// 状态枚举
UENUM()
enum class ESpectatorState : uint8
{
    Seated,
    Standing,
    Cheering,
    Dejected,
    Wave,
    Leaving
};

// 观众状态 Fragment
USTRUCT()
struct STADIUMSIM_API FSpectatorStateFragment : public FMassFragment
{
    GENERATED_BODY()

    ESpectatorState State = ESpectatorState::Seated;
    float StateTimer = 0.0f;  // 当前状态持续时间
};

// 座位信息 Fragment
USTRUCT()
struct STADIUMSIM_API FSeatFragment : public FMassFragment
{
    GENERATED_BODY()

    int32 SeatIndex = 0;
    int32 SectorID = 0;
    FVector SeatWorldPosition = FVector::ZeroVector;
};

// 队伍信息 Fragment
USTRUCT()
struct STADIUMSIM_API FTeamFragment : public FMassFragment
{
    GENERATED_BODY()

    uint8 TeamID = 0;  // 0 = Home, 1 = Away, 2 = Neutral
};

// 程序化动画 Fragment
USTRUCT()
struct STADIUMSIM_API FAnimationFragment : public FMassFragment
{
    GENERATED_BODY()

    float Phase = 0.0f;      // 动画相位（用于错开）
    float Amplitude = 1.0f;  // 动画幅度
    float Height = 0.0f;     // 当前高度偏移（站/坐）
};
```

**关键点**：
- `FMassFragment` 是纯数据 struct，没有任何逻辑
- 继承自 `FMassFragment`
- 使用 `USTRUCT()` 和 `GENERATED_BODY()` 宏

### 4.3 Processor 定义

```cpp
// ========== SpectatorStateProcessor.h ==========
#pragma once

#include "MassProcessor.h"
#include "SpectatorStateProcessor.generated.h"

UCLASS()
class STADIUMSIM_API USpectatorStateProcessor : public UMassProcessor
{
    GENERATED_BODY()

public:
    USpectatorStateProcessor();

protected:
    // 配置这个 Processor 需要哪些 Fragment
    virtual void ConfigureQueries() override;
    
    // 每帧执行
    virtual void Execute(FMassEntityManager& EntityManager, FMassExecutionContext& Context) override;

private:
    FMassEntityQuery EntityQuery;  // 查询器
};

// ========== SpectatorStateProcessor.cpp ==========
#include "SpectatorStateProcessor.h"
#include "MassExecutionContext.h"
#include "MassEntityManager.h"
#include "SpectatorFragments.h"

USpectatorStateProcessor::USpectatorStateProcessor()
{
    // 设置执行阶段
    ExecutionOrder.ExecuteInGroup = UE::Mass::ProcessorGroupNames::Behavior;
}

void USpectatorStateProcessor::ConfigureQueries()
{
    // 声明需要访问的 Fragment
    EntityQuery.AddRequirement<FSpectatorStateFragment>(EMassFragmentAccess::ReadWrite);
    EntityQuery.AddRequirement<FTeamFragment>(EMassFragmentAccess::ReadOnly);
    EntityQuery.AddRequirement<FTransformFragment>(EMassFragmentAccess::ReadWrite);
    
    // 注册查询
    EntityQuery.RegisterWithProcessor(*this);
}

void USpectatorStateProcessor::Execute(FMassEntityManager& EntityManager, FMassExecutionContext& Context)
{
    // 批量处理所有匹配的 Entity
    EntityQuery.ForEachEntityChunk(EntityManager, Context,
        [this](FMassExecutionContext& Context)
        {
            // 获取 Fragment 数组
            const int32 NumEntities = Context.GetNumEntities();
            
            TArrayView<FSpectatorStateFragment> StateFragments = 
                Context.GetMutableFragmentView<FSpectatorStateFragment>();
            TConstArrayView<FTeamFragment> TeamFragments = 
                Context.GetFragmentView<FTeamFragment>();
            TArrayView<FTransformFragment> TransformFragments = 
                Context.GetMutableFragmentView<FTransformFragment>();

            const float DeltaTime = Context.GetDeltaTimeSeconds();

            // 遍历当前 Chunk 中的所有 Entity
            for (int32 i = 0; i < NumEntities; ++i)
            {
                FSpectatorStateFragment& State = StateFragments[i];
                const FTeamFragment& Team = TeamFragments[i];
                FTransformFragment& Transform = TransformFragments[i];

                // 更新状态计时器
                State.StateTimer += DeltaTime;

                // 状态机逻辑
                switch (State.State)
                {
                case ESpectatorState::Seated:
                    // 坐着时的逻辑
                    break;

                case ESpectatorState::Standing:
                    // 站立时的逻辑
                    if (State.StateTimer > 5.0f)
                    {
                        State.State = ESpectatorState::Seated;
                        State.StateTimer = 0.0f;
                    }
                    break;

                case ESpectatorState::Cheering:
                    // 欢呼时的逻辑
                    break;

                // ... 其他状态
                }
            }
        });
}
```

**关键点**：
- `ConfigureQueries()` 声明需要哪些 Fragment（类似你查询 `WorldState`）
- `Execute()` 每帧调用，批量处理 Entity
- `ForEachEntityChunk()` 处理 Entity Chunk（内存连续，Cache-friendly）
- `GetMutableFragmentView()` 可读写，`GetFragmentView()` 只读

### 4.4 Trait 定义

Trait = Fragment 的组合，用于定义"原型"

```cpp
// ========== SpectatorTrait.h ==========
#pragma once

#include "MassEntityTraitBase.h"
#include "SpectatorTrait.generated.h"

UCLASS()
class STADIUMSIM_API USpectatorTrait : public UMassEntityTraitBase
{
    GENERATED_BODY()

protected:
    virtual void BuildTemplate(FMassEntityTemplateBuildContext& BuildContext, const UWorld& World) const override;
};

// ========== SpectatorTrait.cpp ==========
#include "SpectatorTrait.h"
#include "MassEntityTemplateRegistry.h"
#include "SpectatorFragments.h"
#include "MassCommonFragments.h"

void USpectatorTrait::BuildTemplate(FMassEntityTemplateBuildContext& BuildContext, const UWorld& World) const
{
    // 添加所有需要的 Fragment
    BuildContext.AddFragment<FTransformFragment>();
    BuildContext.AddFragment<FSpectatorStateFragment>();
    BuildContext.AddFragment<FSeatFragment>();
    BuildContext.AddFragment<FTeamFragment>();
    BuildContext.AddFragment<FAnimationFragment>();
    
    // 可以在这里设置默认值
    BuildContext.GetMutableObjectFragmentInitializers().Add(
        [](UObject& Owner, FMassEntityView& EntityView, const EMassTranslationDirection Direction)
        {
            FSpectatorStateFragment& State = EntityView.GetFragmentData<FSpectatorStateFragment>();
            State.State = ESpectatorState::Seated;
        });
}
```

### 4.5 生成 Entity

```cpp
// 在某个初始化逻辑中（比如 GameMode::BeginPlay）
void AStadiumGameMode::SpawnSpectators()
{
    UMassEntitySubsystem* EntitySubsystem = GetWorld()->GetSubsystem<UMassEntitySubsystem>();
    FMassEntityManager& EntityManager = EntitySubsystem->GetMutableEntityManager();

    // 获取 SpectatorTrait 的模板
    const UMassEntityConfigAsset* ConfigAsset = LoadObject<UMassEntityConfigAsset>(...);
    FMassEntityTemplate& Template = EntityManager.GetOrCreateEntityTemplate(...);

    // 批量生成 10 万个 Entity
    constexpr int32 NumSpectators = 100000;
    TArray<FMassEntityHandle> Entities;
    Entities.Reserve(NumSpectators);

    for (int32 i = 0; i < NumSpectators; ++i)
    {
        FMassEntityHandle Entity = EntityManager.CreateEntity(Template);
        Entities.Add(Entity);

        // 设置初始数据
        if (FSeatFragment* Seat = EntityManager.GetFragmentDataPtr<FSeatFragment>(Entity))
        {
            Seat->SeatIndex = i;
            Seat->SectorID = i / 10000;  // 每个扇区 1 万人
            Seat->SeatWorldPosition = CalculateSeatPosition(i);
        }

        if (FTransformFragment* Transform = EntityManager.GetFragmentDataPtr<FTransformFragment>(Entity))
        {
            Transform->GetMutableTransform().SetLocation(CalculateSeatPosition(i));
        }
    }
}
```

### 4.6 MassEntity vs 你的 Kodama 后端

| 概念 | Kodama (C#) | MassEntity (UE5) |
|------|-------------|------------------|
| Entity | `Agent` class + `Guid Id` | `FMassEntityHandle` (只是 ID) |
| Data | `Agent` 的字段 | `FMassFragment` struct |
| Container | `WorldState._agents` 字典 | `FMassEntityManager` |
| Logic | `AgentBehaviourService.Process()` | `UMassProcessor.Execute()` |
| Query | `worldState.GetAllAgents()` | `FMassEntityQuery.ForEachEntityChunk()` |

**思维方式完全一致**：
- 批量处理同状态的 Entity
- 数据和逻辑分离
- 避免 per-entity 虚函数调用

---

## 5. StadiumSim 实战路线图

### Milestone 1: 环境搭建 (2-3 小时)

**目标**：创建项目，搭建基础看台

**步骤**：

1. **创建项目**
   - Epic Games Launcher → Unreal Engine 5.3+
   - 新建项目 → Games → Blank → C++
   - 项目名：`StadiumSim`

2. **启用 MassEntity 插件**
   - Edit → Plugins → 搜索 "Mass"
   - 启用：`MassEntity`, `MassGameplay`, `MassAI` (如果存在)
   - 重启编辑器

3. **创建模块**
   - 在 `Source/StadiumSim/` 下创建文件结构：
   ```
   Source/StadiumSim/
   ├── StadiumSim.Build.cs    (修改，添加依赖)
   ├── Fragments/
   │   └── SpectatorFragments.h
   ├── Processors/
   │   ├── SpectatorStateProcessor.h
   │   └── SpectatorStateProcessor.cpp
   └── Traits/
       ├── SpectatorTrait.h
       └── SpectatorTrait.cpp
   ```

4. **修改 Build.cs 添加依赖**
   ```csharp
   // StadiumSim.Build.cs
   PublicDependencyModuleNames.AddRange(new string[] 
   { 
       "Core", 
       "CoreUObject", 
       "Engine", 
       "MassEntity",
       "MassCommon",
       "MassSpawner",
       "StructUtils"
   });
   ```

5. **搭建看台**
   - 在 Level 中用 BSP Brush 或 Geometry 搭建环形看台
   - 简单即可，几个弧形平面
   - 添加一个平面作为球场地面

**验收**：项目能编译运行，看到空看台

### Milestone 2: 静态 Entity 生成 (3-4 小时)

**目标**：生成 10,000 个静态观众

**步骤**：

1. **定义 Fragment**
   - 创建 `SpectatorFragments.h`
   - 定义 `FSpectatorStateFragment`, `FSeatFragment`, `FTeamFragment`

2. **创建 Trait**
   - 创建 `SpectatorTrait.h/cpp`
   - 组合所有 Fragment

3. **创建生成逻辑**
   - 在 GameMode 或自定义 Actor 中
   - 批量创建 Entity
   - 计算座位位置（环形分布）

4. **渲染测试**
   - 暂时用 Debug Draw 或 ISM (Instanced Static Mesh) 验证位置

**验收**：看到 10,000 个点/球体分布在看台上

### Milestone 3: 状态机 (3-4 小时)

**目标**：观众能响应事件，切换状态

**步骤**：

1. **创建 SpectatorStateProcessor**
   - 实现 `ConfigureQueries()` 和 `Execute()`
   - 处理 Seated ↔ Standing 切换

2. **创建事件系统**
   - 定义进球事件
   - Processor 响应事件，批量更新状态

3. **测试**
   - 按键触发进球
   - 观察状态变化（通过日志或 Debug Draw 颜色）

**验收**：按键后观众状态批量改变

### Milestone 4: 程序化动画 (3-4 小时)

**目标**：观众"活起来"

**步骤**：

1. **创建 ProceduralAnimationProcessor**
   - 根据 `FAnimationFragment` 计算动画偏移
   - 更新 `FTransformFragment`

2. **实现动画**
   - 呼吸：Y 轴正弦缩放
   - 摇摆：绕 Z 轴正弦旋转
   - 站起：Y 位置插值

3. **使用 ISM 渲染**
   - 创建 Instanced Static Mesh Actor
   - 每帧更新 Instance Transform

**验收**：观众有呼吸感和摇摆

### Milestone 5: 人浪效果 (2-3 小时)

**目标**：Demo 核心卖点

**步骤**：

1. **创建 WavePropagationProcessor**
   - 人浪信号从起点向两侧传播
   - 根据座位位置计算"波到达时间"

2. **实现传播逻辑**
   ```cpp
   float WaveArrivalTime = Distance / WaveSpeed;
   if (GlobalTime > WaveArrivalTime && GlobalTime < WaveArrivalTime + WaveDuration)
   {
       State.State = ESpectatorState::Wave;
   }
   ```

3. **测试**
   - 按键触发人浪
   - 观察传播效果

**验收**：人浪从一点向两侧传播

### Milestone 6: LOD 优化 (2-3 小时)

**目标**：100,000 Agent @ 30 FPS

**步骤**：

1. **实现 LODProcessor**
   - 根据与相机距离计算 LOD 级别
   - 远距离 Entity 降低更新频率

2. **分层渲染**
   - Near: 完整 Mesh
   - Far: 简化 Mesh 或 Point
   - Culled: 不渲染

3. **性能测试**
   - 用 `stat Mass` 查看 Processor 耗时
   - 用 `stat fps` 验证帧率

**验收**：100,000 Agent，鸟瞰 30 FPS

### Milestone 7: 打磨 & 录制 (2-3 小时)

**目标**：Demo 视频

**步骤**：

1. **材质美化**
   - 主队红色，客队蓝色
   - 简单的 Emissive 材质

2. **相机设置**
   - 添加平滑相机
   - 设置几个预设机位

3. **录制**
   - 展示全景人浪
   - 展示局部特写
   - 展示进球反应

**验收**：15-30 秒 Demo 视频

---

## 6. 常见坑 & 避雷指南

### 6.1 编译相关

**坑1：编译慢**
- 首次编译 5-15 分钟是正常的
- 增量编译约 10-60 秒
- **避雷**：不要在头文件里 include 大型头文件，用前向声明

```cpp
// ❌ 在头文件 include
#include "MassEntityManager.h"  // 巨大的头文件

// ✅ 用前向声明
class FMassEntityManager;  // 只声明
```

**坑2：链接错误 "unresolved external symbol"**
- 原因：忘记在 Build.cs 添加依赖模块
- **避雷**：新用一个模块就加依赖

**坑3：.generated.h 报错**
- 原因：忘了 `GENERATED_BODY()` 宏
- **避雷**：每个 `UCLASS`, `USTRUCT` 必须有

### 6.2 MassEntity 相关

**坑1：Entity 没有 Tick**
- 原因：Processor 没有注册或查询配置错误
- **避雷**：检查 `ConfigureQueries()` 和 `RegisterWithProcessor()`

**坑2：访问 Fragment 崩溃**
- 原因：Entity 没有那个 Fragment，或者 Fragment 没加到 Query
- **避雷**：用 `GetFragmentDataPtr()` 检查 null

**坑3：性能不如预期**
- 原因：在循环里分配内存，或用了 `FindEntity()` 而非批量处理
- **避雷**：用 `ForEachEntityChunk()` 批量处理，避免 per-entity 查找

### 6.3 渲染相关

**坑1：ISM 不更新**
- 原因：忘记调用 `UpdateInstanceTransform()` 或 `MarkRenderStateDirty()`
- **避雷**：每帧更新后调用标脏

**坑2：Draw Call 爆炸**
- 原因：每个 Entity 一个 Actor/Mesh
- **避雷**：用 Instanced Static Mesh 或 Niagara 批量渲染

### 6.4 调试技巧

```cpp
// 打印日志
UE_LOG(LogTemp, Warning, TEXT("Entity count: %d"), NumEntities);

// 屏幕输出
GEngine->AddOnScreenDebugMessage(-1, 5.0f, FColor::Yellow, TEXT("Hello!"));

// Debug Draw
DrawDebugSphere(GetWorld(), Location, 10.0f, 8, FColor::Red, false, 0.1f);

// MassEntity 性能统计
// 在控制台输入: stat Mass
```

---

## 7. 速查表 & 备忘

### 7.1 常用控制台命令

```
stat fps              // 帧率
stat unit             // 各模块耗时
stat Mass             // MassEntity 统计
stat scenerendering   // 渲染统计
t.MaxFPS 60           // 限制帧率
```

### 7.2 快捷键

| 功能 | 快捷键 |
|------|--------|
| 编译 | Ctrl+Shift+B (VS) / Cmd+B (Rider) |
| 热重载 | Ctrl+Alt+F11 (编辑器中) |
| 运行 | Alt+P |
| 停止 | Esc |
| 控制台 | ~ (波浪键) |

### 7.3 文件结构参考

```
StadiumSim/
├── Config/
├── Content/
│   ├── Blueprints/
│   ├── Materials/
│   └── Meshes/
├── Source/
│   └── StadiumSim/
│       ├── StadiumSim.Build.cs
│       ├── StadiumSim.h
│       ├── StadiumSim.cpp
│       ├── Fragments/
│       │   └── SpectatorFragments.h
│       ├── Processors/
│       │   ├── SpectatorStateProcessor.h
│       │   ├── SpectatorStateProcessor.cpp
│       │   ├── ProceduralAnimationProcessor.h
│       │   ├── ProceduralAnimationProcessor.cpp
│       │   ├── WavePropagationProcessor.h
│       │   └── WavePropagationProcessor.cpp
│       └── Traits/
│           ├── SpectatorTrait.h
│           └── SpectatorTrait.cpp
└── StadiumSim.uproject
```

### 7.4 核心头文件

```cpp
// 基础
#include "CoreMinimal.h"

// Actor / Component
#include "GameFramework/Actor.h"
#include "Components/StaticMeshComponent.h"
#include "Components/InstancedStaticMeshComponent.h"

// MassEntity
#include "MassEntityTypes.h"
#include "MassProcessor.h"
#include "MassEntityManager.h"
#include "MassExecutionContext.h"
#include "MassCommonFragments.h"
#include "MassEntityTraitBase.h"
```

### 7.5 面试话术模板

> **问**：为什么选择 MassEntity 而不是传统 Actor？
> 
> **答**：对于 10 万级 Agent，传统 Actor 模式会创建 10 万个 UObject，每个都有虚函数表、反射开销、单独的 Tick。MassEntity 使用 ECS 思想，Entity 只是 ID，数据存储在连续的 Fragment 数组中，Processor 批量处理。这带来两个好处：1) 内存连续，Cache 命中率高；2) 批量处理避免虚函数调用开销。我在 Kodama 项目的 C# 后端也用了类似思想，达到了 10 万 Agent @ 13ms/Tick @ 零分配。

> **问**：如何优化 10 万 Agent 的渲染？
>
> **答**：关键是减少 Draw Call。使用 Instanced Static Mesh，一次 Draw Call 渲染所有同材质的实例。结合 LOD 系统——近距离用完整 Mesh，远距离用简化 Mesh 或点，视锥外直接 Cull。MassEntity 的 Processor 可以根据相机距离分层更新：近处每帧更新，远处每 N 帧更新。

---

## 附录 A：从零创建 UE5 C++ 项目 (详细步骤)

### A.1 安装

1. 下载 Epic Games Launcher
2. 安装 Unreal Engine 5.3+ (选择 Editor + C++ 工具链)
3. 安装 Visual Studio 2022 或 Rider
   - VS2022 需要勾选 "Game development with C++"
   - Rider 需要安装 RiderLink 插件

### A.2 创建项目

1. Epic Launcher → Launch UE5
2. Games → Blank → C++ (不是 Blueprint!)
3. Project Name: `StadiumSim`
4. 取消勾选 Starter Content (不需要)
5. Create Project

### A.3 首次编译

1. 项目创建后会自动打开 VS/Rider
2. 等待 IntelliSense/索引完成
3. Build → Build Solution (首次 5-15 分钟)
4. 回到编辑器，Play 测试

### A.4 推荐的 VS 设置

- View → Solution Explorer → 显示所有文件
- Tools → Options → Text Editor → C++ → 启用 Clang 格式化
- 安装扩展：UnrealVS (Epic 官方)

---

## 附录 B：关键词索引

| 关键词 | 章节 |
|--------|------|
| AActor | 3.2 |
| Blueprint | 3.3 |
| Draw Call | 5 (Milestone 6) |
| Entity | 4.1 |
| FMassEntityHandle | 4.1 |
| FMassFragment | 4.2 |
| ForEachEntityChunk | 4.3 |
| Fragment | 4.2 |
| GPU Instancing | 5 (Milestone 4) |
| LOD | 5 (Milestone 6) |
| MassEntity | 4 |
| Processor | 4.3 |
| Trait | 4.4 |
| UMassProcessor | 4.3 |
| UPROPERTY | 2.4 |

---

**最后更新**: 2026-01-21
**作者**: Technical Mentor (AI Assistant)

---

**祝你 Unreal 开发顺利！有问题随时问。**
