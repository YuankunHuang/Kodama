# Effective Unreal for Simulation
## 大规模模拟开发的 50 条最佳实践

**读者画像**：Unity/C# 背景，转向 UE5 C++ + MassEntity，专注大规模 Agent 模拟。

**设计理念**：每条实践都应该成为你的"默认行为"——不需要思考，自然而然就这么做。

**使用方法**：
1. 首次阅读：通读一遍，理解为什么
2. 开发时：遇到问题回来查
3. 代码审查：作为 Checklist 使用

---

## 目录

### 第一章：思维模式
- [实践 01：先问"Epic 怎么想"，再问"我怎么想"](#实践-01先问epic-怎么想再问我怎么想)
- [实践 02：继承是默认选项，组合是补充](#实践-02继承是默认选项组合是补充)
- [实践 03：C++ 定骨架，Blueprint 填血肉](#实践-03c-定骨架blueprint-填血肉)
- [实践 04：编译慢是正常的，接受它](#实践-04编译慢是正常的接受它)
- [实践 05：不要翻译 Unity 代码，要重新设计](#实践-05不要翻译-unity-代码要重新设计)

### 第二章：内存与生命周期
- [实践 06：所有 UObject* 成员必须有 UPROPERTY()](#实践-06所有-uobject-成员必须有-uproperty)
- [实践 07：优先使用值类型（F 开头的 struct）](#实践-07优先使用值类型f-开头的-struct)
- [实践 08：用 TObjectPtr 替代裸指针（UE5.1+）](#实践-08用-tobjectptr-替代裸指针ue51)
- [实践 09：非 UObject 用 TSharedPtr / TUniquePtr](#实践-09非-uobject-用-tsharedptr--tuniqueptr)
- [实践 10：构造函数里不要写游戏逻辑](#实践-10构造函数里不要写游戏逻辑)
- [实践 11：理解 CDO（Class Default Object）](#实践-11理解-cdoclass-default-object)
- [实践 12：Transient 标记临时数据](#实践-12transient-标记临时数据)

### 第三章：代码组织
- [实践 13：头文件用前向声明，源文件用 include](#实践-13头文件用前向声明源文件用-include)
- [实践 14：一个类一对文件（.h + .cpp）](#实践-14一个类一对文件h--cpp)
- [实践 15：用模块隔离功能边界](#实践-15用模块隔离功能边界)
- [实践 16：Public/Private 文件夹分离接口和实现](#实践-16publicprivate-文件夹分离接口和实现)
- [实践 17：Build.cs 是模块的"契约"](#实践-17buildcs-是模块的契约)
- [实践 18：用 GENERATED_BODY() 而非 GENERATED_UCLASS_BODY()](#实践-18用-generated_body-而非-generated_uclass_body)

### 第四章：性能优化（通用）
- [实践 19：用 SCOPE_CYCLE_COUNTER 测量，不要猜](#实践-19用-scope_cycle_counter-测量不要猜)
- [实践 20：避免每帧 new/delete](#实践-20避免每帧-newdelete)
- [实践 21：TArray 预分配，用 Reserve() 不用 SetNum()](#实践-21tarray-预分配用-reserve-不用-setnum)
- [实践 22：字符串操作用 FName，不用 FString](#实践-22字符串操作用-fname不用-fstring)
- [实践 23：禁用不需要的 Tick](#实践-23禁用不需要的-tick)
- [实践 24：批量操作优于逐个操作](#实践-24批量操作优于逐个操作)
- [实践 25：Cache 友好的数据布局](#实践-25cache-友好的数据布局)

### 第五章：MassEntity 专项
- [实践 26：Fragment 是纯数据，不要有方法](#实践-26fragment-是纯数据不要有方法)
- [实践 27：一个 Processor 只做一件事](#实践-27一个-processor-只做一件事)
- [实践 28：用 Query 的 Access 声明约束读写](#实践-28用-query-的-access-声明约束读写)
- [实践 29：用 Chunk 迭代，不要逐 Entity 查询](#实践-29用-chunk-迭代不要逐-entity-查询)
- [实践 30：用 Tag 而非 bool Fragment 表示状态](#实践-30用-tag-而非-bool-fragment-表示状态)
- [实践 31：Processor 依赖用 ExecutionOrder 声明](#实践-31processor-依赖用-executionorder-声明)
- [实践 32：用 Shared Fragment 共享只读数据](#实践-32用-shared-fragment-共享只读数据)
- [实践 33：用 Trait 组合 Fragment，而非手动添加](#实践-33用-trait-组合-fragment而非手动添加)
- [实践 34：Entity 销毁用 Defer，不要立即删除](#实践-34entity-销毁用-defer不要立即删除)
- [实践 35：Signal 触发状态变化，而非每帧检查](#实践-35signal-触发状态变化而非每帧检查)

### 第六章：渲染与可视化
- [实践 36：用 ISM/HISM，不要用独立 Actor](#实践-36用-ismhism不要用独立-actor)
- [实践 37：LOD 是必须的，不是可选的](#实践-37lod-是必须的不是可选的)
- [实践 38：用 MassRepresentation 管理渲染](#实践-38用-massrepresentation-管理渲染)
- [实践 39：程序化动画用数学，不用骨骼](#实践-39程序化动画用数学不用骨骼)
- [实践 40：材质实例共享，参数用 Per-Instance Custom Data](#实践-40材质实例共享参数用-per-instance-custom-data)

### 第七章：调试与诊断
- [实践 41：用 UE_LOG 分级日志](#实践-41用-ue_log-分级日志)
- [实践 42：用 stat 命令监控性能](#实践-42用-stat-命令监控性能)
- [实践 43：用 Visual Logger 可视化调试](#实践-43用-visual-logger-可视化调试)
- [实践 44：用 Gameplay Debugger 检查 Entity 状态](#实践-44用-gameplay-debugger-检查-entity-状态)
- [实践 45：用 check() 和 ensure() 断言](#实践-45用-check-和-ensure-断言)

### 第八章：工程实践
- [实践 46：每次提交前本地全编译](#实践-46每次提交前本地全编译)
- [实践 47：用 .uplugin 封装可复用系统](#实践-47用-uplugin-封装可复用系统)
- [实践 48：Config 文件用于可调参数](#实践-48config-文件用于可调参数)
- [实践 49：用 Subsystem 管理全局服务](#实践-49用-subsystem-管理全局服务)
- [实践 50：写代码时想象它会被审查](#实践-50写代码时想象它会被审查)

---

## 第一章：思维模式

### 实践 01：先问"Epic 怎么想"，再问"我怎么想"

**原则**：Unreal 是有主见的框架，不是中立的工具箱。

**为什么重要**：
- Epic 已经为你做了大量架构决策
- 顺着框架走 = 少写代码 + 少踩坑
- 对抗框架 = 痛苦 + 维护噩梦

**怎么做**：

```cpp
// ❌ Unity 思维：我要自己管理游戏状态
class MyGameManager : public AActor  // 自己造轮子
{
    void SaveGame();
    void LoadGame();
    void TrackScore();
};

// ✅ Unreal 思维：Epic 已经有 GameMode/GameState
class AMyGameMode : public AGameModeBase  // 继承现有框架
{
    // 游戏规则、胜负判定
};

class AMyGameState : public AGameStateBase
{
    // 所有玩家可见的状态（分数、时间）
};
```

**默认行为**：
> 做任何功能前，先搜索 "Unreal [功能名] best practice"，看看框架提供了什么。

---

### 实践 02：继承是默认选项，组合是补充

**原则**：Unreal 的基类自带大量功能，继承它们比自己组合更划算。

**为什么重要**：
- `ACharacter` 自带移动组件、胶囊碰撞、动画集成
- 自己用组合拼凑 = 重新实现轮子
- 继承让你立即获得 Epic 多年的积累

**怎么做**：

```cpp
// ❌ Unity 思维：组合拼凑
class AMyPlayer : public AActor
{
    USkeletalMeshComponent* Mesh;
    UCharacterMovementComponent* Movement;  // 还要自己连接...
    UCapsuleComponent* Capsule;
    // 然后花 3 天让它们协同工作
};

// ✅ Unreal 思维：继承获得一切
class AMyPlayer : public ACharacter  // 上述全部自带
{
    // 直接开始写游戏逻辑
};
```

**默认行为**：
> 创建新类时，先问：有没有现成的基类可以继承？

**常用基类速查**：
| 需求 | 基类 |
|------|------|
| 可控制的角色 | `ACharacter` |
| 可控制但无移动 | `APawn` |
| 静态物件 | `AActor` |
| 游戏规则 | `AGameModeBase` |
| 共享状态 | `AGameStateBase` |
| 玩家输入 | `APlayerController` |
| 全局服务 | `UGameInstanceSubsystem` |

---

### 实践 03：C++ 定骨架，Blueprint 填血肉

**原则**：C++ 写核心系统和性能关键代码，Blueprint 做变体和调参。

**为什么重要**：
- C++ 编译慢但运行快
- Blueprint 迭代快但运行慢
- 美术/设计师用 Blueprint，程序员用 C++

**怎么做**：

```cpp
// C++ 基类：定义接口和核心逻辑
UCLASS(Blueprintable)  // 允许蓝图继承
class AWeapon : public AActor
{
    GENERATED_BODY()

public:
    // 蓝图可调用
    UFUNCTION(BlueprintCallable)
    void Fire();

    // 蓝图可重写
    UFUNCTION(BlueprintNativeEvent)
    void OnHit(AActor* Target);

protected:
    // 蓝图可编辑
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
    float Damage = 10.0f;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
    float FireRate = 0.5f;
};
```

然后在 Blueprint 中：
- 创建 `BP_Rifle`（继承 `AWeapon`，Damage = 20）
- 创建 `BP_Shotgun`（继承 `AWeapon`，Damage = 50, FireRate = 1.0）
- 美术可以直接调参，不用改 C++

**默认行为**：
> 系统架构和算法用 C++，数值和资源引用暴露给 Blueprint。

---

### 实践 04：编译慢是正常的，接受它

**原则**：C++ 编译慢是物理定律，不是 Epic 的错。

**为什么重要**：
- 首次编译 5-15 分钟是正常的
- 增量编译 10-60 秒是正常的
- 焦虑不会让它变快，但好习惯会

**怎么做**：

1. **用 Live Coding 热重载**（不完美但能用）
   - Ctrl+Alt+F11 或 控制台 `LiveCoding.Compile`
   - 不支持：新增类、修改头文件、修改 UPROPERTY

2. **头文件最小化**
   ```cpp
   // ❌ 头文件 include 一切
   #include "GameFramework/Actor.h"
   #include "Components/StaticMeshComponent.h"
   #include "Materials/MaterialInterface.h"
   #include "Engine/World.h"  // 巨大的头文件！
   
   // ✅ 头文件只前向声明
   class UStaticMeshComponent;
   class UMaterialInterface;
   class UWorld;
   
   // 在 .cpp 中 include
   #include "Components/StaticMeshComponent.h"
   ```

3. **用 Unity Build（不是 Unity 引擎！）**
   - 编辑器 → Editor Preferences → Compile → Use Unity Build
   - 把多个 .cpp 合并编译，减少编译时间

4. **用 Include What You Use (IWYU)**
   - UE5 默认启用
   - 只 include 你直接用的头文件

**默认行为**：
> 编译时去喝水、上厕所、看文档。不要盯着进度条焦虑。

---

### 实践 05：不要翻译 Unity 代码，要重新设计

**原则**：逐行翻译 Unity 代码 = 两个引擎的缺点都占全。

**为什么重要**：
- Unity 的最佳实践 ≠ Unreal 的最佳实践
- 比如 Unity 推崇组合，Unreal 推崇继承
- 比如 Unity 的 Update() 默认启用，Unreal 的 Tick 可以禁用

**怎么做**：

```csharp
// Unity 代码
public class HealthComponent : MonoBehaviour
{
    public float health = 100f;
    public event Action<float> OnHealthChanged;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke(health);
        if (health <= 0) Die();
    }
}
```

```cpp
// ❌ 逐行翻译
UCLASS()
class UHealthComponent : public UActorComponent
{
    UPROPERTY()
    float Health = 100.0f;
    
    DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnHealthChanged, float, NewHealth);
    FOnHealthChanged OnHealthChanged;
    
    void TakeDamage(float Damage);
};

// ✅ 用 Unreal 的方式
// 方案 A：用 Gameplay Ability System (GAS)
// 方案 B：用 Attribute Set
// 方案 C：如果简单，直接放在 Character 里

class AMyCharacter : public ACharacter
{
    UPROPERTY(ReplicatedUsing=OnRep_Health)
    float Health = 100.0f;
    
    UFUNCTION()
    void OnRep_Health();  // 网络同步时自动调用
    
    virtual float TakeDamage(float Damage, ...) override;  // 用 AActor 自带的！
};
```

**默认行为**：
> 拿到 Unity 代码，先理解它要解决什么问题，再用 Unreal 的方式重新设计。

---

## 第二章：内存与生命周期

### 实践 06：所有 UObject* 成员必须有 UPROPERTY()

**原则**：没有 UPROPERTY() 的 UObject 指针 = 悬空指针定时炸弹。

**为什么重要**：
- Unreal GC 只追踪被 UPROPERTY() 标记的引用
- 未标记的指针不阻止对象被回收
- 对象被回收后，你的指针变成野指针 → 崩溃

**怎么做**：

```cpp
// ❌ 必崩
class AMyActor : public AActor
{
    AActor* Target;  // GC 不知道你在用它！
    
    void Tick(float DeltaTime)
    {
        Target->DoSomething();  // 💥 Target 可能已被 GC
    }
};

// ✅ 安全
class AMyActor : public AActor
{
    UPROPERTY()  // GC 知道你在用它，会保持它存活
    AActor* Target;
};
```

**特殊情况**：
```cpp
// 局部变量不需要 UPROPERTY（生命周期在栈上）
void SomeFunction()
{
    AActor* LocalActor = GetWorld()->SpawnActor<AActor>(...);
    LocalActor->DoSomething();  // OK，函数结束前不会 GC
}

// 但如果你存起来就需要了
void SomeFunction()
{
    CachedActor = GetWorld()->SpawnActor<AActor>(...);  // CachedActor 必须有 UPROPERTY
}
```

**默认行为**：
> 写成员变量时，如果类型是 `U*` 或 `A*`，立刻加 `UPROPERTY()`。没有例外。

---

### 实践 07：优先使用值类型（F 开头的 struct）

**原则**：UObject 重，struct 轻。能用 struct 就不用 UObject。

**为什么重要**：
- `UObject` 创建需要反射注册、GC 追踪、序列化支持
- `struct` 就是纯数据，栈分配，几乎零开销
- 10 万个 Agent 用 `UObject` = 内存爆炸

**怎么做**：

```cpp
// ❌ 用 UObject 存纯数据
UCLASS()
class UAgentData : public UObject  // 太重了！
{
    UPROPERTY()
    FVector Position;
    
    UPROPERTY()
    float Health;
};
TArray<UAgentData*> Agents;  // 10 万个 = 灾难

// ✅ 用 struct 存纯数据
USTRUCT()
struct FAgentData
{
    GENERATED_BODY()
    
    FVector Position;
    float Health;
};
TArray<FAgentData> Agents;  // 10 万个 = 没问题

// ✅ MassEntity 版本
USTRUCT()
struct FPositionFragment : public FMassFragment
{
    GENERATED_BODY()
    FVector Value;
};
```

**什么时候用 UObject**：
- 需要蓝图可见
- 需要网络复制
- 需要序列化存盘
- 需要反射/动态创建

**什么时候用 struct**：
- 纯数据容器
- 性能关键路径
- 大量实例

**默认行为**：
> 默认用 `USTRUCT`，只有需要 UObject 特性时才升级为 `UCLASS`。

---

### 实践 08：用 TObjectPtr 替代裸指针（UE5.1+）

**原则**：`TObjectPtr<T>` 是 UE5 推荐的 UObject 指针类型。

**为什么重要**：
- 编辑器中提供更好的空指针检测
- 未来可能支持更多安全特性
- Epic 在逐步迁移所有代码

**怎么做**：

```cpp
// UE4 风格（仍然可用）
UPROPERTY()
AActor* Target;

// UE5 推荐风格
UPROPERTY()
TObjectPtr<AActor> Target;

// 使用方式完全相同
Target->DoSomething();
if (Target) { ... }
```

**注意**：
- 只用于成员变量，不用于局部变量或参数
- 局部变量和参数继续用 `AActor*`

**默认行为**：
> 新代码的 `UPROPERTY()` UObject 指针都用 `TObjectPtr<T>`。

---

### 实践 09：非 UObject 用 TSharedPtr / TUniquePtr

**原则**：非 UObject 的堆分配对象用 Unreal 智能指针管理。

**为什么重要**：
- 非 UObject 不被 GC 管理
- 裸指针 `new/delete` 容易内存泄漏
- 智能指针自动管理生命周期

**怎么做**：

```cpp
// ❌ 裸指针管理非 UObject
class FMyData { ... };  // 不是 UObject

class AMyActor : public AActor
{
    FMyData* Data;  // 谁负责 delete？
    
    AMyActor() { Data = new FMyData(); }
    ~AMyActor() { delete Data; }  // 容易忘记
};

// ✅ 用智能指针
class AMyActor : public AActor
{
    TUniquePtr<FMyData> Data;  // 独占所有权，自动释放
    
    AMyActor()
    {
        Data = MakeUnique<FMyData>();
    }
    // 不需要析构函数，自动释放
};

// 需要共享所有权时
TSharedPtr<FMyData> SharedData = MakeShared<FMyData>();
TWeakPtr<FMyData> WeakData = SharedData;  // 弱引用，不阻止释放
```

**智能指针速查**：
| 类型 | 用途 |
|------|------|
| `TUniquePtr<T>` | 独占所有权，不能复制 |
| `TSharedPtr<T>` | 共享所有权，引用计数 |
| `TWeakPtr<T>` | 弱引用，不增加引用计数 |
| `TSharedRef<T>` | 共享引用，保证非空 |

**默认行为**：
> 非 UObject 的堆分配用 `TUniquePtr`（默认）或 `TSharedPtr`（需要共享时）。

---

### 实践 10：构造函数里不要写游戏逻辑

**原则**：构造函数只做默认值初始化，游戏逻辑放 BeginPlay。

**为什么重要**：
- 构造函数在 CDO（Class Default Object）创建时也会调用
- 编辑器加载时就会调用构造函数
- 这时候 World、GameMode 等都不存在

**怎么做**：

```cpp
// ❌ 构造函数里写逻辑
AMyActor::AMyActor()
{
    // 创建组件 - OK
    MeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
    
    // 游戏逻辑 - 错误！
    GetWorld()->SpawnActor(...);  // 💥 World 可能是 null
    FindActorOfClass(...);        // 💥 同上
    PlaySound();                  // 💥 没有音频系统
}

// ✅ 正确做法
AMyActor::AMyActor()
{
    // 只做：创建组件、设置默认值
    MeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
    RootComponent = MeshComponent;
    
    Health = 100.0f;
    Speed = 600.0f;
}

void AMyActor::BeginPlay()
{
    Super::BeginPlay();
    
    // 游戏逻辑放这里
    SpawnInitialEnemies();
    PlayBackgroundMusic();
}
```

**默认行为**：
> 构造函数 = CreateDefaultSubobject + 默认值。其他都放 BeginPlay。

---

### 实践 11：理解 CDO（Class Default Object）

**原则**：每个 UCLASS 都有一个 CDO，它是所有实例的"模板"。

**为什么重要**：
- 你的构造函数会被调用两次：一次创建 CDO，一次创建实例
- CDO 在编辑器启动时就创建
- 修改 CDO = 影响所有实例的默认值

**怎么做**：

```cpp
// 获取 CDO
const AMyActor* DefaultActor = GetDefault<AMyActor>();
float DefaultSpeed = DefaultActor->Speed;  // 获取默认值

// 检查是否是 CDO
if (HasAnyFlags(RF_ClassDefaultObject))
{
    // 这是 CDO，不是真实实例
    return;
}
```

**CDO 的用途**：
- 蓝图编辑器显示的默认值来自 CDO
- `GetDefault<T>()` 获取 CDO 读取默认配置
- 序列化时，只存储与 CDO 不同的值（节省空间）

**默认行为**：
> 理解构造函数会被 CDO 调用，不要假设 World 存在。

---

### 实践 12：Transient 标记临时数据

**原则**：不需要保存/复制的数据标记为 Transient。

**为什么重要**：
- 没有 Transient 的 UPROPERTY 会被序列化到磁盘
- 会被网络复制
- 运行时临时数据不需要这些

**怎么做**：

```cpp
UPROPERTY()
float Health;  // 保存到磁盘，网络同步

UPROPERTY(Transient)
float CachedDistance;  // 不保存，不同步

UPROPERTY(Transient)
TArray<AActor*> NearbyEnemies;  // 运行时计算的，不需要保存
```

**什么数据应该 Transient**：
- 每帧计算的缓存值
- 运行时查找的引用
- 临时状态（如"正在攻击中"）

**什么数据不应该 Transient**：
- 需要存档的（玩家进度）
- 需要网络同步的（多人游戏状态）

**默认行为**：
> 添加 UPROPERTY 时问自己：这个数据需要保存吗？不需要就加 Transient。

---

## 第三章：代码组织

### 实践 13：头文件用前向声明，源文件用 include

**原则**：头文件只声明类型存在，源文件才真正 include。

**为什么重要**：
- 头文件被大量其他文件 include
- 头文件里 include 大头文件 = 编译时间爆炸
- 前向声明几乎零成本

**怎么做**：

```cpp
// ========== MyActor.h ==========
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "MyActor.generated.h"  // 必须 include

// 前向声明（不 include）
class UStaticMeshComponent;
class UMaterialInterface;
class AEnemy;

UCLASS()
class AMyActor : public AActor
{
    GENERATED_BODY()
    
    UPROPERTY()
    UStaticMeshComponent* Mesh;  // OK，只是指针
    
    void Attack(AEnemy* Target);  // OK，只是指针参数
};

// ========== MyActor.cpp ==========
#include "MyActor.h"
#include "Components/StaticMeshComponent.h"  // 现在才 include
#include "Materials/MaterialInterface.h"
#include "Enemy.h"

void AMyActor::Attack(AEnemy* Target)
{
    Target->TakeDamage(10.0f);  // 需要完整定义，所以在 cpp 里 include
}
```

**什么时候必须在头文件 include**：
- 继承的基类
- 成员变量是值类型（不是指针）
- inline 函数需要完整定义

**默认行为**：
> 头文件里看到类型名，先写前向声明。只有编译器报错才考虑 include。

---

### 实践 14：一个类一对文件（.h + .cpp）

**原则**：每个 UCLASS/USTRUCT 一个 .h/.cpp 对。

**为什么重要**：
- 便于导航和搜索
- 符合 Unreal 约定
- 减少不必要的重编译

**怎么做**：

```
Source/StadiumSim/
├── Spectator/
│   ├── Spectator.h
│   ├── Spectator.cpp
│   ├── SpectatorController.h
│   └── SpectatorController.cpp
├── Fragments/
│   ├── SpectatorStateFragment.h     // 可以只有头文件
│   ├── SeatFragment.h
│   └── AnimationFragment.h
└── Processors/
    ├── SpectatorStateProcessor.h
    ├── SpectatorStateProcessor.cpp
    ├── AnimationProcessor.h
    └── AnimationProcessor.cpp
```

**例外**：
- 简单的 USTRUCT 可以只有头文件（没有 .cpp）
- 多个紧密相关的小 struct 可以放一个文件

**默认行为**：
> 创建新类 = 创建同名的 .h 和 .cpp。

---

### 实践 15：用模块隔离功能边界

**原则**：大型项目应该拆分多个模块，而非一个巨型模块。

**为什么重要**：
- 模块是编译单元，改一个模块不影响其他模块
- 强制明确依赖关系
- 便于复用（可以单独做成插件）

**怎么做**：

```
Source/
├── StadiumSim/              # 主模块
│   ├── StadiumSim.Build.cs
│   └── ...
├── MassSpectator/           # 观众模拟模块
│   ├── MassSpectator.Build.cs
│   ├── Fragments/
│   └── Processors/
└── StadiumCore/             # 核心共享模块
    ├── StadiumCore.Build.cs
    └── Types/
```

```csharp
// MassSpectator.Build.cs
public class MassSpectator : ModuleRules
{
    public MassSpectator(ReadOnlyTargetRules Target) : base(Target)
    {
        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "MassEntity",
            "StadiumCore"  // 依赖核心模块
        });
    }
}
```

**何时拆模块**：
- 代码超过 50 个文件
- 有明确的功能边界
- 想复用到其他项目

**默认行为**：
> 小项目一个模块够用。感觉臃肿时再拆分。

---

### 实践 16：Public/Private 文件夹分离接口和实现

**原则**：Public 文件夹放公开接口，Private 文件夹放内部实现。

**为什么重要**：
- 其他模块只能 include Public 文件夹的头文件
- 明确 API 边界
- 隐藏实现细节

**怎么做**：

```
Source/MassSpectator/
├── Public/                          # 对外接口
│   ├── SpectatorTypes.h            # 公开类型定义
│   ├── ISpectatorSubsystem.h       # 公开接口
│   └── SpectatorFragments.h        # 公开 Fragment
├── Private/                         # 内部实现
│   ├── SpectatorSubsystem.h        # 实现类
│   ├── SpectatorSubsystem.cpp
│   ├── SpectatorStateProcessor.h   # 内部 Processor
│   └── SpectatorStateProcessor.cpp
└── MassSpectator.Build.cs
```

```csharp
// Build.cs 配置
PublicIncludePaths.AddRange(new string[]
{
    Path.Combine(ModuleDirectory, "Public")
});

PrivateIncludePaths.AddRange(new string[]
{
    Path.Combine(ModuleDirectory, "Private")
});
```

**默认行为**：
> 新文件默认放 Private。只有其他模块需要用时才移到 Public。

---

### 实践 17：Build.cs 是模块的"契约"

**原则**：Build.cs 声明模块的依赖、编译选项、平台支持。

**为什么重要**：
- 链接错误 90% 是因为 Build.cs 缺依赖
- 编译选项影响性能和调试
- 是理解项目结构的入口

**怎么做**：

```csharp
// StadiumSim.Build.cs
using UnrealBuildTool;

public class StadiumSim : ModuleRules
{
    public StadiumSim(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
        
        // 公开依赖（暴露给依赖此模块的其他模块）
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreUObject",
            "Engine"
        });
        
        // 私有依赖（只在本模块内部使用）
        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "MassEntity",
            "MassCommon",
            "MassSpawner",
            "StructUtils"
        });
        
        // 可选：启用更严格的警告
        // bEnableUndefinedIdentifierWarnings = true;
    }
}
```

**常见错误**：
- `unresolved external symbol` → 缺少依赖模块
- `cannot open include file` → 缺少依赖或路径问题

**默认行为**：
> 用到新模块时，立刻加到 Build.cs。不要等链接错误。

---

### 实践 18：用 GENERATED_BODY() 而非 GENERATED_UCLASS_BODY()

**原则**：`GENERATED_BODY()` 是现代写法，`GENERATED_UCLASS_BODY()` 是遗留写法。

**为什么重要**：
- `GENERATED_BODY()` 不自动生成构造函数，更灵活
- `GENERATED_UCLASS_BODY()` 会生成一个带 `FObjectInitializer` 参数的构造函数
- 新代码应该用新写法

**怎么做**：

```cpp
// ❌ 遗留写法
UCLASS()
class AMyActor : public AActor
{
    GENERATED_UCLASS_BODY()  // 老式
    // 自动生成：AMyActor(const FObjectInitializer& ObjectInitializer);
};

// ✅ 现代写法
UCLASS()
class AMyActor : public AActor
{
    GENERATED_BODY()  // 新式
    
public:
    AMyActor();  // 自己声明构造函数
};
```

**默认行为**：
> 永远用 `GENERATED_BODY()`。看到 `GENERATED_UCLASS_BODY()` 的老代码可以重构。

---

## 第四章：性能优化（通用）

### 实践 19：用 SCOPE_CYCLE_COUNTER 测量，不要猜

**原则**：性能优化必须基于数据，不要凭感觉。

**为什么重要**：
- 直觉往往是错的
- "显然慢"的代码可能很快
- "显然快"的代码可能是瓶颈

**怎么做**：

```cpp
// 1. 声明统计组（在模块初始化或 .cpp 顶部）
DECLARE_STATS_GROUP(TEXT("Spectator"), STATGROUP_Spectator, STATCAT_Advanced);
DECLARE_CYCLE_STAT(TEXT("Process State"), STAT_ProcessState, STATGROUP_Spectator);
DECLARE_CYCLE_STAT(TEXT("Update Animation"), STAT_UpdateAnimation, STATGROUP_Spectator);

// 2. 在代码中使用
void USpectatorStateProcessor::Execute(...)
{
    SCOPE_CYCLE_COUNTER(STAT_ProcessState);  // 自动计时
    
    // ... 实际逻辑
}

void UAnimationProcessor::Execute(...)
{
    SCOPE_CYCLE_COUNTER(STAT_UpdateAnimation);
    
    // ... 实际逻辑
}
```

```
// 3. 在游戏中查看
控制台输入: stat Spectator
```

**输出示例**：
```
Spectator
  Process State:     2.3ms
  Update Animation:  1.1ms
```

**默认行为**：
> 每个 Processor 的 Execute 都加 SCOPE_CYCLE_COUNTER。优化前先看数据。

---

### 实践 20：避免每帧 new/delete

**原则**：堆分配是性能杀手，尤其在热路径。

**为什么重要**：
- 堆分配需要锁、系统调用
- 内存碎片化
- 你在 Kodama 已经学过这个了

**怎么做**：

```cpp
// ❌ 每帧分配
void Tick(float DeltaTime)
{
    TArray<FHitResult> Hits;  // 每帧构造/析构
    DoSweep(Hits);
}

// ✅ 预分配重用
class AMyActor : public AActor
{
    TArray<FHitResult> CachedHits;  // 成员变量
    
    void Tick(float DeltaTime)
    {
        CachedHits.Reset();  // 清空但保留内存
        DoSweep(CachedHits);
    }
};
```

**TArray 操作速查**：
| 方法 | 行为 |
|------|------|
| `Empty()` | 清空，释放内存 |
| `Reset()` | 清空，保留内存 |
| `Reserve(N)` | 预分配 N 个元素的空间 |
| `SetNum(N)` | 设置大小为 N，可能分配/释放 |
| `SetNumUninitialized(N)` | 设置大小为 N，不初始化 |

**默认行为**：
> 热路径中的 TArray 作为成员变量预分配，用 Reset() 重用。

---

### 实践 21：TArray 预分配，用 Reserve() 不用 SetNum()

**原则**：知道大概数量时，先 Reserve 再 Add。

**为什么重要**：
- TArray 扩容是 O(N) 复制
- 频繁扩容 = 频繁分配 + 复制
- Reserve 预留空间避免扩容

**怎么做**：

```cpp
// ❌ 默认增长，多次扩容
TArray<FVector> Positions;
for (int32 i = 0; i < 10000; ++i)
{
    Positions.Add(GetPosition(i));  // 可能触发多次扩容
}

// ✅ 预分配
TArray<FVector> Positions;
Positions.Reserve(10000);  // 一次性分配
for (int32 i = 0; i < 10000; ++i)
{
    Positions.Add(GetPosition(i));  // 不会扩容
}

// ✅✅ 如果知道确切大小，用 SetNumUninitialized
TArray<FVector> Positions;
Positions.SetNumUninitialized(10000);  // 分配但不初始化
for (int32 i = 0; i < 10000; ++i)
{
    Positions[i] = GetPosition(i);  // 直接赋值
}
```

**默认行为**：
> 循环前知道大概数量就 Reserve。知道确切数量用 SetNumUninitialized。

---

### 实践 22：字符串操作用 FName，不用 FString

**原则**：FName 是不可变的哈希标识符，比较是 O(1)。

**为什么重要**：
- FString 比较是 O(N) 字符比较
- FName 比较是整数比较，O(1)
- FName 内部有全局字符串表，相同字符串共享内存

**怎么做**：

```cpp
// ❌ 用 FString 做标识符
FString Tag = TEXT("Enemy");
if (OtherTag == Tag) { ... }  // O(N) 字符串比较

// ✅ 用 FName 做标识符
FName Tag = TEXT("Enemy");  // 或 FName(TEXT("Enemy"))
if (OtherTag == Tag) { ... }  // O(1) 整数比较

// ✅ 用于 Gameplay Tags
FGameplayTag EnemyTag = FGameplayTag::RequestGameplayTag(FName(TEXT("Unit.Enemy")));
```

**什么时候用 FString**：
- 需要修改字符串内容
- 需要格式化输出
- 用户可见的文本

**什么时候用 FName**：
- 标识符、标签
- 查找资源
- 配置键

**什么时候用 FText**：
- 本地化文本
- UI 显示

**默认行为**：
> 标识符用 FName，显示文本用 FText，只有需要操作时用 FString。

---

### 实践 23：禁用不需要的 Tick

**原则**：不需要每帧更新的 Actor 禁用 Tick。

**为什么重要**：
- 每个 Tick 调用都有开销（虚函数、调度）
- 1000 个 Actor 都 Tick = 每帧 1000 次函数调用
- 大多数 Actor 不需要每帧更新

**怎么做**：

```cpp
// 构造函数中禁用
AMyStaticActor::AMyStaticActor()
{
    PrimaryActorTick.bCanEverTick = false;  // 完全禁用
}

// 或者条件启用
AMyActor::AMyActor()
{
    PrimaryActorTick.bCanEverTick = true;
    PrimaryActorTick.bStartWithTickEnabled = false;  // 默认禁用
}

void AMyActor::Activate()
{
    SetActorTickEnabled(true);  // 需要时启用
}

void AMyActor::Deactivate()
{
    SetActorTickEnabled(false);  // 不需要时禁用
}
```

**Tick 组（控制执行顺序）**：
```cpp
PrimaryActorTick.TickGroup = TG_PrePhysics;  // 物理前
PrimaryActorTick.TickGroup = TG_DuringPhysics;  // 物理中
PrimaryActorTick.TickGroup = TG_PostPhysics;  // 物理后（默认）
```

**默认行为**：
> 新 Actor 默认 `bCanEverTick = false`。确实需要时才改为 true。

---

### 实践 24：批量操作优于逐个操作

**原则**：一次处理 N 个比 N 次处理 1 个更高效。

**为什么重要**：
- 减少函数调用开销
- 更好的 Cache 局部性
- 你在 Kodama 的 Processor 已经这样做了

**怎么做**：

```cpp
// ❌ 逐个操作
for (AActor* Actor : Actors)
{
    FHitResult Hit;
    Actor->LineTrace(..., Hit);  // 每个 Actor 单独 Trace
}

// ✅ 批量操作
TArray<FHitResult> Hits;
Hits.SetNumUninitialized(Actors.Num());
GetWorld()->LineTraceMulti(..., Hits);  // 一次 Trace 多个

// MassEntity 本来就是批量的
EntityQuery.ForEachEntityChunk(EntityManager, Context,
    [](FMassExecutionContext& Context)
    {
        const int32 NumEntities = Context.GetNumEntities();
        // 批量处理整个 Chunk
    });
```

**默认行为**：
> 看到 for 循环里调用引擎 API，问自己：有没有批量版本？

---

### 实践 25：Cache 友好的数据布局

**原则**：连续内存访问比随机访问快 100 倍。

**为什么重要**：
- CPU Cache 按 64 字节 Cache Line 加载
- 访问一个元素会预加载相邻元素
- 指针追逐 = Cache Miss = 慢

**怎么做**：

```cpp
// ❌ 指针数组（Cache 不友好）
TArray<FAgentData*> Agents;  // 每个指针指向不同内存位置
for (FAgentData* Agent : Agents)
{
    Agent->Position += Agent->Velocity;  // 每次可能 Cache Miss
}

// ✅ 值数组（Cache 友好）
TArray<FAgentData> Agents;  // 连续内存
for (FAgentData& Agent : Agents)
{
    Agent.Position += Agent.Velocity;  // 连续访问，Cache Hit
}

// ✅✅ SoA 布局（极致优化）
struct FAgentArrays
{
    TArray<FVector> Positions;
    TArray<FVector> Velocities;
    // 访问 Position 时 Velocity 不污染 Cache
};
```

**MassEntity 的 Fragment 天然是 SoA 风格**：
```cpp
// ForEachEntityChunk 给你的是连续数组
TArrayView<FPositionFragment> Positions = Context.GetMutableFragmentView<FPositionFragment>();
TArrayView<FVelocityFragment> Velocities = Context.GetFragmentView<FVelocityFragment>();

for (int32 i = 0; i < NumEntities; ++i)
{
    Positions[i].Value += Velocities[i].Value * DeltaTime;
}
```

**默认行为**：
> 值类型数组 > 指针数组。SoA > AoS（当需要极致性能时）。

---

## 第五章：MassEntity 专项

### 实践 26：Fragment 是纯数据，不要有方法

**原则**：Fragment 只存数据，逻辑放 Processor。

**为什么重要**：
- Fragment 是 POD（Plain Old Data），便于批量操作
- 方法引入虚函数表、复杂度
- Processor 集中处理逻辑更易优化

**怎么做**：

```cpp
// ❌ Fragment 带方法
USTRUCT()
struct FHealthFragment : public FMassFragment
{
    GENERATED_BODY()
    
    float Current;
    float Max;
    
    void TakeDamage(float Damage)  // 不要这样！
    {
        Current = FMath::Max(0.0f, Current - Damage);
    }
    
    bool IsDead() const { return Current <= 0; }  // 也不要
};

// ✅ Fragment 纯数据
USTRUCT()
struct FHealthFragment : public FMassFragment
{
    GENERATED_BODY()
    
    float Current = 100.0f;
    float Max = 100.0f;
};

// 逻辑放 Processor
void UHealthProcessor::Execute(...)
{
    EntityQuery.ForEachEntityChunk(..., [](FMassExecutionContext& Context)
    {
        TArrayView<FHealthFragment> Healths = Context.GetMutableFragmentView<FHealthFragment>();
        TConstArrayView<FDamageFragment> Damages = Context.GetFragmentView<FDamageFragment>();
        
        for (int32 i = 0; i < NumEntities; ++i)
        {
            Healths[i].Current = FMath::Max(0.0f, Healths[i].Current - Damages[i].Amount);
        }
    });
}
```

**默认行为**：
> Fragment 只有数据成员，没有方法（除了构造函数）。

---

### 实践 27：一个 Processor 只做一件事

**原则**：每个 Processor 有单一职责，通过 ExecutionOrder 协调。

**为什么重要**：
- 单一职责便于理解和测试
- 可以独立启用/禁用
- 便于性能分析（每个 Processor 单独计时）

**怎么做**：

```cpp
// ❌ 一个 Processor 做所有事
class USpectatorMegaProcessor : public UMassProcessor
{
    void Execute(...)
    {
        // 更新状态
        // 计算动画
        // 处理人浪
        // 更新 LOD
        // ... 500 行代码
    }
};

// ✅ 拆分多个 Processor
class USpectatorStateProcessor : public UMassProcessor { ... };      // 状态机
class UProceduralAnimationProcessor : public UMassProcessor { ... }; // 动画
class UWavePropagationProcessor : public UMassProcessor { ... };     // 人浪
class ULODProcessor : public UMassProcessor { ... };                 // LOD
```

**Processor 命名约定**：
- `U[功能]Processor`
- 动词 + 名词：`UUpdateHealthProcessor`、`UApplyDamageProcessor`

**默认行为**：
> 一个 Processor 一个文件，名字体现它做什么。

---

### 实践 28：用 Query 的 Access 声明约束读写

**原则**：明确声明只读/读写，MassEntity 可以优化并行。

**为什么重要**：
- ReadOnly Fragment 可以多 Processor 并行访问
- ReadWrite 需要独占访问
- 错误声明可能导致数据竞争

**怎么做**：

```cpp
void UMyProcessor::ConfigureQueries()
{
    // 只读访问 - 可与其他只读 Processor 并行
    EntityQuery.AddRequirement<FTeamFragment>(EMassFragmentAccess::ReadOnly);
    
    // 读写访问 - 独占
    EntityQuery.AddRequirement<FHealthFragment>(EMassFragmentAccess::ReadWrite);
    
    // 可选 Fragment（有就处理，没有就跳过）
    EntityQuery.AddOptionalRequirement<FShieldFragment>(EMassFragmentAccess::ReadWrite);
    
    // 排除有某个 Fragment 的 Entity
    EntityQuery.AddTagRequirement<FDeadTag>(EMassFragmentPresence::None);
}
```

**Access 类型速查**：
| 类型 | 说明 |
|------|------|
| `EMassFragmentAccess::ReadOnly` | 只读，可并行 |
| `EMassFragmentAccess::ReadWrite` | 读写，独占 |
| `EMassFragmentAccess::None` | 排除有此 Fragment 的 Entity |

**默认行为**：
> 能用 ReadOnly 就不用 ReadWrite。减少独占范围。

---

### 实践 29：用 Chunk 迭代，不要逐 Entity 查询

**原则**：ForEachEntityChunk 批量处理，不要 FindEntity 逐个查。

**为什么重要**：
- Chunk 内存连续，Cache 友好
- FindEntity 是 O(1) 但有查找开销
- 批量处理减少函数调用

**怎么做**：

```cpp
// ❌ 逐 Entity 查询
for (FMassEntityHandle Entity : AllEntities)
{
    FHealthFragment* Health = EntityManager.GetFragmentDataPtr<FHealthFragment>(Entity);
    if (Health)
    {
        Health->Current -= 1.0f;
    }
}

// ✅ 用 Query 批量处理
EntityQuery.ForEachEntityChunk(EntityManager, Context,
    [](FMassExecutionContext& Context)
    {
        TArrayView<FHealthFragment> Healths = Context.GetMutableFragmentView<FHealthFragment>();
        const int32 NumEntities = Context.GetNumEntities();
        
        for (int32 i = 0; i < NumEntities; ++i)
        {
            Healths[i].Current -= 1.0f;
        }
    });
```

**什么时候用 GetFragmentDataPtr**：
- 需要访问单个特定 Entity
- 事件驱动（收到消息后处理特定 Entity）
- 调试

**默认行为**：
> 常规逻辑用 ForEachEntityChunk，只有特殊情况用 GetFragmentDataPtr。

---

### 实践 30：用 Tag 而非 bool Fragment 表示状态

**原则**：二元状态用 Tag（有/无），而非 bool Fragment。

**为什么重要**：
- Tag 是零大小的标记，不占 Fragment 空间
- 可以用 AddTagRequirement 过滤
- 改变 Archetype 需要代价，但状态切换不频繁时这是值得的

**怎么做**：

```cpp
// ❌ 用 bool Fragment
USTRUCT()
struct FIsDeadFragment : public FMassFragment
{
    GENERATED_BODY()
    bool bIsDead = false;  // 每个 Entity 占 1 字节
};

// 每帧都要检查
for (int32 i = 0; i < NumEntities; ++i)
{
    if (!IsDeadFragments[i].bIsDead)
    {
        // 处理活着的
    }
}

// ✅ 用 Tag
USTRUCT()
struct FDeadTag : public FMassTag  // 注意是 FMassTag，不是 FMassFragment
{
    GENERATED_BODY()
};

// Query 自动过滤
void ConfigureQueries()
{
    // 只处理没有 DeadTag 的 Entity
    EntityQuery.AddTagRequirement<FDeadTag>(EMassFragmentPresence::None);
}

// Entity 死亡时添加 Tag
Context.Defer().AddTag<FDeadTag>(Entity);
```

**Tag 使用场景**：
- 死亡/存活
- 激活/未激活
- 已处理/未处理
- 任何二元状态

**默认行为**：
> 二元状态用 Tag，多状态用 enum Fragment。

---

### 实践 31：Processor 依赖用 ExecutionOrder 声明

**原则**：显式声明 Processor 执行顺序，不要依赖偶然顺序。

**为什么重要**：
- 有些 Processor 必须在另一个之后执行
- 不声明依赖 = 依赖不确定顺序 = 偶发 Bug
- 文档化执行流程

**怎么做**：

```cpp
// SpectatorStateProcessor.cpp
USpectatorStateProcessor::USpectatorStateProcessor()
{
    // 设置执行组
    ExecutionOrder.ExecuteInGroup = UE::Mass::ProcessorGroupNames::Behavior;
    
    // 在这些 Processor 之后执行
    ExecutionOrder.ExecuteAfter.Add(UE::Mass::ProcessorGroupNames::Movement);
    
    // 在这些 Processor 之前执行
    ExecutionOrder.ExecuteBefore.Add(TEXT("UAnimationProcessor"));
}

// AnimationProcessor.cpp
UAnimationProcessor::UAnimationProcessor()
{
    ExecutionOrder.ExecuteInGroup = UE::Mass::ProcessorGroupNames::Behavior;
    // 不需要再声明 ExecuteAfter，因为 StateProcessor 已经声明了
}
```

**常用执行组**：
```cpp
UE::Mass::ProcessorGroupNames::Tasks        // 任务
UE::Mass::ProcessorGroupNames::Behavior     // 行为
UE::Mass::ProcessorGroupNames::Movement     // 移动
UE::Mass::ProcessorGroupNames::Avoidance    // 避让
UE::Mass::ProcessorGroupNames::LOD          // LOD
UE::Mass::ProcessorGroupNames::Representation  // 渲染
```

**默认行为**：
> 每个 Processor 构造函数都设置 ExecuteInGroup 和必要的依赖。

---

### 实践 32：用 Shared Fragment 共享只读数据

**原则**：多个 Entity 共享的只读数据用 Shared Fragment。

**为什么重要**：
- 普通 Fragment 每个 Entity 一份
- Shared Fragment 同 Archetype 共享一份
- 节省内存，便于批量更新

**怎么做**：

```cpp
// 共享配置（如：同一队伍的颜色）
USTRUCT()
struct FTeamConfigSharedFragment : public FMassSharedFragment  // 注意是 Shared
{
    GENERATED_BODY()
    
    FLinearColor TeamColor;
    float CheerProbability;
};

// 访问方式不同
void UMyProcessor::Execute(...)
{
    EntityQuery.ForEachEntityChunk(..., [](FMassExecutionContext& Context)
    {
        // Shared Fragment 每个 Chunk 一个（不是每个 Entity 一个）
        const FTeamConfigSharedFragment& TeamConfig = 
            Context.GetSharedFragment<FTeamConfigSharedFragment>();
        
        // 所有 Entity 共享同一个 TeamConfig
        for (int32 i = 0; i < NumEntities; ++i)
        {
            if (FMath::FRand() < TeamConfig.CheerProbability)
            {
                // ...
            }
        }
    });
}
```

**什么数据适合 Shared**：
- 配置/设置（同类型 Entity 共享）
- 只读的引用数据
- 不因 Entity 而异的参数

**默认行为**：
> 多个 Entity 共享的只读配置用 Shared Fragment。

---

### 实践 33：用 Trait 组合 Fragment，而非手动添加

**原则**：Trait 定义"这类 Entity 有哪些 Fragment"。

**为什么重要**：
- 手动添加 Fragment 容易遗漏
- Trait 是可复用的模板
- 可以组合多个 Trait

**怎么做**：

```cpp
// ========== SpectatorTrait.h ==========
UCLASS()
class USpectatorTrait : public UMassEntityTraitBase
{
    GENERATED_BODY()

public:
    // 可配置参数
    UPROPERTY(EditAnywhere)
    float InitialHealth = 100.0f;

protected:
    virtual void BuildTemplate(FMassEntityTemplateBuildContext& BuildContext, const UWorld& World) const override;
};

// ========== SpectatorTrait.cpp ==========
void USpectatorTrait::BuildTemplate(FMassEntityTemplateBuildContext& BuildContext, const UWorld& World) const
{
    // 添加 Fragment
    BuildContext.RequireFragment<FTransformFragment>();
    BuildContext.RequireFragment<FSpectatorStateFragment>();
    BuildContext.RequireFragment<FHealthFragment>();
    
    // 设置默认值
    BuildContext.GetMutableObjectFragmentInitializers().Add(
        [this](UObject& Owner, FMassEntityView& EntityView, const EMassTranslationDirection Direction)
        {
            FHealthFragment& Health = EntityView.GetFragmentData<FHealthFragment>();
            Health.Max = InitialHealth;
            Health.Current = InitialHealth;
        });
}
```

**使用方式**：
1. 在编辑器创建 DataAsset
2. 添加 Trait
3. 配置参数
4. 用 MassSpawner 生成

**默认行为**：
> 定义新类型的 Entity 时，创建对应的 Trait。

---

### 实践 34：Entity 销毁用 Defer，不要立即删除

**原则**：在 Processor Execute 中不能直接删除 Entity，用 Defer 延迟。

**为什么重要**：
- Execute 中正在遍历 Entity
- 直接删除会导致迭代器失效
- Defer 在安全的时机执行

**怎么做**：

```cpp
// ❌ 直接删除
void UHealthProcessor::Execute(...)
{
    EntityQuery.ForEachEntityChunk(..., [&EntityManager](FMassExecutionContext& Context)
    {
        for (int32 i = 0; i < NumEntities; ++i)
        {
            if (Healths[i].Current <= 0)
            {
                EntityManager.DestroyEntity(Entities[i]);  // 💥 遍历中删除！
            }
        }
    });
}

// ✅ 用 Defer
void UHealthProcessor::Execute(...)
{
    EntityQuery.ForEachEntityChunk(..., [](FMassExecutionContext& Context)
    {
        TConstArrayView<FMassEntityHandle> Entities = Context.GetEntities();
        TArrayView<FHealthFragment> Healths = Context.GetMutableFragmentView<FHealthFragment>();
        
        for (int32 i = 0; i < Context.GetNumEntities(); ++i)
        {
            if (Healths[i].Current <= 0)
            {
                Context.Defer().DestroyEntity(Entities[i]);  // 安全！延迟执行
            }
        }
    });
}
```

**Defer 可以做的事**：
```cpp
Context.Defer().DestroyEntity(Entity);           // 销毁
Context.Defer().AddTag<FDeadTag>(Entity);        // 添加 Tag
Context.Defer().RemoveTag<FAliveTag>(Entity);    // 移除 Tag
Context.Defer().AddFragment<FNewFragment>(Entity); // 添加 Fragment
Context.Defer().SwapTags<FOld, FNew>(Entity);    // 替换 Tag
```

**默认行为**：
> Execute 中任何修改 Entity 结构的操作都用 Defer。

---

### 实践 35：Signal 触发状态变化，而非每帧检查

**原则**：用 Signal/Event 触发变化，不要每帧轮询。

**为什么重要**：
- 10 万 Entity 每帧检查"进球了吗" = 浪费
- Signal 只在事件发生时触发
- 减少无用计算

**怎么做**：

```cpp
// ❌ 每帧检查全局状态
void USpectatorReactionProcessor::Execute(...)
{
    // 每帧都获取游戏状态
    AStadiumGameState* GameState = GetWorld()->GetGameState<AStadiumGameState>();
    
    EntityQuery.ForEachEntityChunk(..., [GameState](FMassExecutionContext& Context)
    {
        for (int32 i = 0; i < NumEntities; ++i)
        {
            if (GameState->bGoalJustScored)  // 10 万次检查同一个 bool
            {
                // 反应
            }
        }
    });
}

// ✅ Signal 驱动
// 1. 定义 Signal
USTRUCT()
struct FGoalScoredSignal : public FMassSignal
{
    GENERATED_BODY()
    uint8 TeamID;
};

// 2. 进球时发送 Signal
void AStadiumGameState::OnGoalScored(uint8 TeamID)
{
    FGoalScoredSignal Signal;
    Signal.TeamID = TeamID;
    GetWorld()->GetSubsystem<UMassSignalSubsystem>()->SignalEntities(..., Signal);
}

// 3. Processor 只处理收到 Signal 的 Entity
void USpectatorReactionProcessor::ConfigureQueries()
{
    EntityQuery.AddRequirement<FGoalScoredSignal>(EMassFragmentAccess::ReadOnly);
}
```

**Signal 使用场景**：
- 进球、犯规等游戏事件
- 区域进入/离开
- 任何"偶发事件"

**默认行为**：
> "偶发事件"用 Signal，"持续状态"用每帧检查。

---

## 第六章：渲染与可视化

### 实践 36：用 ISM/HISM，不要用独立 Actor

**原则**：批量渲染用 Instanced Static Mesh，不要每个 Entity 一个 Actor。

**为什么重要**：
- 1 个 Actor = 1 个 UObject + Transform + 组件 = 几 KB
- 10 万个 Actor = 1 GB+ 内存
- ISM 一次 Draw Call 渲染所有实例

**怎么做**：

```cpp
// ❌ 每个 Entity 一个 Actor
for (int32 i = 0; i < 100000; ++i)
{
    GetWorld()->SpawnActor<ASpectator>(...);  // 10 万个 Actor！
}

// ✅ 用 ISM
UPROPERTY()
UInstancedStaticMeshComponent* SpectatorMesh;

void ASpectatorRenderer::Initialize()
{
    SpectatorMesh = CreateDefaultSubobject<UInstancedStaticMeshComponent>(TEXT("Mesh"));
    SpectatorMesh->SetStaticMesh(CapsuleMesh);
    SpectatorMesh->SetMaterial(0, SpectatorMaterial);
    SpectatorMesh->NumCustomDataFloats = 4;  // 自定义数据（颜色等）
}

void ASpectatorRenderer::UpdateInstances(const TArray<FTransform>& Transforms)
{
    // 批量更新所有实例
    SpectatorMesh->BatchUpdateInstancesTransforms(0, Transforms, true, true, true);
}
```

**ISM vs HISM**：
| 类型 | 特点 | 用途 |
|------|------|------|
| `UInstancedStaticMeshComponent` | 简单，每帧更新全部 | 动态实例 |
| `UHierarchicalInstancedStaticMeshComponent` | 支持 LOD、Culling | 大规模静态场景 |

**默认行为**：
> 大规模同类物体用 ISM。只有需要独立行为的才用 Actor。

---

### 实践 37：LOD 是必须的，不是可选的

**原则**：大规模模拟必须有 LOD，否则无法达到目标帧率。

**为什么重要**：
- 远处的细节人眼看不到
- 渲染远处和近处一样精细 = 浪费
- LOD 减少顶点数、更新频率

**怎么做**：

```cpp
// LOD Fragment
USTRUCT()
struct FLODFragment : public FMassFragment
{
    GENERATED_BODY()
    
    uint8 LODLevel = 0;  // 0=近, 1=中, 2=远, 3=culled
    float DistanceToCamera = 0.0f;
};

// LOD Processor
void ULODProcessor::Execute(...)
{
    const FVector CameraLocation = GetCameraLocation();
    
    EntityQuery.ForEachEntityChunk(..., [CameraLocation](FMassExecutionContext& Context)
    {
        TConstArrayView<FTransformFragment> Transforms = Context.GetFragmentView<FTransformFragment>();
        TArrayView<FLODFragment> LODs = Context.GetMutableFragmentView<FLODFragment>();
        
        for (int32 i = 0; i < Context.GetNumEntities(); ++i)
        {
            float Distance = FVector::Dist(Transforms[i].GetTransform().GetLocation(), CameraLocation);
            LODs[i].DistanceToCamera = Distance;
            
            if (Distance < 5000.0f)       // 50m
                LODs[i].LODLevel = 0;
            else if (Distance < 20000.0f) // 200m
                LODs[i].LODLevel = 1;
            else if (Distance < 50000.0f) // 500m
                LODs[i].LODLevel = 2;
            else
                LODs[i].LODLevel = 3;     // Culled
        }
    });
}

// 根据 LOD 调整更新频率
if (LOD.LODLevel == 0)
    UpdateEveryFrame();
else if (LOD.LODLevel == 1)
    UpdateEveryNFrames(2);
else if (LOD.LODLevel == 2)
    UpdateEveryNFrames(5);
// LOD 3 不更新
```

**LOD 策略速查**：
| 距离 | Mesh | 动画 | 更新 |
|------|------|------|------|
| Near | 完整 | 完整 | 每帧 |
| Mid | 简化 | 简化 | 每 2 帧 |
| Far | Point/Billboard | 无 | 每 5 帧 |
| Culled | 无 | 无 | 无 |

**默认行为**：
> 一开始就设计 LOD 系统，不要等优化阶段再加。

---

### 实践 38：用 MassRepresentation 管理渲染

**原则**：MassEntity 有内置的渲染系统，不要自己造。

**为什么重要**：
- MassRepresentation 已经处理了 LOD、ISM、Culling
- 与 MassEntity 深度集成
- 减少自己写的代码

**怎么做**：

```cpp
// 1. 添加依赖
// Build.cs
PrivateDependencyModuleNames.Add("MassRepresentation");

// 2. 使用 FMassRepresentationFragment
USTRUCT()
struct FSpectatorRepresentationFragment : public FMassRepresentationFragment
{
    GENERATED_BODY()
};

// 3. 配置 Trait
void USpectatorTrait::BuildTemplate(FMassEntityTemplateBuildContext& BuildContext, const UWorld& World) const
{
    BuildContext.RequireFragment<FMassRepresentationFragment>();
    BuildContext.RequireFragment<FMassRepresentationLODFragment>();
    // MassRepresentation Processor 会自动处理
}
```

**注意**：MassRepresentation 是 UE5 的实验性模块，API 可能变化。

**默认行为**：
> 先尝试用 MassRepresentation。不满足需求再自己写。

---

### 实践 39：程序化动画用数学，不用骨骼

**原则**：大规模模拟不用骨骼动画，用数学计算 Transform。

**为什么重要**：
- 骨骼动画每个角色需要独立计算
- 10 万个骨骼动画 = 不可能
- 数学动画（正弦、插值）开销极低

**怎么做**：

```cpp
// 程序化动画 Fragment
USTRUCT()
struct FProceduralAnimFragment : public FMassFragment
{
    GENERATED_BODY()
    
    float Phase = 0.0f;        // 相位偏移（错开动画）
    float BreathScale = 1.0f;  // 呼吸缩放
    float SwayAngle = 0.0f;    // 摇摆角度
    float Height = 0.0f;       // 高度偏移（站/坐）
};

// 程序化动画 Processor
void UProceduralAnimProcessor::Execute(...)
{
    const float Time = GetWorld()->GetTimeSeconds();
    
    EntityQuery.ForEachEntityChunk(..., [Time](FMassExecutionContext& Context)
    {
        TArrayView<FProceduralAnimFragment> Anims = Context.GetMutableFragmentView<FProceduralAnimFragment>();
        TArrayView<FTransformFragment> Transforms = Context.GetMutableFragmentView<FTransformFragment>();
        TConstArrayView<FSpectatorStateFragment> States = Context.GetFragmentView<FSpectatorStateFragment>();
        
        for (int32 i = 0; i < Context.GetNumEntities(); ++i)
        {
            const float Phase = Anims[i].Phase;
            
            // 呼吸：Y 轴缩放正弦波
            Anims[i].BreathScale = 1.0f + 0.02f * FMath::Sin(Time * 2.0f + Phase);
            
            // 摇摆：Z 轴旋转正弦波
            Anims[i].SwayAngle = 3.0f * FMath::Sin(Time * 1.5f + Phase);
            
            // 站/坐：高度插值
            float TargetHeight = (States[i].State == ESpectatorState::Standing) ? 50.0f : 0.0f;
            Anims[i].Height = FMath::FInterpTo(Anims[i].Height, TargetHeight, DeltaTime, 5.0f);
            
            // 应用到 Transform
            FTransform& T = Transforms[i].GetMutableTransform();
            T.SetScale3D(FVector(1.0f, Anims[i].BreathScale, 1.0f));
            T.SetRotation(FQuat(FVector::UpVector, FMath::DegreesToRadians(Anims[i].SwayAngle)));
            T.AddToTranslation(FVector(0, 0, Anims[i].Height));
        }
    });
}
```

**常用程序化动画技巧**：
| 效果 | 实现 |
|------|------|
| 呼吸 | Y 缩放正弦 |
| 摇摆 | Z 旋转正弦 |
| 站起 | Y 位移插值 |
| 欢呼 | Y 位移正弦 + 快速 |
| 沮丧 | Y 位移负值 + 前倾 |

**默认行为**：
> 数学函数 + 插值 = 程序化动画。不用骨骼。

---

### 实践 40：材质实例共享，参数用 Per-Instance Custom Data

**原则**：所有实例用同一个材质，颜色等差异用 Custom Data 传递。

**为什么重要**：
- 每个材质实例 = 1 个 Draw Call
- 10 种颜色 = 10 个 Draw Call（可接受）
- 10 万种颜色 = 10 万 Draw Call（灾难）

**怎么做**：

```cpp
// 1. ISM 设置 Custom Data
SpectatorMesh->NumCustomDataFloats = 4;  // RGBA

// 2. 每个实例设置颜色
void SetInstanceColor(int32 InstanceIndex, FLinearColor Color)
{
    SpectatorMesh->SetCustomDataValue(InstanceIndex, 0, Color.R);
    SpectatorMesh->SetCustomDataValue(InstanceIndex, 1, Color.G);
    SpectatorMesh->SetCustomDataValue(InstanceIndex, 2, Color.B);
    SpectatorMesh->SetCustomDataValue(InstanceIndex, 3, Color.A);
}

// 3. 材质中读取 Custom Data
// 在材质编辑器中：
// PerInstanceCustomData 节点 → 读取 Index 0-3 → 作为颜色
```

**材质设置**：
1. 创建材质
2. 添加 `PerInstanceCustomData` 节点
3. 设置 Data Index = 0, 1, 2, 3
4. 连接到 Base Color

**默认行为**：
> 一个材质 + Custom Data 实现差异。不要每个实例一个材质。

---

## 第七章：调试与诊断

### 实践 41：用 UE_LOG 分级日志

**原则**：使用正确的日志级别，便于过滤。

**为什么重要**：
- Log 输出可以按级别过滤
- Error/Warning 在输出中高亮
- 便于区分正常信息和问题

**怎么做**：

```cpp
// 1. 定义 Log Category（模块头文件）
DECLARE_LOG_CATEGORY_EXTERN(LogSpectator, Log, All);

// 2. 实现（模块 cpp 文件）
DEFINE_LOG_CATEGORY(LogSpectator);

// 3. 使用
UE_LOG(LogSpectator, Log, TEXT("Normal info: %d entities"), Count);
UE_LOG(LogSpectator, Warning, TEXT("Performance warning: %f ms"), Time);
UE_LOG(LogSpectator, Error, TEXT("Failed to spawn entity!"));
UE_LOG(LogSpectator, Fatal, TEXT("Critical error, crashing!"));  // 会崩溃

// 4. 格式化
UE_LOG(LogSpectator, Log, TEXT("Entity %s at %s"), 
    *Entity.ToString(), 
    *Location.ToString());
```

**日志级别速查**：
| 级别 | 用途 | 颜色 |
|------|------|------|
| `Log` | 正常信息 | 灰色 |
| `Display` | 重要信息 | 灰色 |
| `Warning` | 潜在问题 | 黄色 |
| `Error` | 错误 | 红色 |
| `Fatal` | 致命错误（崩溃） | 红色 |

**控制台过滤**：
```
Log LogSpectator Warning    # 只显示 Warning 及以上
Log LogSpectator off        # 关闭这个 Category
```

**默认行为**：
> 每个模块定义自己的 Log Category。正确使用级别。

---

### 实践 42：用 stat 命令监控性能

**原则**：stat 命令是性能分析的第一工具。

**为什么重要**：
- 不需要 Profiler
- 实时显示
- 覆盖主要性能指标

**常用命令**：

```
stat fps                  # 帧率 + 帧时间
stat unit                 # Game/Draw/GPU 时间分解
stat unitgraph            # 图形化显示

stat Mass                 # MassEntity 统计
stat MassProcessor        # 各 Processor 耗时

stat scenerendering       # 渲染统计
stat rhi                  # GPU 统计
stat memory               # 内存统计

stat startfile            # 开始录制 profiling
stat stopfile             # 停止录制，生成 .ue4stats 文件
```

**输出解读**：
```
Frame: 16.6ms (60 FPS)
  Game: 5.2ms        # 游戏逻辑（Tick、Processor）
  Draw: 3.1ms        # CPU 渲染准备
  GPU: 8.3ms         # GPU 渲染
```

**默认行为**：
> 开发时常开 `stat fps`。性能问题时开 `stat unit`。

---

### 实践 43：用 Visual Logger 可视化调试

**原则**：复杂行为用 Visual Logger 录制回放。

**为什么重要**：
- 可以"倒带"查看历史状态
- 可视化比文本 Log 直观
- 可以录制到文件离线分析

**怎么做**：

```cpp
// 启用（需要 include）
#include "VisualLogger/VisualLogger.h"

// 记录位置
UE_VLOG_LOCATION(this, LogSpectator, Log, Location, 10.0f, FColor::Red, TEXT("Entity %d"), ID);

// 记录方向
UE_VLOG_ARROW(this, LogSpectator, Log, Start, End, FColor::Green, TEXT("Velocity"));

// 记录形状
UE_VLOG_BOX(this, LogSpectator, Log, Box, FColor::Blue, TEXT("Bounds"));

// 记录文本
UE_VLOG(this, LogSpectator, Log, TEXT("State: %s"), *StateString);
```

**查看方式**：
1. 编辑器菜单：Window → Developer Tools → Visual Logger
2. 播放游戏
3. Visual Logger 自动录制
4. 可以暂停、回放、查看历史

**默认行为**：
> AI/行为调试用 Visual Logger，比文本 Log 清晰 10 倍。

---

### 实践 44：用 Gameplay Debugger 检查 Entity 状态

**原则**：运行时按 `'`（撇号）打开 Gameplay Debugger。

**为什么重要**：
- 无需代码就能检查游戏状态
- 支持自定义 Category
- MassEntity 有内置支持

**怎么做**：

1. 运行游戏
2. 按 `'`（撇号键）打开
3. 用数字键切换 Category
4. 可以锁定特定 Actor/Entity

**自定义 Category**（可选）：
```cpp
// 继承 UGameplayDebuggerCategory
UCLASS()
class USpectatorDebuggerCategory : public UGameplayDebuggerCategory
{
    // ...
};

// 注册
UGameplayDebuggerCategoryReplicator::RegisterCategory(
    TEXT("Spectator"),
    []() { return MakeShared<FGameplayDebuggerCategory_Spectator>(); },
    ...
);
```

**默认行为**：
> 运行时检查状态先按 `'` 看看 Gameplay Debugger 有什么。

---

### 实践 45：用 check() 和 ensure() 断言

**原则**：用断言捕获不应该发生的情况。

**为什么重要**：
- 早发现问题，早崩溃，早修复
- 比空指针崩溃信息更有用
- 文档化假设

**怎么做**：

```cpp
// check - 失败时崩溃（Debug/Development）
void ProcessEntity(FMassEntityHandle Entity)
{
    check(Entity.IsValid());  // 如果无效，立即崩溃
    // ...
}

// checkf - 带消息的 check
void SetHealth(float Value)
{
    checkf(Value >= 0, TEXT("Health cannot be negative: %f"), Value);
    Health = Value;
}

// ensure - 失败时只报错，不崩溃（Debug/Development）
void MaybeDoSomething(AActor* Actor)
{
    if (!ensure(Actor != nullptr))  // 不崩溃，但输出错误
    {
        return;
    }
    Actor->DoSomething();
}

// ensureMsgf - 带消息的 ensure
if (!ensureMsgf(Index < Array.Num(), TEXT("Index %d out of bounds"), Index))
{
    return;
}

// verify - 表达式始终执行，但只在 Debug 检查结果
verify(SomeImportantFunction());
```

**断言类型速查**：
| 类型 | 失败行为 | Shipping 版本 |
|------|---------|--------------|
| `check` | 崩溃 | 不执行 |
| `ensure` | 报错继续 | 不执行 |
| `verify` | 崩溃 | 执行但不检查 |

**默认行为**：
> 函数入口检查参数用 `check`。可能失败但能恢复用 `ensure`。

---

## 第八章：工程实践

### 实践 46：每次提交前本地全编译

**原则**：提交前确保 Development 和 Shipping 都能编译。

**为什么重要**：
- CI 编译失败会阻塞团队
- Shipping 编译可能有 Debug 没有的错误
- 本地编译比等 CI 快

**怎么做**：

```bash
# 命令行编译
# Windows
Engine\Build\BatchFiles\Build.bat StadiumSimEditor Win64 Development

# 或者用 IDE
# VS: Build → Build Solution
# Rider: Build → Build Solution
```

**提交前 Checklist**：
- [ ] Development Editor 编译通过
- [ ] 没有新的 Warning（或已知的）
- [ ] 基本功能手动测试过
- [ ] 提交信息清晰

**默认行为**：
> 改完代码 → 编译 → 测试 → 提交。不要跳过编译。

---

### 实践 47：用 .uplugin 封装可复用系统

**原则**：可复用的系统封装成 Plugin。

**为什么重要**：
- 可以在多个项目使用
- 明确的 API 边界
- 便于分享/开源

**怎么做**：

```
Plugins/
└── MassSpectator/
    ├── MassSpectator.uplugin    # 插件描述文件
    ├── Source/
    │   └── MassSpectator/
    │       ├── Public/
    │       ├── Private/
    │       └── MassSpectator.Build.cs
    └── Resources/
        └── Icon128.png
```

```json
// MassSpectator.uplugin
{
    "FileVersion": 3,
    "Version": 1,
    "VersionName": "1.0",
    "FriendlyName": "Mass Spectator",
    "Description": "Large-scale spectator simulation",
    "Category": "Gameplay",
    "CreatedBy": "Your Name",
    "Modules": [
        {
            "Name": "MassSpectator",
            "Type": "Runtime",
            "LoadingPhase": "Default"
        }
    ],
    "Plugins": [
        {
            "Name": "MassEntity",
            "Enabled": true
        }
    ]
}
```

**何时做成 Plugin**：
- 计划在多个项目使用
- 功能相对独立完整
- 想开源或分享

**默认行为**：
> 项目初期不急着做 Plugin。功能稳定后再抽取。

---

### 实践 48：Config 文件用于可调参数

**原则**：需要调整的参数放 Config，不要硬编码。

**为什么重要**：
- 不用重编译就能调参
- 便于不同环境不同配置
- 便于非程序员调整

**怎么做**：

```ini
; Config/DefaultGame.ini
[/Script/StadiumSim.SpectatorSettings]
NumSpectators=100000
WaveSpeed=50.0
LODNearDistance=5000.0
LODMidDistance=20000.0
```

```cpp
// SpectatorSettings.h
UCLASS(Config=Game, DefaultConfig)
class USpectatorSettings : public UDeveloperSettings
{
    GENERATED_BODY()
    
public:
    UPROPERTY(Config, EditAnywhere)
    int32 NumSpectators = 100000;
    
    UPROPERTY(Config, EditAnywhere)
    float WaveSpeed = 50.0f;
    
    // 获取单例
    static const USpectatorSettings* Get()
    {
        return GetDefault<USpectatorSettings>();
    }
};

// 使用
int32 Num = USpectatorSettings::Get()->NumSpectators;
```

**Config 文件层次**：
| 文件 | 用途 |
|------|------|
| `DefaultX.ini` | 默认值 |
| `X.ini` | 用户/项目覆盖 |
| `UserX.ini` | 用户本地覆盖（不提交） |

**默认行为**：
> 会调整的数值用 Config。只有真正的常量才硬编码。

---

### 实践 49：用 Subsystem 管理全局服务

**原则**：全局服务用 Subsystem，不要用单例或静态变量。

**为什么重要**：
- Subsystem 有正确的生命周期
- 自动创建/销毁
- 可以被 DI 注入（测试友好）

**怎么做**：

```cpp
// WorldSubsystem - 每个 World 一个实例
UCLASS()
class USpectatorSubsystem : public UWorldSubsystem
{
    GENERATED_BODY()
    
public:
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;
    
    void TriggerWave(FVector Origin);
    
private:
    TArray<FWaveData> ActiveWaves;
};

// 使用
USpectatorSubsystem* Subsystem = GetWorld()->GetSubsystem<USpectatorSubsystem>();
Subsystem->TriggerWave(Origin);
```

**Subsystem 类型速查**：
| 类型 | 生命周期 | 用途 |
|------|---------|------|
| `UEngineSubsystem` | 引擎启动-关闭 | 引擎级服务 |
| `UEditorSubsystem` | 编辑器启动-关闭 | 编辑器扩展 |
| `UGameInstanceSubsystem` | 游戏启动-退出 | 跨关卡服务 |
| `UWorldSubsystem` | World 创建-销毁 | 关卡级服务 |
| `ULocalPlayerSubsystem` | 玩家加入-退出 | 玩家服务 |

**默认行为**：
> 全局服务用 Subsystem。不要自己写单例。

---

### 实践 50：写代码时想象它会被审查

**原则**：写每一行代码时，想象有高级工程师在审查。

**为什么重要**：
- 你正在准备的项目是面试作品
- 代码质量 = 第一印象
- 好习惯会自然形成

**审查 Checklist**：

**命名**：
- [ ] 变量名能看出是什么
- [ ] 函数名能看出做什么
- [ ] 命名一致（不混用 Count/Num/Size）

**结构**：
- [ ] 函数不超过 50 行
- [ ] 一个函数只做一件事
- [ ] 嵌套不超过 3 层

**注释**：
- [ ] 复杂逻辑有注释说明"为什么"
- [ ] 不注释显而易见的代码
- [ ] TODO 有关联的 Issue/任务

**性能**：
- [ ] 热路径没有堆分配
- [ ] 没有多余的复制
- [ ] 有性能测量代码

**安全**：
- [ ] 指针使用前检查
- [ ] 数组访问前检查范围
- [ ] 错误有合理处理

**默认行为**：
> 写完代码，自己先审查一遍，再提交。

---

## 附录：快速参考卡

### A. 每日 Checklist

**开始开发前**：
- [ ] 同步最新代码
- [ ] 编译确认无错误

**开发中**：
- [ ] 新 UObject 指针加 UPROPERTY()
- [ ] 热路径避免分配
- [ ] 用 SCOPE_CYCLE_COUNTER 测量

**提交前**：
- [ ] 本地编译通过
- [ ] 基本功能测试
- [ ] 代码自审一遍

### B. 常用快捷键

| 功能 | VS | Rider |
|------|-----|-------|
| 编译 | Ctrl+Shift+B | Ctrl+Shift+B |
| 跳转定义 | F12 | Ctrl+Click |
| 查找引用 | Shift+F12 | Alt+F7 |
| 重命名 | Ctrl+R+R | Shift+F6 |

### C. 常用控制台命令

```
stat fps / stat unit / stat unitgraph
stat Mass / stat MassProcessor
stat scenerendering / stat rhi
log LogSpectator Verbose
```

### D. 紧急排错

**编译错误 "unresolved external"**
→ 检查 Build.cs 依赖

**运行时崩溃 "Access Violation"**
→ 检查 UPROPERTY() 是否遗漏

**MassEntity Processor 不执行**
→ 检查 ConfigureQueries() 和 RegisterWithProcessor()

**渲染不出来**
→ 检查 Transform、Mesh、Material 是否设置

---

**最后更新**: 2026-01-21
**作者**: Technical Mentor (AI Assistant)

---

> "Excellence is not an act, but a habit." — Aristotle
>
> 把这些实践变成习惯，你就已经是 Senior 了。
