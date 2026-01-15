# Hướng Dẫn Setup Hệ Thống Enemy

## Tổng Quan
Hệ thống enemy đơn giản: spawn tại block, rơi xuống đất, di chuyển quanh player trong khu vực X/Z, attack turn-based.

## Yêu Cầu
- Player có tag "Player".
- Ground có collider.
- Enemy prefab: Rigidbody (gravity on), CapsuleCollider, Animator (Idle/Walk/Attack/Hit/Death), scripts Enemy + EnemyAI.

## Setup
1. Tạo prefab enemy với components trên.
2. Tạo GameObject với EnemySpawner, gán prefab.
3. Tạo EnemyManager.
4. Test: Player vào trigger, enemies spawn và rơi.