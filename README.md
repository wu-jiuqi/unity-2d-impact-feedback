# Unity 2D 碰撞反馈

供 Codex 使用的 skill，包含闪白、拖尾、震屏、顿帧、慢镜、碎片、挤压和升调，以及使用 BounceBusters 素材的独立 Windows 调参展馆。

展馆提供单项、开关对照和组合模式；支持滑条／数值框、预设保存载入、自动和手动发射。顿帧／慢镜通过两个独立的真实2D物理场景演示。

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

首版构建0错误／0警告；30fps（1280×720）和60fps（1600×900）配置各73项运行检查通过、日志无错误。结果包含真实碰撞与时钟隔离、数值输入、预设往返、五色16帧及25fps动画；这些配置不是设备性能承诺。发布包附运行报告和哈希。预览图为GPU合成截图，另已检查真实窗口中的五色序列与滑条交互。
