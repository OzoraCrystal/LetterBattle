# LetterBattle
这是一个拼写字母游戏，实现了类似格斗游戏技能输入检测。

# LetterBattle [演示视频](https://www.bilibili.com/video/BV1DWNne1EjU)

## 🎮 核心玩法
- **技能拼写系统**：通过 WSAD 键组合检测玩家输入，实现法术技能的释放与反馈。
- **敌我状态博弈**：利用 AI 驱动的敌人平面状态机，实现敌人的随机决策与行为切换，同时管理战斗双方的生命值和状态反馈。

## ⚙️ 技术架构
### 战斗系统
| 模块          | 实现方案                       | 性能指标或特点           |
|---------------|-------------------------------|-------------------------|
| 状态机         | 平面状态机（敌人与战斗状态管理）| 状态切换响应迅速          |
| 技能系统       | ScriptableObject配置          | 支持灵活配置多种技能参数  |
| UI系统        | UGUI动静分离                   | 流畅的实时 UI 更新与反馈  |

## ⚙️ 文件结构
```/Assets
  /Scripts
    ├── EnemyCombatController.cs      // 控制敌人的战斗流程与状态切换
    ├── EnemyStateMachine.cs          // 敌人状态机，管理状态转换（Idle、Selecting、Spelling、Attacking）
    ├── BattleManager.cs              // 战斗管理器，负责玩家输入、技能拼写、超时处理及反馈展示
    ├── HealthManager.cs              // 血量管理，负责扣血、回血及死亡事件触发
    ├── PlayerUIManager.cs            // 玩家 UI 管理，负责血条更新与数值显示
    ├── EnemyUIManager.cs             // 敌人 UI 管理，负责敌人血条更新与战斗提示
    ├── UIManager.cs                  // 整体 UI 管理，负责技能格的生成和高亮显示
    ├── SkillCell.cs                  // 技能格控制，显示技能名称及拼写序列，动态高亮匹配字符
    ├── SkillData.cs                  // 技能数据定义，包括 Skill 类和 SkillType 枚举
  /Prefabs
    └── Skill.prefab                  // 技能格预制体（供 UIManager 动态生成技能格使用）
  /Resources
    └── SkillData
         └── LHSkillData.asset        // 技能数据资源文件（存储所有技能的列表）
```
## 第三方资源

本项目中使用了以下第三方免费资源：

- **Pixel Skies DEMO Background pack**  
  免费资源，来自 Unity Asset Store。请参阅 [官方页面](https://assetstore.unity.com/packages/2d/environments/pixel-skies-demo-background-pack-226622#reviews)了解详细许可信息。

- **Hero Knight - Pixel Art**  
  免费资源，来自 Unity Asset Store。请参阅 [官方页面](https://assetstore.unity.com/packages/2d/characters/hero-knight-pixel-art-165188#reviews)了解详细许可信息。

以上资源均根据各自的许可协议使用，仅用于本项目开发与演示。项目代码仅公开核心部分，第三方资源文件不包含在内。

