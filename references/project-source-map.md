# BounceBusters 来源映射

来源工程：`BounceGarden/BounceGarden`，Unity 6000.0.26f1。整理日期2026-11-20。原工程目录只用于维护时追溯，安装后的示例不依赖原盘符。`DEVELOPMENT_BASELINE.json` 区分已确认0.1.80历史基线与当前0.4.0工作版本，不为提取反馈回退游戏版本。

| 内容 | 原工程中的来源 |
|---|---|
| 闪白 | `Assets/Scripts/World/PhysicsBodies.cs`、`Assets/Resources/HitFlash.shader` |
| 拖尾 | `Assets/Scripts/World/GameplayFlight.cs`、`Assets/Resources/YarnTrail79.shader`、`Art/YarnV19` |
| 震屏 | `Assets/Scripts/World/CameraJuice.cs` |
| 顿帧／慢镜／粒子池 | `Assets/Scripts/World/CombatFeedback79.cs` |
| 统一参数 | `Assets/Scripts/Core/FeedbackProfile.cs`、`Assets/Resources/FeedbackProfile.asset` |
| 挤压 | `Assets/Scripts/World/JellyMotion.cs`、`PhysicsBodies.cs` |
| 升调 | `Assets/Scripts/SoundService.cs`、`Assets/Scripts/Core/JuiceSession.cs` |
| C版界面 | `Assets/Scripts/UI/CSettingsArt.cs`、`UIBounce.cs`、`Resources/Art/SettingsC` |
| 正式五色砖块与16帧破碎 | `Assets/Scripts/World/SevenSpecialArt.cs` 的 OrdinaryFrames／FitOrdinary；`Resources/Art/FiveBakedV80/ordinary-{pink,yellow,mint,blue,lilac}.png` 与 footprints.json |

五色图集按4×4从左上到右下切16帧，256 pixels-per-unit；播放沿用 `JellyBrickArt` 的25fps，总长.64秒。使用 `footprints.json` 的有效宽度比例，使五色静止砖块的可见宽度一致；`BakedJelly.shader` 的alpha处理 `saturate((alpha-.4)/.6)` 已移入独立Shader的可选 `_AlphaCutoff=.4`，清除原图半透明底噪，不重画／改色源图。普通命中保留第一帧，破砖或特殊事件播放破碎并短暂隐藏后复位，以便循环展示。

## 版本差异

历史 `COMBAT_FEEDBACK_V79/80` 文档记录特殊事件顿帧45–60ms、120ms每秒预算。当前源码实际限制为25ms、500ms冷却，Profile预算30ms；本包采用当前源码作为参考，文档旧值仅用于追溯。原有旧QA仍有45ms断言，不能直接拿来验证新参考值。

震屏历史文档中的.65%／200ms已经与当前柔和曲线、.32%／320ms默认不同。项目参考预设保留当前参数。0.2.0展馆默认改为1.4%／240ms和首下完整冲击，再衰减回弹，避免较小视口里强度低于一个像素。

普通砖受击形变参数原为.045／120ms，方向性特殊目标 `JellyMotion` 曲线为.28／450ms。示例统一为可调.18／450ms，属于教学适配；不把它伪称为原游戏唯一参数。

原粒子为液滴飞行后落成颜料色斑，含十二束、不对称采样和.45–2.8砖宽半径。0.2.0已复用原jelly_particles与paint_splatters图集和飞行→落地色斑→停留淡出的表现；保留128槽、原方向采样、大小分组、1.2秒寿命和400ms淡出。只移除原棋盘非线性坐标校正，适配独立世界坐标。

原毛线专用Shader依赖纹理线芯提取；0.2.0复用双层LineRenderer、原毛线纹理与YarnTrail79纤维提取Shader，只替换原BoardClip为Unity标准投影；物理步与碰撞点采样，保留折角。闪白Shader已去除原棋盘非线性坐标变换，使用标准Unity世界坐标。升调从特定玩法中提取为通用连续命中逻辑。

## 素材与许可

运行素材和逐文件SHA-256在 `asset-manifest.json`，包括项目已有球、砖块、粒子、SettingsC图集、猫啃珠圆体及音效。字体保留附带OFL和中文授权文件，音频保留项目来源说明；DOTween保留作者readme和许可链接。项目原创／生成美术仅按本次私有项目复用授权提供，不授予公开素材分发权。仓库保持私有。

`scripts/import_project_assets.py` 是维护用的确定性复制脚本，已导入后无需再运行。UIBounce仅移除原AppUI和FeedbackProfile业务门禁，保留原按压、悬停、回弹曲线及网格变形。
