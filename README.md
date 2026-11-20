# Unity 2D 碰撞反馈

供 Codex 使用的 skill，包含闪白、拖尾、震屏、顿帧、慢镜、碎片、挤压和升调，以及使用 BounceBusters 素材的独立 Windows 调参展馆。

展馆提供单项、开关对照和组合模式；支持滑条／数值框、预设保存载入、手动五球发射、3×5棋盘与消失即补齐。顿帧／慢镜通过两个独立的真实2D物理场景演示。

使用项目正式的粉、黄、薄荷绿、蓝、丁香紫五色方砖，可切换颜色；复用各色16帧／25fps破碎序列，可单独开关。

![五色砖块与实时顿帧调参](assets/gallery-preview.png)

- Skill 入口：[SKILL.md](SKILL.md)
- 参数：[八种效果](references/effects.md)
- 使用、构建与打包：[展馆说明](references/gallery.md)
- 源码：`assets/unity-sample`，Unity **6000.0.26f1 / Built-in / Windows x64**
- 展示包与带展示包的完整 skill ZIP：私有仓库的 **Releases**。

```powershell
./scripts/build.ps1
./scripts/verify.ps1 -Fps 60
./scripts/verify.ps1 -Fps 30
python ./scripts/package.py
python ./scripts/install.py --with-windows
```

素材来源、适配差异和授权说明见 [来源映射](references/project-source-map.md)。保持仓库私有；未授予项目美术公开分发权。


0.2.0：取消自动发射；玩家拖动瞄准、松手发射一组5球（间隔0.10秒）；15格五色方砖播完破碎序列立即补齐。震屏增加首下冲击，拖尾复用原线芯Shader，碎片恢复为液滴落成原版颜料色斑。验证结果与哈希随Release附带。

发射输入与排队直接复用原GameSession／ChainLiveSession／AppUI／RuleState；包括12°–168°方向约束、炮口偏移、整组预留、固定步出球、0.55秒连发及一次松手缓存。展馆仅做坐标与物理对象绑定。可执行 `python scripts/verify_project_launch.py` 核对来源。

0.2.1：升调改为do re mi fa so la xi 高音do，使用固定音高短促乐音。密集事件不跳音、断连回do、最高级封顶；可调音符间隔／音量／最高音阶，并显示当前音符。原项目发射代码保持不变。

0.2.1验证：30fps和60fps配置各131项通过，原20条RuleState规则自测通过；包括实际PCM音高、八级推进、密集碰撞不跳音、断连重置和旧预设兼容。音阶试听文件随发布包附带。
