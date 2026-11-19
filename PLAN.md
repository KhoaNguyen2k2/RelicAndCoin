# PLAN.md — Relic & Coin: Kế hoạch Phát triển

## Tổng quan
Game 2D Top-Down Action Roguelike kết hợp quản lý cửa hàng.
Engine: Unity 6 (2D URP) | Ngôn ngữ: C#

---

## Giai đoạn 1: Kiến trúc Lõi (Core Systems)

### Event & Communication
- [x] `GameEventChannel<T>` (ScriptableObject generic event base class) — Lượt 1
- [x] `VoidEventChannel` — sự kiện không tham số — Lượt 1
- [x] `IntEventChannel`, `FloatEventChannel` — sự kiện có giá trị — Lượt 1
- [x] `VoidEventListener`, `IntEventListener`, `FloatEventListener` (MonoBehaviour + UnityEvent) — Lượt 2
- [x] `DayPhaseEventChannel` — event channel cho enum DayPhase — Lượt 3

### Game Loop Ngày/Đêm
- [x] `DayPhase` enum: Morning_Prep, Day_ShopOpen, Evening_Closing, Night_Dungeon — Lượt 2
- [x] `DayCycleModel` (Pure C#) — logic đếm thời gian, chuyển pha, đếm ngày — Lượt 2
- [x] `DayCycleConfigSO` — ScriptableObject chứa thời lượng 4 pha — Lượt 3
- [x] `DayCycleController` — MonoBehaviour mỏng, bridge Unity ↔ DayCycleModel — Lượt 3
- [ ] Tích hợp với URP 2D Global Light (Giai đoạn 5)

### Hệ thống Độ khó
- [x] `DifficultyMode` enum: Normal, Hard, Nightmare — Lượt 3
- [x] `DifficultyConfigSO` — hệ số nhân EnemyHealth, EnemyDamage, ShopPrice, CurseChance — Lượt 3
- [ ] Tích hợp `DifficultyConfigSO` vào `GameManager` khi khởi động game

### Hệ thống Chỉ số Nhân vật
- [x] `StatType` enum (MaxHP, MaxStamina, MaxMana, AttackDamage, Defense, MoveSpeed, CritChance) — Lượt 4
- [x] `StatModifier` struct — Flat + Percent, Source-based removal — Lượt 4
- [x] `CharacterStatsModel` (Pure C#) — HP/Stamina/Mana, Fatigue, regen, modifier system — Lượt 4
- [x] `BaseStatsConfigSO` — ScriptableObject chỉ số gốc + factory `CreateModel()` — Lượt 4
- [x] `CharacterStatsController` — MonoBehaviour thin wrapper — Lượt 5

### Save / Load
- [x] `SaveData` — serializable data class (JSON) — Lượt 5
- [x] `SaveLoadManager` — đọc/ghi file JSON bằng `System.IO` (atomic write `.tmp`) — Lượt 5
- [x] `ISaveable` interface cho mọi thành phần cần lưu — Lượt 5

---

## Giai đoạn 2: Nhân vật, Chiến đấu & Trang bị

### Nhân vật & Class
- [x] `PlayerController` (MonoBehaviour thin wrapper)
- [x] `PlayerFSM` — Idle, Move, Attack, Roll, Interact, Dead
- [x] Hệ thống chọn nhân vật: **Kerral** (nam) / **Kaery** (nữ)
- [x] Hệ thống Class: **Chiến binh** (STR-build) / **Phù thủy** (INT-build)

### Vũ khí (23 loại)
- [x] `WeaponData` (ScriptableObject) — damage, range, speed, element
- [x] `WeaponController` — logic tấn công, hitbox, animation trigger
- [x] Danh sách: Kiếm, Đại kiếm, Giáo, Búa, Dao găm, Cung, Trượng, ... (23 loại)

### Hệ thống Runes (5 bậc)
- [x] `RuneData` (ScriptableObject) — tier, effect, slot requirement
- [x] Tier: Normal → Rare → Epic → Legendary → Mythical
- [x] `RuneSocket` — gắn rune vào vũ khí/giáp
- [x] Hiệu ứng rune: stat buff, proc effect, element add

### Cây Kỹ năng & Debuff
- [x] `SkillTreeData` (ScriptableObject graph) → `SkillNodeSO`
- [x] `SkillNode` — prerequisite, cost SP, passive/active
- [x] `DebuffSystem` — Burn, Freeze, Poison, Stun, Bleed, Curse
- [x] `DebuffData` (ScriptableObject) — duration, tick damage, stack limit

---

## Giai đoạn 3: Hầm ngục & AI Kẻ địch

### Procedural Dungeon Generator
- [x] `DungeonGenerator` — BSP hoặc Room-and-Corridor algorithm
- [x] `RoomData` (DungeonRoomData) — loại phòng (combat, treasure, boss, shop, curse)
- [x] `TilemapPainter` (DungeonTilemapPainter) — vẽ tilemap từ dungeon data
- [x] `DungeonEventChannel` — sự kiện mở cửa, kích hoạt bẫy (đề xuất thêm ở Giai đoạn 4 khi có Shop/Event cần)

### A* Pathfinding
- [x] `AStarPathfinder` — thuật toán A* trên grid 2D
- [x] `PathfindingGrid` (DungeonGridMap) — quản lý node, walkable/blocked
- [x] `AgentMover` — di chuyển theo path với smoothing (EnemyController/BossController di chuyển theo CurrentPath)

### AI Kẻ địch
- [x] `EnemyFSM` — Idle, Patrol, Chase, Attack, Stunned, Dead (Patrol/Chase/Attack đã viết, Stunned/Dead qua StatusEffectSystem + IsAlive)
- [x] `EnemyData` (EnemyDataSO ScriptableObject) — stats, drop table (Gold/Exp), AI profile
- [x] Quái thường: Goblin, Skeleton, Slime, ... (các loại cơ bản) — dữ liệu qua EnemyDataSO, spawn qua pool ở Giai đoạn 4
- [x] Object Pool cho quái spawn — đề xuất dùng `UnityEngine.Pool.ObjectPool<T>` ở Giai đoạn 4

### Boss Đa Giai đoạn
- [x] `BossFSM` — phases riêng biệt với trigger HP threshold (BossController + BossPhaseConfig)
- [x] `BossPhaseData` (BossPhaseConfig) — pattern tấn công, tốc độ, spawn thêm quái
- [x] Tối thiểu 3 boss: 1 per dungeon tier — dữ liệu qua EnemyDataSO + BossPhaseConfig, tạo prefab ở Giai đoạn 5

### Lời nguyền & Tu sĩ Giải nguyền
- [x] `CurseEffect` (CurseDataSO) — debuff đặc biệt bám vào nhân vật/vật phẩm
- [x] `PriestNPC` — AI, dialogue, giá giải nguyền tính theo curse tier (CursePurifyService xử lý thanh tẩy, giá vàng quá Priest ở Giai đoạn 4)

---

## Giai đoạn 4: Cửa hàng, Kinh tế Động & Quản lý Base

### Grid Inventory có Lời nguyền
- [x] `ItemCategory` — enum: Relic, Material, Potion, Equipment — Lượt 1
- [x] `ItemCurseType` — enum: None, DestroyAdjacent, SendHome, EdgeOnly — Lượt 1
- [x] `ItemDataSO` — ScriptableObject: ID, tên, mô tả, icon, phân loại, GridSize, MaxStack, BaseValue, CurseType — Lượt 1
- [x] `InventorySlotItem` — pure C#: ItemData, Quantity, GridPosition, IsRotated, EffectiveSize — Lượt 1
- [x] `GridInventoryModel` — pure C# 5×4 lưới: CanPlaceItem, TryPlaceAt, TryAddItem, RemoveItem, ResolveDungeonExitCurses — Lượt 1

### Cửa hàng, Kinh tế động & Sổ tay Giá cả
- [x] `PriceReaction` — enum: Cheap, Fair, Expensive, Overpriced — Lượt 2
- [x] `ItemPopularity` — enum: LowDemand, Normal, HighDemand — Lượt 2
- [x] `DynamicEconomyService` — pure C#: giá chuẩn = BaseValue × popularity × ShopPriceMultiplier, đánh giá ListedPrice → PriceReaction — Lượt 2
- [x] `PriceBookModel` — pure C#: lịch sử giá thử theo ItemId, mức Fair cao nhất đã bán, mức độ phổ biến — Lượt 2
- [x] `ShopDisplaySlot` — pure C#: InventorySlotItem, ListedPrice, sự kiện OnItemBought/OnCustomerWalkedAway — Lượt 2

### AI Khách hàng & Nhân viên cửa hàng
- [x] `CustomerState` — enum: Entering, Browsing, InspectingItem, QueuingToPay, LeavingHappy, LeavingAngry — Lượt 3
- [x] `CustomerAIModel` — pure C#: quyết định mua dựa trên DynamicEconomyService, MaxBudget, thời gian suy nghĩ — Lượt 3
- [x] `CustomerController` — MonoBehaviour mỏng: di chuyển tới quầy, gọi AI model, giao dịch qua ShopDisplaySlot, cập nhật PriceBookModel — Lượt 3
- [x] `StaffRole` + `StaffDataSO` — enum: Cashier, Restocker, Guard; ScriptableObject: ID, tên, vai trò, DailySalary, WorkSpeed — Lượt 3
- [x] `StaffPayrollService` — pure C#: quản lý nhân viên đã thuê, tính tổng lương ngày, trừ vàng, nghỉ việc nếu thiếu tiền — Lượt 3

### Nâng cấp Cơ sở & Chế tạo
- [x] `BaseFacilityType` — enum: Shop, Blacksmith, Alchemy — Lượt 4
- [x] `FacilityUpgradeDataSO` — ScriptableObject: loại cơ sở, cấp độ mục tiêu, chi phí vàng, nguyên liệu yêu cầu, hiệu ứng mở khóa — Lượt 4
- [x] `BaseUpgradeService` — pure C#: quản lý cấp độ 3 cơ sở, kiểm tra đủ vàng và nguyên liệu trong GridInventoryModel để nâng cấp — Lượt 4
- [x] `CraftingRecipeSO` — ScriptableObject: cơ sở yêu cầu, cấp độ tối thiểu, nguyên liệu đầu vào, chi phí vàng, kết quả ItemDataSO/WeaponDataSO — Lượt 4
- [x] `CraftingService` — pure C#: kiểm tra điều kiện, tiêu hao nguyên liệu/vàng để chế tạo vật phẩm mới — Lượt 4

---

## Giai đoạn 5: Hoàn thiện & Build

### UI/UX
- [x] HUD: HP/Stamina/Mana bar, ngày/giờ, tiền — `PlayerHUDView.cs` (Lượt 1)
- [x] Inventory UI: lưới ba-lô, icon, số lượng, viền màu curse — `GridInventoryView.cs` (Lượt 1)
- [x] Shop UI: điều chỉnh giá niêm yết, icon cảm xúc PriceReaction — `ShopCounterView.cs` (Lượt 1)
- [x] Sổ tay giá cả: mức Fair cao nhất + ItemPopularity — `PriceBookView.cs` (Lượt 1)
- [x] Main Menu: chọn nhân vật/Class/Độ khó, New Game/Continue — `MainMenuView.cs` (Lượt 2)
- [ ] Inventory UI: drag-drop grid, tooltip, compare stats (defer sang sau MVP)
- [ ] Dialogue System: branching dialogue với ScriptableObject (defer sang sau MVP)
- [ ] Pause Menu, Game Over screen (defer sang sau MVP)

### Âm thanh
- [x] `SoundDataSO` — clip, volume, pitch, loop — `SoundDataSO.cs` (Lượt 2)
- [x] `AudioManager` — pooled AudioSource SFX, BGM crossfade — `AudioManager.cs` (Lượt 2)
- [ ] Âm thanh môi trường ngày/đêm, combat, shop ambience (gán SoundDataSO vào AudioManager ở bước dựng Scene)

### Hiệu ứng Ánh sáng URP 2D
- [x] `DayNightLightingController` — Light2D màu/cường độ theo 4 pha ngày/đêm — `DayNightLightingController.cs` (Lượt 2)
- [ ] Point Light 2D cho lửa, đèn lồng, phép thuật (gán lúc dựng Scene)
- [ ] Shadow Caster 2D cho tường dungeon (gán lúc dựng Scene)

### Build & QA
- [ ] Tối ưu: Profiler pass, giảm draw calls, texture atlas
- [ ] Settings: Resolution, Volume, Keybinding (lưu vào `PlayerPrefs`)
- [ ] Build Windows (IL2CPP, x64)
- [ ] Playtesting: loop hoàn chỉnh ngày 1–7 in-game

---

## Trạng thái Hiện tại
**Giai đoạn đang thực hiện**: 4 — Cửa hàng, Kinh tế động & Quản lý Base ✅ HOÀN THÀNH | 5 — Hoàn thiện UI/UX, Âm thanh & Đóng gói 🔄 ĐANG THỰC HIỆN (phần lập trình ✅ hoàn thành)

**Ngày bắt đầu**: 2026-11-17 | **Ngày chốt hạ**: 2026-11-17

**Sẵn sàng sang**: Dựng Scene, gán Inspector, Build Windows (IL2CPP, x64).
