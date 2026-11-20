# Bounce Garden AI 生成音频资源包

生成日期：2026-10-30
生成工具：Doubao text-to-audio（AI 文生音频）
风格定位：柔和、轻弹性、果冻气泡、毛线与猫咪玩具质感；适配手机扬声器

---

## 一、文件总览

本目录共 **61 个 WAV 文件**，分四类：

| 类别 | 数量 | 说明 |
|---|---:|---|
| 核心短音 SFX | 11 | 发射、反弹、受击、消除、回收、点击、金币、胜败、弹窗、落地 |
| 容器连锁 SFX | 18 | 破甲、免伤、弱点、容器、大箱、收集、补砖、道具、激光、炸弹、连锁、四档评价 |
| 旧玩法 / 主线 SFX | 22 | 破冰、重击、各能力球、共鸣、钥匙、核心、升阶、拉链、猫爪、Boss、画作、采集 |
| UI / 猫叫 | 5 | 返回、无效操作、装备、开心猫叫、安慰猫叫 |
| BGM | 4 | 首页、局内、Boss、结算（均为可循环段落） |
| 环境声 | 1 | 花园底声 |

> **注意：** 这批是 AI 新生成素材，**不替换** `Assets/Resources/Audio/Jelly/` 下已有的 CC0 运行素材。建议先在 Unity 外试听，确认满意后再按 cue 名逐批替换或并存 A/B 对比。

---

## 二、文件清单与接入说明

### 2.1 核心短音（对应现有 11 个 cue）

| 文件名 | 用途 | 目标时长 | 建议音量 | 限频间隔 | 备注 |
|---|---|---:|---:|---:|---|
| `shoot.wav` | 毛线球发射（每球出手） | 0.10–0.20s | 0.85 | 25ms | 可做 2 个微变体 |
| `wall.wav` | 果冻墙反弹 | 0.06–0.12s | 0.85 | 45ms | 可做 2 个微变体；pitch 0.85–1.15 |
| `hit.wav` | 普通砖受击未碎 | 0.08–0.16s | 0.85 | 55ms | 可做 2 个微变体 |
| `break.wav` | 普通果冻消除 | 0.12–0.25s | **0.70** | 55ms | pitch 固定 1.0；核心音，密度高时保持舒适 |
| `splash.wav` | 弹球出界回收 | 0.15–0.35s | 0.85 | 80ms | 弱化存在感 |
| `click.wav` | 通用按钮点击 | 0.05–0.12s | 0.85 | 25ms | 可做 1 个变体 |
| `coin.wav` | 金币到账 | 0.15–0.30s | 0.85 | 25ms | 可做 1 个变体 |
| `win.wav` | 过关成功短提示 | 0.6–1.2s | 0.85 | — | 走胜败 AudioSource |
| `lose.wav` | 失败短提示 | 0.5–1.0s | 0.85 | — | 走胜败 AudioSource |
| `whoosh.wav` | 弹窗展开 | 0.12–0.25s | 0.85 | 25ms | 可做 1 个变体 |
| `jelly_land.wav` | 开场字母落地 | 0.20–0.40s | 0.85 | 25ms | 可做 1 个变体；连续 12 字母不重叠 |

### 2.2 容器连锁 v9 专用音

| 文件名 | 用途 | 目标时长 | 优先级 | 备注 |
|---|---|---:|---|---|
| `armor_break.wav` | 金色护甲破壳 | 0.18–0.35s | P0 | 轻壳啪裂 + 温和金属感，区别于本体破碎 |
| `blocked.wav` | 石砖/护板免伤 | 0.08–0.16s | P0 | 闷木石碰撞，不做错误报警 |
| `large_box_split.wav` | 2×2 大箱碎成四块 | 0.35–0.65s | P0 | 一次播放，不分裂四遍 |
| `collect_arrive.wav` | 任务物品收集到账 | 0.15–0.30s | P0 | 柔亮确认，区别于金币 |
| `cross_laser.wav` | 十字激光激活 | 0.25–0.50s | P0 | 一次激活一声，不四方向各播 |
| `bomb.wav` | 九宫格炸弹 | 0.25–0.50s | P0 | 低中频软爆裂 |
| `link_mark.wav` | 紫色连锁标记 | 0.3–0.6s | P0 | 柔韧魔法连线 |
| `link_collapse.wav` | 整组连锁崩解 | 0.3–0.6s | P0 | 一组短连弹，不长尾 |
| `rear_critical.wav` | 背部弱点击碎 | 0.18–0.35s | P1 | 脆爽但柔和 |
| `container_hit.wav` | 收藏容器受击 | 0.10–0.20s | P1 | 空心轻敲 |
| `container_open.wav` | 收藏容器打开 | 0.22–0.45s | P1 | 开盖 + 物品掉出 |
| `goal_complete.wav` | 单类任务收齐 | 0.4–0.8s | P1 | 弱于整关胜利 |
| `grid_settle.wav` | 下落补砖落位 | 0.12–0.25s | P1 | 按批合并播放，不每格播 |
| `tool_spawn.wav` | 评价奖励道具出现 | 0.25–0.45s | P1 | 所有工具共用 |
| `rating_nice.wav` | NICE（4 次命中） | 0.18–0.30s | P1 | C 大调，四档逐级上行 |
| `rating_great.wav` | GREAT（8 次） | 0.22–0.35s | P1 | 同音色两音上行 |
| `rating_amazing.wav` | AMAZING（12 次） | 0.3–0.45s | P1 | 三音上行 |
| `rating_incredible.wav` | INCREDIBLE（18 次） | 0.4–0.65s | P1 | 四音上行 + 和弦闪光 |

### 2.3 旧玩法 / 主线音

| 文件名 | 用途 | 目标时长 | 优先级 |
|---|---|---:|---|
| `ice_break.wav` | 冰壳解冻 | 0.20–0.40s | P2 |
| `heavy_charge.wav` | 重击球蓄力 | 0.25–0.45s | P2 |
| `heavy_impact.wav` | 重击命中 | 0.18–0.35s | P2 |
| `needle_activate.wav` | 穿针能力发动 | 0.20–0.40s | P2 |
| `bloom_activate.wav` | 绒花范围绽放 | 0.25–0.50s | P2 |
| `split_activate.wav` | 薄荷分线 | 0.18–0.35s | P2 |
| `rainbow_sweep.wav` | 定向彩虹扫线 | 0.25–0.45s | P2 |
| `resonance.wav` | 共鸣传递 | 0.20–0.40s | P2 |
| `key_unlock.wav` | 钥匙封条释放 | 0.25–0.50s | P2 |
| `core_charge.wav` | 蓄能核心进度 | 0.12–0.25s | P2 |
| `core_burst.wav` | 蓄能核心爆发 | 0.30–0.60s | P2 |
| `heat_upgrade.wav` | 毛线球升温升阶 | 0.20–0.40s | P2 |
| `bank_charge.wav` | 借墙蓄能 | 0.15–0.30s | P2 |
| `zip_step.wav` | 拉链传播单步 | 0.06–0.12s | P2 |
| `cream_critical.wav` | 奶油夹心背击 | 0.18–0.35s | P2 |
| `paw_bounce.wav` | 幸运猫爪回弹 | 0.20–0.40s | P2 |
| `boss_hit.wav` | 布丁王受击 | 0.10–0.20s | P2 |
| `boss_action.wav` | 布丁王换位 | 0.40–0.70s | P2 |
| `boss_defeat.wav` | 布丁王落败 | 0.60–1.0s | P2 |
| `painting_collect.wav` | 画作完成收藏 | 0.70–1.2s | P2 |
| `paw_pick.wav` | 采集猫爪取物 | 0.20–0.40s | P2 |
| `clear_yarn.wav` | 采集毛线清列 | 0.25–0.50s | P2 |

### 2.4 可选 UI / 猫叫

| 文件名 | 用途 | 目标时长 | 优先级 |
|---|---|---:|---|
| `ui_back.wav` | 返回/关闭弹窗 | 0.08–0.16s | P3 |
| `ui_unavailable.wav` | 无效操作提示 | 0.12–0.25s | P3 |
| `cosmetic_equip.wav` | 外观装备成功 | 0.25–0.45s | P3 |
| `cat_happy.wav` | 猫咪开心短叫 | 0.35–0.70s | P3 |
| `cat_retry.wav` | 猫咪安慰短叫 | 0.35–0.70s | P3 |

### 2.5 BGM 与环境声

| 文件名 | 用途 | 时长 | BPM | 备注 |
|---|---|---:|---:|---|
| `bgm_home.wav` | 首页/选关/图鉴/排行榜 | ~84s | 92 | 32 小节循环，C 大调，木琴+拨弦+毡钢琴 |
| `bgm_gameplay.wav` | 通用局内 | ~74s | 104 | 32 小节循环，低旋律密度让出中高频给碰撞音 |
| `bgm_boss.wav` | 布丁王 Boss | ~69s | 112 | 32 小节循环，同主题变奏，俏皮轻紧张 |
| `bgm_result.wav` | 结算停留 | ~21s | 92 | 8 小节循环，柔和主题变奏 |
| `amb_garden.wav` | 花园环境底声 | ~45s | — | 极轻叶声空气，立体声循环 |

---

## 三、Unity 接入指南

### 3.1 导入路径建议

```
Assets/Resources/Audio/Jelly/AI/
├── sfx/          ← 所有短音效（mono 导入）
├── bgm/          ← 4 首 BGM（stereo 导入）
└── amb/          ← 环境声（stereo 导入）
```

或直接平铺在 `Jelly/` 下，按 cue 名加载。**不要**直接覆盖现有 CC0 文件，建议先放进 `AI/` 子目录并行试听。

### 3.2 导入设置

- **SFX 短音**：Force To Mono ✓，加载类型 Decompress On Load，压缩格式建议 PCM 或 ADPCM（短音用 ADPCM 省包体）
- **BGM**：Streaming，Vorbis 压缩，质量 0.7–0.85，勾选 Loop
- **环境声**：Streaming，Vorbis，勾选 Loop
- 采样率：母带约 44.1kHz，无需转换；与现有 44.1kHz 素材一致

### 3.3 SoundService 扩展要点

现有 SoundService 只有 8 个 AudioSource（6 常规 SFX + 1 胜败 + 1 金币）和 11 个预载 cue。接入本批需要：

1. **扩充 cue 表**：在 `SoundService.cs` 的枚举/字典中加入上表所有新 cue 名
2. **扩充 AudioSource 池**：新道具音（激光、炸弹、连锁等）可能与高频碰撞音叠加，建议常规 SFX 声道从 6 扩到 10–12
3. **BGM 独立通道**：新建一个 BGM AudioSource，音量独立控制，不挤占 SFX 声部
4. **限频参数沿用现有规则**：
   - `break`：间隔 55ms，pitch=1.0，音量 0.70
   - `wall`：间隔 45ms
   - `hit`：间隔 55ms
   - `splash`：间隔 80ms
   - 其余：间隔 25ms，音量 0.85，pitch 范围 0.85–1.15
5. **同屏并发上限**：同类常规音一般 ≤3；激光/炸弹/连锁各只播一次，不随目标数量重复触发

### 3.4 需要后期处理的部分

AI 生成的短音母带约 1–2 秒，**尾部带静音**，接入前建议：

1. **裁切静音**：用 Audacity / ffmpeg 裁到上表目标时长，保留约 20–50ms 干净尾音
2. **峰值归一化**：母带峰值约留 3dB 余量；短音主要调感知响度，不用 LUFS 硬拉
3. **BGM 循环接缝**：在 Unity 里 Loop 模式下试听首尾接缝，如接缝明显可在音频编辑器做 crossfade 或裁掉首尾不完整小节
4. **变体生成**：`shoot`/`wall`/`hit` 等高频音可在 Unity 内用 pitch 微随机（±0.05）制造变体，不必单独出文件

---

## 四、复用建议

| 场景 | 可复用文件 | 说明 |
|---|---|---|
| 石砖免伤、护板、正面异色反弹 | `blocked.wav` | 替代原 wall 的免伤场景 |
| 破甲、破冰、Boss 受击原复用 hit | `armor_break` / `ice_break` / `boss_hit` | 各有专用音后不再复用 hit |
| 旧拉链传播原复用 jelly_land | `zip_step.wav` | 每 90ms 播一次，极短 tick |
| 蓄能/猫爪回弹原复用 whoosh | `bank_charge` / `paw_bounce` | 各有专用音 |
| 结算界面 BGM | `bgm_result.wav` 或复用 `bgm_home` | 文档建议先复用首页 BGM |
| 道具激活统一音 | `tool_spawn.wav` | 所有评价奖励道具共用 |
| 穿针球 vs 十字激光 | `needle_activate` ≠ `cross_laser` | 不要混用 |

---

## 五、与现有 CC0 素材的关系

- 现有 11 个 CC0 文件（`Assets/Resources/Audio/Jelly/`）**保持不变**，本批为新增 AI 生成备选
- 现有 `break.ogg` 保留不动；本批 `break.wav` 为重生备选，替换时注意 OGG→WAV 的导入记录和 .meta GUID
- 本批文件版权归 AI 生成，**不继承**原 CC0 授权；商用前需确认生成工具的使用许可条款

---

## 六、快速试听顺序建议

按游戏实际播放密度试听：

1. **高频连发组**：`shoot` → `wall` → `hit` → `break` 循环 5 球连发
2. **道具连锁组**：`collect_arrive` → `cross_laser` → `bomb` → `link_mark` → `link_collapse`
3. **评价递进组**：`rating_nice` → `rating_great` → `rating_amazing` → `rating_incredible`
4. **BGM 叠加 SFX**：开 `bgm_gameplay` 后叠加 `shoot`/`break`，检查 BGM 是否抢戏
5. **胜败收尾**：`win` 或 `lose` → `bgm_result`
