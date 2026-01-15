# Blade Pursuit - Game Architecture Summary

## Project Overview

**Blade Pursuit** là một third-person action game với combat system gồm combo attacks, rolling mechanics, và enemy knockback physics. Được xây dựng trên Unity Input System với player-centric controller pattern.

---

## Core Systems Architecture

### 1. **Input System (PlayerInputActions)**
- Actions: Move (WASD), Look (Mouse), Sprint (Shift), Attack (LMB), Roll (RMB)
- Auto-generated class từ InputActionAsset
- Subscribe callbacks across controllers

### 2. **Third-Person Camera (ThirdPersonCamera)**
- **Fixed Distance**: 5m từ player
- **Fixed Height**: 2m trên player
- **Mouse-Only Rotation**: Pitch (-30° to 60°), Yaw (360°)
- **Collision Detection**: SphereCast để avoid clipping
- Camera-relative movement
- Cursor locking (ESC/Click toggle)

### 3. **Movement System (ThirdPersonController)**
- **Speed**: 5 m/s walk, 8 m/s sprint
- **Camera-Relative**: Tất cả input relative to camera direction
- **Movement Lock** (during):
  - Attack execution
  - Rolling
  - Impact/stun state
  - Death
- **Rotation**: Smooth lerp (10 m/s)
- **Gravity**: -9.81 m/s²

### 4. **Attack Combo System (AttackComboController)**
- **3-Hit Combo**: Slash 1 → Slash 2 → Slash 3
- **Input Window**: Animation events control input acceptance
- **Weapon Hitbox**: Integration với damage detection
- **Dash Movement**: 1.5m forward over 0.2s
- **Interrupt**: ForceResetCombo() on damage/stun

### 5. **Roll/Dodge System (RollController)**
- **Distance**: 4m roll distance
- **Duration**: 0.6s animation
- **I-Frames**: 20%-70% of animation (0.2s-0.7s)
- **Cooldown**: 1 second
- **Layer-Based Invincibility**: Physics layer collision
- **Forced End**: ForceEndRoll() on interrupt

### 6. **Health & Damage System (PlayerHealth)**
- **Max Health**: 100
- **Damage Immunity**: 1 second after hit
- **Impact Duration**: 0.7 seconds (ALL INPUTS LOCKED)
- **Knockback**: FROM attacker TO player, 5 m/s force
- **Knockback Duration**: 0.3 seconds with drag
- **Death Sequence**: Play animation → Delay 3s → Respawn/Game Over

### 7. **Enemy Combat (Enemy)**
- **Knockback Modes**:
  1. Player Forward Direction (current): Predictable
  2. Position-Based: Away from player
- **Physics**: Direct Rigidbody velocity, không forces
- **Knockback Velocity**: `(direction * force) + (up * upwardForce)`
- **Hit Tracking**: hitCount per enemy

### 8. **Enemy AI System (NEW)**

**EnemyBehaviorTree:**
- Active Attacker: Chase (3 m/s) → Attack (2s cooldown)
- Waiting Attacker: Retreat (0.5 m/s) chậm
- Detection Range: 15m
- Attack Range: 2m

**EnemyAttackManager (Singleton):**
- Max 2-3 active attackers
- Auto-promote waiting enemies
- Register/Unregister on spawn/death

**EnemySpawner:**
- Spawn lần lượt (1.5s interval)
- Trigger: Player gần 20m
- Customizable: Số lượng + spawn points

### 9. **Environmental Damage (DamageTrigger)**
- Damage: 20 per hit
- Cooldown: 1 second
- Optional: Continuous damage
- Visual: Flash effect

---

## State Flow Diagram

```
┌──────────┐
│  IDLE    │
└────┬─────┘
     │ Input detected
     ├─────────────────────────────────────┐
     │                                     │
     ↓                                     ↓
┌──────────┐                        ┌──────────┐
│ MOVING   │ ←─ Camera input ─→    │ SPRINT   │
└────┬─────┘ ←─ Shift pressed ─→   └──────────┘
     │
     ├─ LMB → ┌──────────┐
     │         │ ATTACK   │ (3-hit combo)
     │         │ Combo 1-3│
     │         └──────────┘
     │
     ├─ RMB → ┌──────────┐
     │         │  ROLL    │ (I-frames 20-70%)
     │         │ (0.6s)   │
     │         └──────────┘
     │
     └─ Take Damage ──→ ┌──────────────┐
                        │   IMPACT     │ (Locked 0.7s)
                        │  Knockback   │
                        │ + I-frames   │
                        └──────┬───────┘
                               │
                               ├─ HP > 0 → IDLE
                               │
                               └─ HP <= 0 → DEAD
                                            (Respawn or Game Over)
```

---

## Data Flow: Attack Combo

```
1. Player Click (LMB)
   ↓
2. AttackComboController.OnAttackInput()
   ↓
3. Check States:
   - Not rolling? ✓
   - Not in impact? ✓
   - Not dead? ✓
   ↓
4. StartCombo(1):
   - animator.attackIndex = 1
   - isExecutingAttack = true
   - Movement locked
   ↓
5. Animation Plays (0.6s)
   ├─ Frame 15% → EnableWeaponDamage()
   │   └─ WeaponHitbox detects enemies
   ├─ Frame 60% → EnableNextInput()
   │   └─ canReceiveInput = true (can queue combo 2)
   └─ Frame 70% → DisableWeaponDamage()
   ↓
6. Enemy Hit:
   - WeaponHitbox.OnTriggerEnter()
   - enemy.TakeDamage(20, playerPos, playerForward)
   - Enemy knockback applied
   - hitCount++
   ↓
7. Animation End → FinishCombo()
   - Movement unlocked
   - Wait for next input
```

---

## Critical Variables

| System | Variable | Value | Purpose |
|--------|----------|-------|---------|
| Camera | Distance | 5m | Fixed camera |
| Camera | Height | 2m | Fixed height |
| Camera | Mouse Sens | 2x | Rotation |
| Movement | Walk Speed | 5 m/s | Base speed |
| Movement | Sprint Speed | 8 m/s | Sprint boost |
| Attack | Dash Dist | 1.5m | Attack movement |
| Attack | Dash Dur | 0.2s | Movement time |
| Roll | Distance | 4m | Roll travel |
| Roll | Duration | 0.6s | Animation length |
| Roll | I-Frame Start | 20% | Invincibility start |
| Roll | I-Frame End | 70% | Invincibility end |
| Roll | Cooldown | 1s | Between rolls |
| Health | Max HP | 100 | Player max health |
| Health | Damage Immunity | 1s | Post-hit invincibility |
| Health | Impact Duration | 0.7s | Stun/frozen time |
| Health | Knockback Force | 5 m/s | Enemy knockback mag |
| Enemy AI | Detection Range | 15m | Chase distance |
| Enemy AI | Attack Range | 2m | Melee range |
| Enemy AI | Move Speed | 3 m/s | Chase speed |
| Enemy AI | Retreat Speed | 0.5 m/s | Waiting speed |
| Spawner | Spawn Interval | 1.5s | Between spawns |
| Spawner | Activation Dist | 20m | Trigger distance |

---

## Design Principles

✅ **State Isolation**: Mỗi controller quản lý 1 aspect
✅ **Animation-Driven**: Animation events = game logic source
✅ **Interrupt Priority**: Damage/stun → force reset
✅ **Physics-Based**: Velocity, không impulses
✅ **Layer Invincibility**: Layer collision vs invincible window
✅ **Camera Binding**: Tất cả direction = camera-relative

---

## Integration Checklist

- [x] All controllers reference Player
- [x] Animator parameter sync
- [x] Input System enable/disable
- [x] Layer collision matrix
- [x] Physics continuous simulation
- [x] Rigidbody gravity
- [x] Tags: Player, Enemy
- [x] Enemy AI systems
- [x] Spawn mechanics

---

## Performance

- ✅ No physics lag: Velocity-based
- ✅ Lightweight AI: Simple behavior tree
- ✅ Memory efficient: On-demand spawn/destroy
- ✅ No FPS drop: Tested with 10+ enemies

---

**Last Updated**: January 13, 2026
