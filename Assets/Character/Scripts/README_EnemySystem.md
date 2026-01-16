# Hệ Thống Enemy Giống Dark Souls

Hướng dẫn setup hệ thống enemy với AI chase, attack, retreat, spawn theo vùng, và quản lý attack cooldown.

## Tổng Quan
- **EnemyManager**: Quản lý vùng spawn, spawn enemy khi player vào vùng, đảm bảo chỉ 1 enemy attack tại 1 thời điểm với cooldown 3s.
- **Enemy**: AI với states Idle, Chase, Attack, Retreat. Chỉ hoạt động trong vùng manager.

## Bước Setup

### 1. Chuẩn Bị Player
- Đảm bảo Player GameObject có **tag "Player"**.
- Player cần có Collider (nếu chưa có).

### 2. Tạo Enemy Prefab
1. Tạo GameObject mới cho Enemy.
2. Attach các component:
   - **Rigidbody**: Set constraints nếu cần (e.g., freeze rotation Y nếu không muốn quay).
   - **Animator**: Với Animator Controller có states và transitions cho "Idle", "Run", "Attack", "Hit", "Death".
   - **Capsule Collider** hoặc **Box Collider**: Cho collision.
   - **Enemy.cs** script.
3. Trong Inspector của Enemy GameObject:
   - **Tag**: Set thành "Enemy" (tạo tag mới nếu chưa có).
4. Trong Inspector của Enemy.cs:
   - **Stats**: maxHealth = 100.
   - **AI Settings**:
     - detectionRange = 15 (khoảng cách phát hiện player).
     - attackRange = 2 (khoảng cách attack).
     - retreatDistance = 5 (khoảng cách lùi sau attack).
     - moveSpeed = 5 (tốc độ di chuyển).
     - attackDelay = 1 (thời gian chuẩn bị attack).
   - **Knockback Settings**: Giữ nguyên hoặc điều chỉnh.
5. Lưu làm Prefab trong Assets/Character/Prefabs/ (tạo thư mục nếu cần).

### 3. Tạo EnemyManager
1. Tạo empty GameObject, đặt tên "EnemyManager_Zone1" (hoặc tương tự).
2. Attach **EnemyManager.cs** script.
3. Attach **Box Collider** hoặc **Sphere Collider**:
   - Set **Is Trigger** = true.
   - Điều chỉnh Size để định vùng hoạt động (e.g., Size = 20x20x20 cho vùng vuông).
4. Trong Inspector của EnemyManager.cs:
   - **Spawn Settings**:
     - enemyPrefab: Drag prefab Enemy vào.
     - maxEnemies = 5 (số enemy tối đa trong vùng).
     - spawnRadius = 10 (bán kính spawn xung quanh center).
     - spawnHeight = 0 (chiều cao spawn).
   - **Attack Management**:
     - attackCooldown = 3 (delay giữa các attack).

### 4. Đặt Trên Map
- Đặt EnemyManager GameObject tại vị trí mong muốn trên map.
- Có thể tạo nhiều EnemyManager cho các vùng khác nhau.
- Đảm bảo vùng không overlap nếu không muốn spawn chồng chéo.

### 5. Animation Setup (Tùy Chọn)
- Trong Animator của Enemy:
  - Thêm parameters: Trigger "Attack", "Hit", "Death".
  - States: Idle, Run, Attack, Hit, Death.
  - Transitions: Từ Idle -> Run khi chase, Run -> Attack khi attack, etc.
- Trong Enemy.cs, animation được trigger tự động.

### 6. Test
1. Chạy scene.
2. Di chuyển Player vào vùng EnemyManager.
3. Enemy sẽ spawn và bắt đầu chase.
4. Khi gần, 1 enemy attack, lùi lại, rồi tiếp tục.
5. Nếu player ra khỏi vùng, enemy không đuổi theo.

## Lưu Ý
- **Performance**: Nếu nhiều enemy, tối ưu bằng Object Pooling.
- **Customization**: Có thể thêm damage player trong PerformAttack() coroutine.
- **Debug**: Set showDebugInfo = true trong Enemy.cs để log.
- **Lỗi Thường Gặp**:
  - "Player not found": Kiểm tra tag "Player".
  - Enemy không di chuyển: Kiểm tra Rigidbody không bị freeze.
  - Không spawn: Kiểm tra Collider là Trigger và Player có Collider.

## Code Overview
- **EnemyManager.cs**: Spawn, quản lý attack.
- **Enemy.cs**: AI states, movement, knockback.

Nếu cần chỉnh sửa, liên hệ dev!</content>
<parameter name="filePath">d:\unity\bladepersuit\Assets\Character\Scripts\README_EnemySystem.md