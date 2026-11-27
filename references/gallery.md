# Windows 展馆

0.2.2 已同步游戏 0.5.1 花瓣。选择“碎片”，用参数区下方“破碎样式”切换花瓣／旧颜料；新建、项目参考和教学预设默认花瓣，旧 JSON 仍保持颜料。花瓣参数与集成见 [花瓣说明](petals.md)。

## 启动与操作

解压发布ZIP后运行 `Windows/ImpactFeedbackGallery.exe`。不需要Unity编辑器；EXE与`UnityPlayer.dll`、`MonoBleedingEdge`、`ImpactFeedbackGallery_Data`等同目录文件必须一同保留。

- 左侧选八项之一，右侧调参数；效果旁的开关控制是否启用。
- 顶部五色按钮切换正式五色方砖；“序列帧”开关启停原有16帧破碎动画。帧序、25fps速度、alpha清理和五色尺寸标定沿用项目；动画使用各侧的模拟时钟，受顿帧／慢镜／暂停影响。两侧都播放同一套基础动画。
- 单项体验只展示选中项；开关对照左侧关闭、右侧开启，同样球速和方向；组合体验遵循普通命中／破砖／特殊事件分级。
- 滑条即时改参数；数值框可精确输入，回车或失去焦点后提交。顿帧单位毫秒，慢镜是倍率。
- 玩家在展示区拖动瞄准、松手发射，或使用“发射5球”／空格；每组5球按0.10秒间隔发出，不自动重播。按原0.55秒冷却支持飞行中续发，并可缓存一次松手输入；P暂停／继续。
- 上方固定3行×5列五色砖，破碎序列结束同一tick原位补齐，不再额外停留空白；关闭序列帧后即时补齐。左侧基准静音，右侧可比较连续命中升调与固定音高。
- 项目参考预设恢复源码参考值；教学强化放大短促效果；恢复单项仅恢复当前效果参数及演示音量。
- 保存／载入预设读写用户数据目录 `BounceBusters/Impact Feedback Gallery/impact-profile.json`；保存成功后界面显示绝对路径。此文件可用于正式接入的 `ImpactProfile`。
- 失焦暂时冻结演示并清空瞬态效果；聚焦后继续。关闭窗口退出。

显示栏分别展示设定顿帧、预算后请求、实际暂停时长。实际值受当前帧率量化；申请0或预算耗尽时不发生顿帧。调整每秒预算才能观察更长顿帧。

## 本地构建

示例路径 `assets/unity-sample`，Unity 6000.0.26f1与Windows Build Support。从仓库根执行：

```powershell
./scripts/build.ps1 -Unity 'D:/Unity/Editor/Unity.exe'
```

输出 `Builds/Windows/ImpactFeedbackGallery.exe`，日志`evidence/build.log`。构建器创建空启动场景、配置窗口与Shader引用，独立于原游戏版本防护和存档。首次导入自动生成的Unity `.meta` 应一并提交，以保持资产身份稳定。

运行自动检查：

```powershell
./scripts/verify.ps1 -Fps 60
./scripts/verify.ps1 -Fps 30
```

生成 `evidence/qa60`／`qa30` 中的实际碰撞、双世界隔离、时钟恢复、粒子上限、预设往返、八项控件与截图结果。配置帧率不是实测性能承诺。视觉还应查看真实窗口和对应截图；自动检查不能代替主观手感验收。

## 打包与安装

```powershell
python ./scripts/package.py
python ./scripts/install.py --with-windows
```

打包前必须有成功构建与两个帧率的运行结果。输出 `dist/unity-2d-impact-feedback-0.2.2.zip`（含完整Windows展示）与`dist/ImpactFeedbackGallery-Windows-x64-0.2.2.zip`。文件哈希写入 `dist/delivery.json`，检查摘要附在压缩包中。大体积构建产物放 GitHub Release，不提交到源码 Git 历史。

安装脚本把仓库内skill和示例复制到当前用户 `.codex/skills/unity-2d-impact-feedback`，排除Library、Temp、日志、Git及发布ZIP。传入`--with-windows`时将已构建展示复制到该skill的`Windows/`目录。默认不覆盖已存在的skill目录；更新时显式传`--replace`，仅更新本skill管理的文件。

0.2.0强化了震屏首下并显示实际像素峰值；默认预设1.4%视口高度，项目参考预设仍保留原0.32%。拖尾使用项目原线芯Shader，碎片使用项目液滴和不规则色斑两张图集。

## 0.2.1 八级音阶

选择“升调”后，连续命中依次播放do re mi fa so la xi 高音do，达到最高级后保持；默认超过1.2秒没有音符重置到do。面板显示当前音符，支持最高音阶、音符最小间隔、断连重置、音阶音量和“重置到do”。重置音阶不操作发射队列。全部音符使用固定音高的乐音，不再对敲击声做任意小幅变调。单项关闭升调时保留固定do用于比较。发布包verification/solfeggio-scale.wav提供相同音色的完整音阶试听。
