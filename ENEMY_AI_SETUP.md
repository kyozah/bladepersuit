# Enemy AI Setup - Step by Step Guide

## Tổng Quan
Hướng dẫn thiết lập hệ thống AI cho enemy trong Unity, từ tạo Behavior Tree đến test hoàn chỉnh.

---

## Bước 1: Tạo Behavior Tree cho Enemy

### 1.1 Tạo Thư Mục và Scripts Cơ Bản
1. Tạo thư mục: `Assets/Scripts/AI/BehaviorTree/`
2. Tạo các script cơ bản:
   - `BTNode.cs` (abstract class)
   - `CompositeNode.cs` (Selector, Sequence)
   - `DecoratorNode.cs`
   - `ConditionNode.cs`
   - `ActionNode.cs`

### 1.2 Tạo Behavior Tree Asset

1. Right-click trong Project window > Create > AI > Behavior Tree
2. Đặt tên: `EnemyBehaviorTree_Default`
3. Tạo root node: Chọn "Selector" từ menu dropdown
4. Thêm các child nodes theo cấu trúc:

   ```
   Selector (Root)
   ├── Sequence (Active Attacker)
   │   ├── IsActiveAttacker (Condition)
   │   └── Selector (Attack/Chase)
   │       ├── Sequence (Attack)
   │       │   ├── PlayerInAttackRange (Condition)
   │       │   └── AttackPlayer (Action)
   │       └── Sequence (Chase)
   │           ├── PlayerInDetectionRange (Condition)
   │           └── MoveToPlayer (Action)
   ├── Sequence (Retreat)
   │   ├── Not IsActiveAttacker (Condition)
   │   └── RetreatFromPlayer (Action)
   └── Idle (Action)
   ```

**Cách tạo từng node:**
- Root: Chọn "Selector"
- Active Attacker: Chọn "Sequence"
- IsActiveAttacker: Chọn "Condition" > "IsActiveAttacker"
- Attack/Chase Selector: Chọn "Selector"
- Attack Sequence: Chọn "Sequence"
- PlayerInAttackRange: Chọn "Condition" > "PlayerInAttackRange"
- AttackPlayer: Chọn "Action" > "AttackPlayer"
- Chase Sequence: Chọn "Sequence"
- PlayerInDetectionRange: Chọn "Condition" > "PlayerInDetectionRange"
- MoveToPlayer: Chọn "Action" > "MoveToPlayer"
- Retreat Sequence: Chọn "Sequence"
- Not IsActiveAttacker: Chọn "Condition" > "Not IsActiveAttacker"
- RetreatFromPlayer: Chọn "Action" > "RetreatFromPlayer"
- Idle: Chọn "Action" > "Idle"

### 1.3 Cấu Hình Logic Flow
- **Active Attacker**: Chase player nếu ngoài attack range, attack nếu trong range
- **Waiting Attacker**: Di chuyển lùi chậm
- **Idle**: Khi không tìm thấy player

---

## Bước 2: Setup Enemy Prefab với Behavior Tree

### 2.1 Chuẩn Bị Components Cơ Bản
1. Tạo Enemy prefab với:
   - Rigidbody (Is Kinematic = FALSE)
   - Animator (với animations)
   - BoxCollider (tag = "Enemy")
   - Script: Enemy.cs
   - Script: EnemyBehaviorTree.cs

### 2.2 Cấu Hình Rigidbody
```
Mass: 1
Drag: 0.1
Angular Drag: 0.05
Use Gravity: TRUE
Constraints: Freeze Rotation X, Y, Z ✓
```

### 2.3 Cấu Hình Enemy.cs
```
Stats:
  Max Health: 100

Knockback Settings:
  Knockback Force: 10
  Knockback Upward Force: 2
  Knockback Duration: 0.3
  Knockback Drag: 8
  Use Player Forward Direction: TRUE ✓

Debug:
  Show Debug Info: FALSE
  Debug AI: FALSE
```

### 2.4 Cấu Hình EnemyBehaviorTree.cs
```
Behavior Tree: [Gán EnemyBehaviorTree_Default asset]
Behavior:
  Detection Range: 15
  Attack Range: 2
  Move Speed: 3
  Retreat Speed: 0.5
  Attack Cooldown: 2
```

### 2.5 Setup Animator Parameters
```
Bool:
  - Run
  - BackWalk
  - Idle

Trigger:
  - Attack
  - Hit
  - Death
```

---

## Bước 3: Tạo Enemy Spawner

### 3.1 Tạo Script EnemySpawner.cs
- Đặt trong `Assets/Character/Scripts/`

### 3.2 Cấu Hình EnemySpawner
```
Spawn Settings:
  Enemy Prefab: [Drag Enemy prefab đã setup]
  Enemies To Spawn: 5
  Spawn Interval: 1.5
  Activation Distance: 20

Spawn Points: [List các Transform spawn]
  Size: 3
  Element 0-2: [Empty GameObjects xung quanh]

Debug:
  Show Debug Info: FALSE
```

### 3.3 Cách Hoạt Động
- Khi player trong 20m, bắt đầu spawn
- Spawn lần lượt mỗi 1.5s
- Mỗi enemy tự register với AttackManager

---

## Bước 4: Setup Spawn Block trên Map

### 4.1 Tạo GameObject Spawn Block
1. Tạo empty GameObject: `Enemy_Spawn_Block_1`
2. Attach script: `EnemySpawner.cs`
3. Cấu hình như Bước 3

### 4.2 Tạo Spawn Points
1. Tạo 3 empty GameObjects con của Block
2. Đặt vị trí xung quanh block
3. Gán vào list "Spawn Points" trong EnemySpawner

### 4.3 Vị Trí Block
- Đặt trên map tại các zone muốn spawn enemy
- Khoảng cách activation: 20m từ player

---

## Bước 5: Tạo Enemy Attack Manager Singleton

### 5.1 Tạo GameObject
1. Tạo empty GameObject: `EnemyAttackManager`
2. Attach script: `EnemyAttackManager.cs`

### 5.2 Cấu Hình
```
Max Active Attackers: 2
```

### 5.3 Quan Trọng
- Chỉ 1 instance per scene
- Quản lý tối đa 2-3 quái tấn công cùng lúc
- Auto-promote waiting enemies khi active chết

---

## Bước 6: Cấu Hình Animator và Animations

### 6.1 Animator Parameters
```
Bool:
  - Run
  - BackWalk
  - Idle

Trigger:
  - Attack
  - Hit
  - Death
```

### 6.2 Animation States
```
Idle (default)
  ↓ [Run = true]
Run (loop)
  ↓ [Attack trigger]
Attack (0.5s)
  ↓ [timeout]
BackWalk (loop)
  ↓ [BackWalk = false]
Idle
```

### 6.3 Animation Clips
- **Run**: Loop, 0.5s per cycle
- **BackWalk**: Loop, 0.4s per cycle
- **Attack**: 0.5s, play once
- **Hit**: 0.3s, play once
- **Death**: 1.0s, play once

---

## Bước 7: Test và Debug Enemy

### 7.1 Test Cơ Bản
1. Chạy scene với player
2. Tiếp cận spawn block (20m)
3. Quan sát enemy spawn và behavior

### 7.2 Debug Tools
- Enable Gizmos: Select enemy để thấy detection/attack range
- Enable Debug Logs: Trong Inspector các script
- Check Console cho errors

### 7.3 Common Issues
- Enemy không di chuyển: Check Rigidbody settings
- Không spawn: Check activation distance
- Behavior lạ: Check Animator parameters

### 7.4 Checklist Hoàn Thành
- [ ] Enemy Prefab: Rigidbody + Animator + Collider
- [ ] Enemy tag set
- [ ] Player tag set
- [ ] EnemyAttackManager scene instance
- [ ] EnemySpawner on spawn block
- [ ] Spawn points (hoặc để trống)
- [ ] Test spawn: Player gần zone
- [ ] Test attack: Max 2-3 quái tấn công
- [ ] Test AI: Quái khác di chuyển lùi

---

## Behavior Tree Parameters Guide

### Detection Range (15m)
- Quái phát hiện player từ khoảng cách này
- Ngoài range → idle

### Attack Range (2m)
- Khoảng cách tấn công tối thiểu

### Move Speed (3 m/s)
- Tốc độ chase

### Retreat Speed (0.5 m/s)
- Tốc độ lùi (PHẢI CHẬM)

### Attack Cooldown (2s)
- Thời gian chờ giữa attacks

---

## Gameplay Flow

### Khi Player Tiếp Cận:
1. Player trong 20m → Spawner kích hoạt
2. Spawn enemy lần lượt (1.5s)
3. Enemy register với AttackManager
4. Max 2 active attackers, còn lại waiting

### Khi Enemy Nhận Damage:
1. TakeDamage() → health -= damage
2. Play "Hit" animation
3. Apply knockback
4. Nếu health <= 0 → Die() → Unregister → Promote waiting

---

## Troubleshooting

**Quái không di chuyển:**
- Check Rigidbody: Is Kinematic = FALSE
- Check Animator: Parameters đúng
- Check Behavior Tree: Nodes có Tick()?

**Không spawn enemy:**
- Check activation distance
- Check Enemy prefab assigned
- Check Console errors

**Behavior Tree không hoạt động:**
- Check BehaviorTree asset assigned
- Check nodes created đúng
- Check Initialize() called

**Performance issues:**
- Behavior Tree lightweight per frame
- Max 10+ enemies OK
- Monitor FPS khi test

---

**Last Updated**: January 15, 2026
3. **Add Component** → Search "EnemyBehaviorTree"
4. **Attach Script** `EnemyBehaviorTree.cs`

**Trong Enemy Prefab Hierarchy:**
```
Enemy (Root)
├── Model
├── Collider
├── [Scripts]
│   ├── Enemy.cs
│   └── EnemyBehaviorTree.cs ← Add here
```

**EnemyBehaviorTree.cs Setup trong Inspector:**

| Setting | Value | Mô Tả |
|---------|-------|-------|
| Detection Range | 15 | Quái phát hiện player từ 15m |
| Attack Range | 2 | Quái tấn công khi gần 2m |
| Move Speed | 3 | Tốc độ chase player (m/s) |
| Retreat Speed | 0.5 | Tốc độ di chuyển lùi CHẬM (m/s) |
| Attack Cooldown | 2 | Cooldown giữa các lần tấn công |

**Behavior Tree Logic Flow:**

```
Update() mỗi frame:
│
├─ Tìm Player
│  └─ Không tìm thấy? → SetAnimatorState("Idle") → RETURN
│
├─ Calc Distance to Player
│
├─ Check: IsActiveAttacker()?
│  │
│  ├─ YES (Active Attacker):
│  │  └─ if distanceToPlayer > detectionRange:
│  │      → SetAnimatorState("Idle")
│  │    else if distanceToPlayer > attackRange:
│  │      → MoveTowardPlayer() → "Run"
│  │    else:
│  │      → HandleAttack() → "Attack"
│  │
│  └─ NO (Waiting Attacker):
│     └─ HandleRetreating() → Di chuyển lùi chậm → "BackWalk"
```

**Active Attacker - Chase & Attack:**
- Xoay về hướng player (5°/s)
- Di chuyển với `moveSpeed` (3 m/s)
- Khi gần hơn `attackRange` (2m) → tấn công
- Attack cooldown: 2 giây
- Damage: 15 HP
- Damage delay: 0.5s sau khi trigger attack

**Waiting Attacker - Retreat Slowly:**
- Di chuyển LÙI (away from player)
- Tốc độ RẤT CHẬM: `retreatSpeed` (0.5 m/s)
- Xoay nhẹ theo player (3°/s)
- Khi quái active attacker chết → Promote to active

**Animator Parameters Required:**

```csharp
// Bool parameters
animator.SetBool("Run", true/false);
animator.SetBool("BackWalk", true/false);
animator.SetBool("Idle", true/false);

// Trigger parameters
animator.SetTrigger("Attack");
animator.SetTrigger("Hit");
animator.SetTrigger("Death");
```

**Gizmos Visualization:**

Khi select Enemy trong Scene:
- 🟡 **Yellow sphere** (15m): Detection range
- 🔴 **Red sphere** (2m): Attack range
- Giúp visualize AI behavior

---

### Bước 1.6: Tạo và Sử dụng Behavior Tree (chi tiết)

**Mục tiêu:** Hướng dẫn từng bước để tạo một Behavior Tree đơn giản, tích hợp vào `Enemy` prefab và cách debug khi chạy.

- Tạo thư mục: `Assets/Scripts/AI/BehaviorTree`
- Tạo các script cơ bản:
  - `BTNode` (abstract): `Tick()` trả về `NodeState` {Success, Failure, Running}
  - `CompositeNode` : `Selector`, `Sequence` (chứa danh sách children)
  - `DecoratorNode` : 1 child, sửa kết quả child
  - `ConditionNode` : kiểm tra trạng thái (ví dụ: `PlayerInRange`)
  - `ActionNode` : hành động (ví dụ: `MoveTo`, `Attack`, `Retreat`)

**Cách tạo cây (workflow):**
1. Viết các lớp nodes ở trên và serialize các node (ScriptableObject hoặc serializable MonoBehaviour) để có thể cấu hình trong Inspector.
2. Tạo `BehaviorTree` MonoBehaviour hoặc ScriptableObject có tham chiếu tới `root` (một node composite).
3. Thêm component `BehaviorTreeRunner` (hoặc dùng `EnemyBehaviorTree`) trên `Enemy` prefab và gán `BehaviorTree`/`root` vào đó.
4. Trong `EnemyBehaviorTree.Update()` gọi `root.Tick()` mỗi frame (hoặc theo interval) để đánh giá cây.

**Implementation đã hoàn thành:**
- Các script BTNode, CompositeNode, DecoratorNode, ConditionNode, ActionNode đã được tạo trong `Assets/Character/Scripts/AI/BehaviorTree/`
- EnemyBehaviorTree đã được cập nhật để sử dụng BehaviorTree.
- Nếu không gán BehaviorTree trong Inspector, hệ thống sẽ tự động tạo cây mặc định với logic: Active attacker (Chase/Attack) hoặc Retreat.
- Các node có thể được tạo như ScriptableObject assets qua menu Assets > Create > AI/Behavior Tree.

**Cách tạo Behavior Tree Asset:**
1. Right-click trong Project window > Create > AI > Behavior Tree
2. Gán root node (ví dụ Selector)
3. Tạo các child nodes qua menu tương ứng và gán vào children list.
4. Gán BehaviorTree asset vào EnemyBehaviorTree component trên prefab.

**Cấu trúc cây mặc định:**
```
Selector (Root)
├── Sequence (Active Attacker)
│   ├── IsActiveAttacker
│   └── Selector
│       ├── Sequence (Attack)
│       │   ├── PlayerInAttackRange
│       │   └── AttackPlayer
│       └── Sequence (Chase)
│           ├── PlayerInDetectionRange
│           └── MoveToPlayer
├── Sequence (Retreat)
│   ├── Not IsActiveAttacker
│   └── RetreatFromPlayer
└── Idle
```

**Ví dụ ngắn (ý tưởng code):**
```csharp
public abstract class BTNode { public abstract NodeState Tick(); }
public class ConditionPlayerInRange : BTNode {
  public float range;
  public override NodeState Tick() {
    return Vector3.Distance(owner.transform.position, player.position) <= range ? NodeState.Success : NodeState.Failure;
  }
}
public class ActionMoveToPlayer : BTNode {
  public override NodeState Tick() {
    if (ai.MoveTowards(player.position)) return NodeState.Running;
    return NodeState.Success;
  }
}
```

**Cấu hình trong Prefab:**
```csharp
public abstract class BTNode { public abstract NodeState Tick(); }
public class ConditionPlayerInRange : BTNode {
  public float range;
  public override NodeState Tick() {
    return Vector3.Distance(owner.transform.position, player.position) <= range ? NodeState.Success : NodeState.Failure;
  }
}
public class ActionMoveToPlayer : BTNode {
  public override NodeState Tick() {
    if (ai.MoveTowards(player.position)) return NodeState.Running;
    return NodeState.Success;
  }
}
```

**Cấu hình trong Prefab:**
- Tạo child `BT_Root` trên `Enemy`, add component `BehaviorTree` và cấu hình root: ví dụ `Selector` với 2 nhánh:
  1) `Sequence` [ConditionPlayerInRange -> ActionMoveToPlayer]
  2) `Sequence` [ConditionAttackRange -> ActionAttack]
- Gán tham chiếu `player`, `Animator`, `Rigidbody`/`NavAgent` cho các nodes hoặc qua blackboard chung.

**Debug & Visualization:**
- Vẽ gizmos cho node đang active (ví dụ in màu, hoặc vẽ đường đi hành động).
- Thêm `Debug.Log` trong `Tick()` của node để trace chu kỳ đánh giá.
- (Optional) Viết `EditorWindow` nhỏ để inspect trạng thái cây runtime.

**Best practices ngắn:**
- Condition rẻ và không giữ trạng thái; lưu trạng thái chung (blackboard) nếu cần.
- Actions trả `Running` khi đang thực hiện (di chuyển, animation playing).
- Tách logic quyết định (BT) và logic effectors (movement, attack) qua interfaces/blackboard.

**Animator Setup:**
```
Parameters (Bool):
  - Run
  - BackWalk
  - Idle

Parameters (Trigger):
  - Attack
  - Hit
  - Death

States:
  Idle (default)
    ↓ [Run = true]
  Run
    ↓ [Attack trigger]
  Attack (0.5s)
    ↓ [timeout]
  BackWalk
    ↓ [BackWalk = false]
  Idle
```

**Animation Clip Timing:**
- **Run**: Loop, 0.5s per cycle
- **BackWalk**: Loop, 0.4s per cycle (chậm hơn)
- **Attack**: 0.5s, play 1x
- **Hit**: 0.3s, play 1x
- **Death**: 1.0s, play 1x

---

### Bước 2: Tạo Spawn Block trên Map

1. **Tạo GameObject:** `Enemy_Spawn_Block_1`
2. **Thêm Script:** `EnemySpawner.cs`

**EnemySpawner.cs Inspector:**
```
Spawn Settings:
  Enemy Prefab: [Drag Enemy prefab]
  Enemies To Spawn: 5
  Spawn Interval: 1.5
  Activation Distance: 20

Spawn Points:
  Size: 3
  Element 0-2: [Transform của spawn points]

Debug:
  Show Debug Info: FALSE
```

**Tạo Spawn Points (tùy chọn):**
- Tạo 3 empty GameObjects xung quanh block
- Đặt vào list "Spawn Points"
- Nếu để trống, spawn random xung quanh block

---

### Bước 3: Setup Scene

1. **Tạo AttackManager Singleton:**
   - Tạo empty: `EnemyAttackManager`
   - Attach script: `EnemyAttackManager.cs`
   - Inspector:
     ```
     Max Active Attackers: 2
     ```
   - **Lưu ý:** Chỉ cần 1 instance per scene

2. **Đảm bảo Tags:**
   - Player: tag = "Player"
   - Enemy: tag = "Enemy"

3. **Physics Settings:**
   - Gravity: (0, -9.81, 0)

---

## Behavior Tree Parameters Guide

### Detection Range (15m)
- Quái bắt đầu phát hiện player từ khoảng cách này
- Nếu player ngoài range → enemy idle (chờ)
- **Reduce to 10m**: Enemy ít aggressive
- **Increase to 20m**: Enemy long-range detection

### Attack Range (2m)
- Khoảng cách tối thiểu để enemy tấn công
- Nếu distanceToPlayer > 2m → chase
- Nếu distanceToPlayer ≤ 2m → tấn công
- **Match your weapon size** (typically 1.5-2.5m)

### Move Speed (3 m/s)
- Tốc độ khi active attacker chase player
- Áp dụng qua `rb.linearVelocity`
- **3 m/s ≈ 10.8 km/h** (trot speed)
- **Increase to 5 m/s**: Chase nhanh hơn
- **Decrease to 2 m/s**: Chase chậm hơn

### Retreat Speed (0.5 m/s)
- Tốc độ khi waiting attacker di chuyển LÙI
- **PHẢI CHẬM** để player có cơ hội escape
- 0.5 m/s ≈ 1.8 km/h (very slow walk)
- **KHÔNG THAY** trong gameplay (core design)

### Attack Cooldown (2 seconds)
- Thời gian chờ giữa các lần tấn công
- Bắt đầu đếm khi `animator.SetTrigger("Attack")`
- Damage được gây **0.5s sau trigger**
- **2s**: Balanced difficulty
- **Reduce to 1s**: Enemy tấn công nhanh hơn (hard)
- **Increase to 3s**: Enemy tấn công chậm hơn (easy)

---

## Gameplay Flow

### Khi Player Tiếp Cận Zone

```
1. Player trong 20m
   ↓
2. EnemySpawner.Update() kích hoạt
   ↓
3. Spawn enemy lần lượt (1.5s interval)
   ↓
4. Mỗi enemy:
     - Register với EnemyAttackManager
     - UpdateActiveAttackers()
     - Max 2 active, còn lại waiting
   ↓
5. Active attacker → Chase + Attack
   Waiting attacker → Di chuyển lùi
```

### Khi Quái Nhận Damage

```
1. Player hit enemy
   ↓
2. WeaponHitbox.OnTriggerEnter()
   ↓
3. Enemy.TakeDamage():
     - currentHealth -= 20
     - hitCount++
     - PlayAnimation("Hit")
     - ApplyKnockback()
   ↓
4. Nếu currentHealth <= 0:
     - Die()
     - EnemyAttackManager.UnregisterEnemy()
     - Promote waiting enemy
     - Destroy sau 2s
```

---

## Animator Setup

**Parameters:**
```
Bool:
  - Run
  - BackWalk
  - Idle

Trigger:
  - Attack
  - Hit
  - Death
```

**Animation States:**
```
Idle (default)
  ↓ [Run input]
Run (0.5s)
  ↓ [Attack trigger]
Attack (0.5s)
  ↓ [timeout]
BackWalk (loop)
  ↓ [Stop]
Idle
```

---

**Animator Setup:**
```
Parameters (Bool):
  - Run
  - BackWalk
  - Idle

Parameters (Trigger):
  - Attack
  - Hit
  - Death

States:
  Idle (default)
    ↓ [Run = true]
  Run
    ↓ [Attack trigger]
  Attack (0.5s)
    ↓ [timeout]
  BackWalk
    ↓ [BackWalk = false]
  Idle
```

**Animation Clip Timing:**
- **Run**: Loop, 0.5s per cycle
- **BackWalk**: Loop, 0.4s per cycle (chậm hơn)
- **Attack**: 0.5s, play 1x
- **Hit**: 0.3s, play 1x
- **Death**: 1.0s, play 1x

---

## Behavior Tree Parameters Explained

✅ **CPU**: Lightweight behavior tree updates per frame
✅ **Memory**: On-demand instantiate + proper cleanup
✅ **Physics**: Direct velocity, không forces
✅ **No FPS Drop**: Tested with 10+ enemies

---

## Debug

**Enable Logs:**
```csharp
// In Inspector:
Enemy.cs: showDebugInfo = TRUE
EnemyBehaviorTree.cs: [Add debug logs]
EnemySpawner.cs: showDebugInfo = TRUE
```

**Check Active Attackers:**
```csharp
int activeCount = EnemyAttackManager.GetActiveAttackerCount();
int waitingCount = EnemyAttackManager.GetWaitingEnemyCount();
```

---

## Customization

**Số quái tấn công:**
```csharp
// EnemyAttackManager inspector:
maxActiveAttackers = 3  // default 2
```

**AI Behavior:**
```csharp
// EnemyBehaviorTree inspector:
moveSpeed = 5           // chase nhanh hơn
retreatSpeed = 0.3      // lùi chậm hơn
attackRange = 3         // tấn công xa hơn
```

**Spawn Timing:**
```csharp
// EnemySpawner inspector:
spawnInterval = 2.0     // spawn chậm hơn
activationDistance = 30 // trigger xa hơn
```

---

## Troubleshooting

**Quái không di chuyển:**
- ✓ Rigidbody: Is Kinematic = FALSE
- ✓ Constraints: Không freeze X/Z

**Quái không spawn:**
- ✓ Enemy Prefab assigned?
- ✓ Player có tag "Player"?
- ✓ Player trong activation distance?

**Quá nhiều quái tấn công:**
- ✓ maxActiveAttackers = 2

**FPS Drop:**
- ✓ Giảm enemiesToSpawn
- ✓ Tăng spawnInterval
- ✓ Giảm detectionRange

---

## Files

```
Assets/Character/Scripts/
├── Enemy.cs (sửa)
├── EnemyBehaviorTree.cs (mới)
├── EnemyAttackManager.cs (mới)
├── EnemySpawner.cs (mới)
└── [existing files]
```

---

## Checklist

- [ ] Enemy Prefab: Rigidbody + Animator + Collider
- [ ] Enemy tag set
- [ ] Player tag set
- [ ] EnemyAttackManager scene instance
- [ ] EnemySpawner on spawn block
- [ ] Spawn points (hoặc để trống)
- [ ] Test spawn: Player gần zone
- [ ] Test attack: Max 2-3 quái tấn công
- [ ] Test AI: Quái khác di chuyển lùi

---

**Last Updated**: January 13, 2026
