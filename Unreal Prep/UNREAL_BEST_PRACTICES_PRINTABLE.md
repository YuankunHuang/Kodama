# Unreal 大规模模拟：内功心法

---

## 内存生死线

- **所有 UObject 指针必须 UPROPERTY，没有例外。** 不标记 = GC 不追踪 = 野指针 = 随机崩溃。

- **10 万个实体用 struct，不用 class。** UObject 太重，FStruct 才是大规模的答案。

- **构造函数只设默认值，游戏逻辑放 BeginPlay。** 构造函数里 World 可能不存在。

- **裸指针 new 了必须 delete，不想管就用智能指针。** TUniquePtr 独占，TSharedPtr 共享。

- **Transient 标记运行时临时数据。** 不需要存盘的东西别让引擎帮你序列化。

---

## 性能直觉

- **不测量就不优化。** SCOPE_CYCLE_COUNTER 是真相，直觉是谎言。

- **热路径零分配。** 预分配 TArray，用 Reset() 清空重用，别每帧 new。

- **Reserve 再 Add，别让数组自己扩容。** 每次扩容都是一次全量复制。

- **标识符用 FName，显示文本用 FText，拼接才用 FString。** FName 比较是整数比较，O(1)。

- **不需要 Tick 的 Actor 就关掉 Tick。** 默认 bCanEverTick = false。

- **连续内存比指针数组快 100 倍。** Cache line 是 64 字节，指针追逐是性能杀手。

- **批量处理，别逐个调用。** 一次处理 1000 个，比 1000 次处理 1 个快得多。

---

## MassEntity 本质

- **Entity 只是个 ID，不是对象。** 轻如鸿毛，可以有百万个。

- **Fragment 是纯数据，不带方法。** 有方法就不是 DOD，就失去了批量处理的优势。

- **Processor 是上帝视角，批量处理同类 Entity。** 不是让每个 Entity 自己决定做什么。

- **ForEachEntityChunk 是唯一正确的遍历方式。** Chunk 内存连续，Cache 命中率高。

- **Query 声明 ReadOnly 能并行，ReadWrite 要独占。** 能只读就别读写。

- **二元状态用 Tag，多值状态用 enum Fragment。** Tag 零存储，只是个标记。

- **Execute 里不直接删 Entity，用 Defer。** 遍历时删除 = 迭代器失效 = 崩溃。

- **Processor 执行顺序用 ExecutionOrder 显式声明。** 别依赖偶然顺序。

---

## 渲染大规模

- **一万个同样的东西，一个 Draw Call。** ISM (Instanced Static Mesh) 是唯一答案。

- **LOD 不是优化，是必须。** 远处看不见细节，渲染它就是浪费。

- **程序化动画用数学，不用骨骼。** 正弦函数 + 插值，10 万个无压力。

- **实例颜色用 Custom Data，不用单独材质。** 一个材质 + Per-Instance 数据。

---

## 代码组织

- **头文件前向声明，源文件才 include。** 头文件 include 大头文件 = 编译时间爆炸。

- **链接错误先查 Build.cs。** 90% 是忘加依赖模块。

- **一个类一对文件。** MyClass.h + MyClass.cpp，不要挤一起。

- **编译慢是正常的，接受它。** 首次 10 分钟，增量 30 秒，这是 C++。

---

## 调试本能

- **stat fps 看帧率，stat unit 看瓶颈。** Game/Draw/GPU 哪个慢一目了然。

- **stat Mass 看 MassEntity 性能。** 每个 Processor 耗时多少。

- **check() 断言必须为真，ensure() 报错但继续。** 前者崩溃定位问题，后者容错继续。

- **UE_LOG 分级别：Log/Warning/Error。** Warning 黄色，Error 红色，一眼看到。

---

## 工程习惯

- **提交前本地编译。** 编不过别推，别让队友等 CI 失败。

- **可调参数放 Config。** 改数值不用重编译。

- **全局服务用 Subsystem。** 别自己写单例，Unreal 有正确的生命周期管理。

- **写代码时假装有人在审查。** 这是你的面试作品，每一行都代表你的水平。

---

## 思维根基

- **先问 "Unreal 怎么做"，再问 "我想怎么做"。** 顺着框架走，别对抗框架。

- **继承是默认选项。** ACharacter 自带移动、碰撞、动画，别自己拼。

- **C++ 定骨架，Blueprint 填血肉。** 系统用 C++，变体和调参用 BP。

- **不要翻译 Unity 代码，要重新设计。** 两个引擎的最佳实践不一样。

- **DOD 的核心是数据连续、批量处理。** 这个思想跨引擎通用。

---

## 一句话总结

> **大规模模拟的本质：数据连续存储，逻辑批量执行，渲染合批提交。**

---

## 紧急排错

- **"unresolved external symbol"** → Build.cs 缺模块依赖
- **"Access Violation"** → UPROPERTY 遗漏或空指针
- **Processor 不执行** → ConfigureQueries 配置错误
- **渲染不出来** → Transform/Mesh/Material 哪个没设

---

*把这些变成本能反应，你就是真正懂行的人。*
