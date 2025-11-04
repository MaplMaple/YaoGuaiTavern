# 致命伤害系统使用指南

## 📋 系统概述

致命伤害系统允许某些物体或区域对玩家造成致命伤害，直接将玩家传送回最近的存档点。适用于跳跳乐路线、岩浆、深渊等危险区域。

## 🎯 两种伤害类型

### 1. 普通伤害
- 调用：`PlayerController.OnTakeDamage(attackerPosition)`
- 效果：击退玩家，短暂无敌时间
- 用于：一般敌人、轻微陷阱

### 2. 致命伤害 ⚠️
- 调用：`PlayerController.OnTakeFatalDamage(attackerPosition)`
- 效果：直接送回存档点
- 用于：跳跳乐尖刺、岩浆、深渊

## 🔧 使用方法

### 方法 1: FatalSpike（致命尖刺）

适用于跳跳乐路线上的尖刺。

**设置步骤：**
1. 创建 GameObject（尖刺）
2. 添加 `FatalSpike` 组件
3. 添加 `Collider2D` 组件
4. 配置参数：
   - **Is Fatal**: 勾选 = 致命伤害，不勾选 = 普通伤害

```csharp
// FatalSpike 会自动处理碰撞
public class FatalSpike : MonoBehaviour
{
    public bool isFatal = true;  // 是否为致命伤害
    
    // 碰撞时自动调用对应的伤害方法
}
```

### 方法 2: KillZone（死亡区域）

适用于岩浆、深渊等大面积危险区域。

**设置步骤：**
1. 创建 GameObject（可以是空对象）
2. 添加 `KillZone` 组件
3. 添加 `Collider2D` 组件（会自动设置为 Trigger）
4. 调整碰撞器大小覆盖危险区域
5. 配置参数：
   - **Zone Type**: 选择区域类型（Lava/Abyss/Poison 等）
   - **Show Warning Gizmos**: 是否在编辑器中显示警告区域
   - **Warning Color**: 警告颜色（默认红色半透明）

```csharp
// KillZone 示例
public class KillZone : MonoBehaviour
{
    public EKillZoneType zoneType = EKillZoneType.Lava;
    
    // 玩家进入区域时自动触发致命伤害
}
```

### 方法 3: BouncingSpike（可弹跳尖刺）

现有的弹跳尖刺，新增致命伤害选项。

**设置步骤：**
1. 现有的 `BouncingSpike` GameObject
2. 在 Inspector 中：
   - **Is Fatal**: 
     - ✅ 勾选 = 致命伤害（送回存档点）
     - ❌ 不勾选 = 普通伤害（击退 + 无敌）

### 方法 4: 自定义实现

在你自己的脚本中：

```csharp
public class MyDangerousObject : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                // 致命伤害
                player.OnTakeFatalDamage(transform.position);
                
                // 或者普通伤害
                // player.OnTakeDamage(transform.position);
            }
        }
    }
}
```

## 🎮 实际应用场景

### 场景 1: 跳跳乐路线

```
设置：
- 使用 FatalSpike 或 BouncingSpike (isFatal = true)
- 沿着跳跃路线放置尖刺
- 玩家失误掉到尖刺上 → 直接回到存档点
```

### 场景 2: 岩浆/深渊

```
设置：
- 使用 KillZone
- 设置 Zone Type = Lava 或 Abyss
- 在岩浆/深渊区域放置大面积 BoxCollider2D
- 玩家掉入 → 直接回到存档点
```

### 场景 3: 混合使用

```
同一关卡中：
- 路上的敌人：普通伤害（OnTakeDamage）
- 跳跃路线的尖刺：致命伤害（OnTakeFatalDamage）
- 底部深渊：KillZone 致命伤害
```

## 🎨 视觉反馈

### KillZone Gizmos

在 Scene 视图中，KillZone 会显示：
- **半透明红色区域** - 危险区域范围
- **红色边框** - 选中时显示清晰边界

可以关闭 Gizmos 显示：
- 取消勾选 `Show Warning Gizmos`

## 🔄 致命伤害流程

```
玩家接触致命物体
    ↓
调用 OnTakeFatalDamage()
    ↓
游戏暂停 0.1 秒（提供视觉反馈）
    ↓
短暂延迟（0.1秒）
    ↓
调用 RespawnAtNearestCheckpoint()
    ↓
玩家在最近的存档点重生
    ↓
所有状态重置（速度、攻击、冲刺等）
```

## ⚙️ 与普通伤害的区别

| 特性 | 普通伤害 | 致命伤害 |
|------|---------|---------|
| 调用方法 | OnTakeDamage() | OnTakeFatalDamage() |
| 击退效果 | ✅ 有 | ❌ 无 |
| 无敌时间 | ✅ 0.5秒 | ❌ 无 |
| 传送到存档点 | ❌ 否 | ✅ 是 |
| 重置玩家状态 | ❌ 否 | ✅ 是 |
| 用途 | 战斗、轻伤 | 跳跳乐、即死陷阱 |

## 📝 注意事项

1. **确保存档点系统已设置**
   - 场景中必须有 CheckpointManager
   - 至少有一个存档点已解锁

2. **性能考虑**
   - KillZone 使用 Trigger，性能开销小
   - 建议大面积区域使用一个大的 KillZone，而不是多个小的

3. **玩家标签**
   - 确保玩家 GameObject 的 Tag 设置为 "Player"

4. **碰撞层设置**
   - 确保致命物体和玩家在正确的 Layer 上
   - Physics2D 设置允许它们相互碰撞

## 🐛 故障排查

### 问题：致命伤害不起作用
- 检查玩家 Tag 是否为 "Player"
- 检查 CheckpointManager 是否在场景中
- 检查是否有已解锁的存档点
- 检查 Collider2D 是否正确设置

### 问题：KillZone 看不到 Gizmos
- 确保 `Show Warning Gizmos` 已勾选
- Scene 视图的 Gizmos 按钮已启用

### 问题：玩家没有回到存档点
- 检查 Console 是否有 "没有找到已解锁的检查点" 警告
- 确保至少有一个存档点设置为 `Is Unlocked By Default = true`

## 🎯 最佳实践

1. **跳跳乐关卡**
   - 在跳跃路线的起点放置存档点
   - 路线上的所有尖刺使用致命伤害
   - 避免玩家反复经历击退动画

2. **混合难度设计**
   - 简单区域：普通伤害
   - 高难度区域：致命伤害
   - 清晰区分，让玩家有预期

3. **视觉提示**
   - 致命区域使用明显的颜色（红色、黑色）
   - 在附近放置警告标志
   - 使用粒子特效增强危险感

4. **存档点布局**
   - 致命区域前必须有存档点
   - 存档点间距合理
   - 避免玩家重复大段路程

