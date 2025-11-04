# 屏幕渐变效果使用指南

## 📋 系统概述

ScreenFade 系统提供屏幕渐变效果，用于死亡、传送等场景转换。当玩家受到致命伤害时，屏幕会先变黑再恢复，同时传送到存档点。

## 🚀 设置步骤

### 步骤 1: 创建 ScreenFade 对象

1. 在 Hierarchy 中右键 → **UI → Canvas**（如果已有 Canvas 可以跳过）
2. 在 Canvas 下创建空物体：
   - 右键 Canvas → Create Empty
   - 命名为 **ScreenFade**

3. 选中 ScreenFade 对象，添加组件：
   - **Add Component** → 搜索 **ScreenFade**
   - 添加 `ScreenFade` 脚本

### 步骤 2: 自动配置（推荐）

ScreenFade 脚本会自动创建和配置所需组件：
- ✅ 自动添加 Canvas（如果没有）
- ✅ 自动创建全屏黑色 Image
- ✅ 自动设置为最上层显示

**无需手动配置！** 运行游戏时会自动初始化。

### 步骤 3: 手动配置（可选）

如果你想自定义渐变图像：

1. 在 ScreenFade 对象下创建 Image：
   - 右键 ScreenFade → **UI → Image**
   - 命名为 **FadeImage**

2. 配置 Image：
   ```
   FadeImage (RectTransform)
   ├─ Anchor Presets: 拉伸全屏
   │  └─ 点击 Anchor Presets 按钮，按住 Alt+Shift，点击右下角的拉伸选项
   ├─ Left: 0
   ├─ Right: 0
   ├─ Top: 0
   ├─ Bottom: 0
   │
   └─ Image 组件
      ├─ Color: Black (0, 0, 0, 0)  ← 初始透明
      └─ Raycast Target: ❌ false
   ```

3. 将 FadeImage 拖到 ScreenFade 脚本的 **Fade Image** 字段

### 步骤 4: 配置 ScreenFade 参数

在 Inspector 中：

```
ScreenFade 组件：

【渐变设置】
├─ Fade Image: （自动创建或手动拖入）
└─ Fade Color: Black (0, 0, 0, 255)
   （可以改成其他颜色，如白色）
```

## 🎯 完整场景结构

```
Canvas
├─ ScreenFade (空对象)
│  ├─ ScreenFade 脚本
│  └─ FadeImage (Image，自动创建)
│     └─ 全屏黑色图像
│
└─ 其他 UI 元素...
```

## ⚙️ 系统特性

### 1. 致命伤害流程

当玩家受到致命伤害时：

```
时间轴：
0.0s  - 游戏暂停 0.1s
0.1s  - 恢复游戏
0.3s  - 开始屏幕渐变（0.4秒总时长）
0.5s  - 屏幕完全变黑，传送到存档点
0.7s  - 屏幕恢复透明，玩家在存档点重生
```

### 2. 自动功能

- ✅ 单例模式，全局唯一
- ✅ DontDestroyOnLoad（切换场景不销毁）
- ✅ 渲染顺序最高（9999）
- ✅ 不受 Time.timeScale 影响

### 3. 可自定义

```csharp
// 改变渐变颜色为白色
ScreenFade.Instance.fadeColor = Color.white;

// 自定义渐变时长
await ScreenFade.Instance.FadeOutAndIn(1.0f); // 1秒渐变
```

## 🎨 高级用法

### 手动触发渐变

```csharp
// 在任何脚本中调用
if (ScreenFade.Instance != null)
{
    // 渐变效果（先黑后透明）
    await ScreenFade.Instance.FadeOutAndIn(0.5f);
    
    // 在黑屏期间执行操作
    // 例如：切换场景、传送等
}
```

### 立即设置屏幕状态

```csharp
// 立即变黑
ScreenFade.Instance.SetBlack();

// 立即透明
ScreenFade.Instance.SetClear();
```

### 用于场景切换

```csharp
public async void LoadNextLevel()
{
    // 渐变到黑色
    await ScreenFade.Instance.FadeOutAndIn(1.0f);
    
    // 在黑屏时加载场景（在 0.5s 时执行）
    SceneManager.LoadScene("NextLevel");
}
```

## 🐛 故障排查

### 问题 1: 没有渐变效果
**检查：**
- ScreenFade GameObject 是否在场景中？
- ScreenFade 脚本是否已添加？
- Console 是否有错误信息？

### 问题 2: 黑屏一直不消失
**原因：** FadeImage 的初始 Alpha 不是 0
**解决：** 
- 选中 FadeImage
- 设置 Color 的 Alpha (A) 为 0
- 或删除手动创建的 FadeImage，让脚本自动创建

### 问题 3: 渐变被其他UI遮挡
**原因：** Canvas 渲染顺序不对
**解决：**
- 确保 ScreenFade 的 Canvas.sortingOrder = 9999
- 或将 ScreenFade 移到最上层的 Canvas

### 问题 4: 渐变时游戏卡顿
**原因：** Time.timeScale 影响
**解决：** 
- ScreenFade 已使用 `Time.unscaledDeltaTime`
- 不受 `Time.timeScale` 影响
- 检查其他可能的性能问题

## 📝 注意事项

1. **只需要一个 ScreenFade**
   - 单例模式自动处理
   - 重复创建会自动销毁多余的

2. **Canvas 设置**
   - 使用 Screen Space - Overlay 模式
   - 渲染顺序设为最高（9999）

3. **DontDestroyOnLoad**
   - ScreenFade 会在场景切换时保留
   - 确保不会重复创建

4. **性能考虑**
   - 渐变使用协程，性能开销很小
   - 全屏 Image 只在需要时显示（透明度 > 0）

## 🎯 效果预览

### 致命伤害效果

```
玩家碰到 FatalSpike
    ↓
暂停 0.1s
    ↓
等待 0.2s
    ↓
屏幕开始变黑（0.2s）
    ↓
完全黑屏时传送
    ↓
屏幕恢复透明（0.2s）
    ↓
玩家在存档点重生
```

总时长：0.1s + 0.2s + 0.4s = **0.7秒**

## 🔧 自定义时间参数

如果你想调整时间，修改 PlayerController 中的 `TakeFatalDamage` 方法：

```csharp
// 修改等待时间
await UniTask.Delay(200);  // 改成其他值（毫秒）

// 修改渐变时长
ScreenFade.Instance.FadeOutAndIn(0.4f).Forget();  // 改成其他值（秒）
```

现在你的游戏有了更加专业的死亡效果！🎬

