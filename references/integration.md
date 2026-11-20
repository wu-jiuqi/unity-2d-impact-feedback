# 接入与生命周期

## 可复用文件

仅拷贝 `Assets/ImpactFeedback/Runtime` 可获得参数、时钟、闪白 Shader 和表现组件。`ImpactPresenter` 不依赖 BounceBusters 业务类、UGUI 或 DOTween。宿主提供一个 SpriteRenderer 视觉子层、运动物体 Transform、相机、粒子精灵数组、音频和拖尾纹理。完整展示工程另有 UGUI 和既有按钮补间依赖。

```csharp
using ImpactFeedback;

var presenter = gameObject.AddComponent<ImpactPresenter>();
presenter.Profile = JsonUtility.FromJson<ImpactProfile>(profileJson);
presenter.Profile.Clamp();
presenter.Initialize(brickVisual, ballTransform, camera, particleSprites,
                     hitClip, yarnTexture);
// 在真实碰撞回调中，direction 为传入方向：
presenter.Hit(contactPoint, incomingDirection, brickTint, ImpactKind.Special);
```

`Initialize` 的视觉目标应是专门的子物体，且其所有者负责生命周期。当前示例一个 Presenter 绑定一个目标／运动头／相机；多目标正式场景可拆分逐目标闪白挤压与场景级镜头、时钟、池服务，不能为每块砖创建一个抢写同一镜头的实例。初始化一次，后续重复调用 `Hit`。

运行时必须确保两个 Shader 被构建包含：`ImpactFeedback/SpriteFlash` 和 `Sprites/Default`。示例构建器生成引用它们的 Resources 材质。手动迁移时保留这些引用或加入项目已有的 Shader 收集配置。当前闪白 Shader 使用 Built-in 顶点变换，URP 2D 项目应移植到项目的 Sprite Shader 并保留 `_Flash` 与 alpha 行为。

## 手动物理宿主

展馆的两个场景使用 `SceneManager.CreateScene(name, new CreateSceneParameters(LocalPhysicsMode.Physics2D))`，各自拥有 `PhysicsScene2D`。它们不由默认场景自动推进。每帧：

1. `presenter.Clock.Tick(Time.unscaledTime, Time.unscaledDeltaTime, paused)`。
2. 把 `Clock.Delta` 累加到宿主固定步预算；以1/120秒步长调用该场景的 `Simulate`。
3. 真实碰撞回调可能申请顿帧；检查 `Clock.Stopped` 并结束当前帧剩余物理步，避免本帧继续推进。
4. 调用 `presenter.Step(presentationDelta, realDelta)`，镜头恢复使用真实时间，粒子、挤压、拖尾使用模拟时间。

示例每帧最多16子步，单帧输入delta最多.1秒，长卡顿时主动丢弃部分时间，避免补帧失控。这是展示宿主的过载策略，正式游戏按自己的模拟规则选择。A/B两侧以相同初始位置和速度开始；顿帧后墙钟时刻的物理位置自然不同，不应再强制同步球位。

## 自动物理宿主

不要直接复制展馆 `GalleryLane` 的模拟循环到已经由 Unity 推进的物理世界，否则会重复模拟。把请求接到项目唯一的时间服务：

- 保存当前宿主倍率，由统一服务应用顿帧0／慢镜倍率，结束时恢复宿主倍率。
- 用非缩放时间维护结束、冷却和预算；宿主暂停高于反馈，取消反馈不等于解除游戏暂停。
- 重入时合并时长或取较强请求，规定总量上限。不要由两个组件分别保存／恢复全局 `Time.timeScale`。
- 显式调整 fixedDeltaTime 时也由同一服务保存与恢复；不能随意改动项目物理固定步设置。

## 清理

在切场景、对象回收、暂停、失焦、禁用时调用 `ResetPresentation()`。它清空拖尾、粒子、音频、时钟，并把相机和视觉scale还原。展馆失焦冻结两个世界；重新聚焦继续。宿主销毁组件前还需销毁或回收其拥有的对象池根；在长期场景中建议让 Presenter 与池根共同生灭。

改变分辨率或相机基础位置后更新相机静止基准，避免旧基准覆盖新布局。当前展示相机的基础位置固定；迁移到跟随相机时应改为独立的震屏子节点或给宿主镜头控制器提供偏移量。

## 参数保存

展馆读写 `Application.persistentDataPath/impact-profile.json`。通过 `.tmp` 写入后原子替换保存文件，错误显示在界面中。加载检查schemaVersion并对所有数值Clamp；当前schema=1。`ImpactProfile.Copy()` 可复制预设；参考预设每次创建新实例，自定义编辑不会覆盖源码中的默认值。

## 验证重点

物理顿帧需要观察真实刚体位置保持且基准侧继续；慢镜需要观察真实运动距离／时钟倍率。挤压检查碰撞体尺寸和根坐标不变。对多个连续事件检查声部、粒子、拖尾上限；重复暂停与恢复检查时钟和镜头。构建成功不等于实际运行检查通过。
