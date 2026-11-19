# CLAUDE.md — Relic & Coin Project Rules

## Ngôn ngữ & Nền tảng
- **Unity 6 (2D URP)**, C# 10+
- Logic thuần túy viết bằng **Pure C# classes** hoặc **ScriptableObjects** — không phụ thuộc vào UnityEngine nếu không cần thiết
- **MonoBehaviour** chỉ dùng làm lớp mỏng (thin wrapper) để giao tiếp với Unity lifecycle; mọi business logic đặt trong lớp riêng

## Design Patterns bắt buộc

### Hierarchical State Machine (HSM/FSM)
- Mọi nhân vật (Player, Enemy, NPC/Staff) đều dùng HSM
- Mỗi state là một class riêng implement `IState`
- Transition logic nằm trong state hoặc context, không nằm rải rác trong `Update()`

### Observer Pattern
- Dùng **ScriptableObject Event Channels** cho giao tiếp giữa các hệ thống (UI ↔ Economy, Day/Night ↔ Shop)
- Dùng **C# Events / Actions** cho giao tiếp nội bộ trong cùng một hệ thống
- Không dùng singleton messaging; ưu tiên dependency injection qua Inspector

### Object Pooling
- **Bắt buộc** cho: đạn, hiệu ứng particle, spawn quái, floating damage text
- Dùng `UnityEngine.Pool.ObjectPool<T>` (built-in Unity 6)
- Không dùng `Instantiate`/`Destroy` trong gameplay loop

## Quy tắc Hiệu năng

```
- KHÔNG gọi GetComponent<T>() trong Update() / FixedUpdate()
- KHÔNG gọi GameObject.Find() hay FindObjectOfType() ở bất kỳ đâu ngoài Awake()/Start()
- Cache mọi component reference trong Awake()
- Tránh new() allocation trong Update() — dùng struct hoặc pool
- Dùng [System.Serializable] struct cho data thay vì class khi không cần kế thừa
- Ưu tiên Span<T> / NativeArray cho xử lý mảng lớn
```

## Quy tắc Code

- **Namespace**: `RelicAndCoin.Core`, `RelicAndCoin.Player`, `RelicAndCoin.Combat`, v.v.
- **Naming**: PascalCase cho class/method/property; camelCase cho field private; `_camelCase` cho private serialized field
- **ScriptableObjects**: đặt trong `Assets/_Project/ScriptableObjects/`, mỗi loại một thư mục con
- **Tuyệt đối không** dùng magic strings — mọi key/tag dùng `const string` hoặc `enum`
- Mỗi file chỉ chứa **một** class/struct/interface chính

## Cấu trúc Thư mục

```
Assets/_Project/
├── Scripts/
│   ├── Core/          # GameManager, EventChannels, SaveLoad, DayNight
│   ├── Player/        # PlayerController, PlayerStats, ClassSystem
│   ├── Combat/        # WeaponSystem, RuneSystem, SkillTree, Debuff
│   ├── Inventory/     # GridInventory, ItemData, CurseSystem
│   ├── Dungeon/       # DungeonGenerator, Room, PathFinding
│   ├── Shop/          # ShopManager, PriceBook, CustomerAI
│   ├── AI/            # EnemyFSM, BossPhase, StaffAI, AStarPathfinder
│   └── UI/            # HUD, InventoryUI, ShopUI, DialogueUI
├── ScriptableObjects/ # SO assets (Events, Items, Stats, Weapons, Runes)
├── Prefabs/           # Player, Enemies, Weapons, UI, VFX prefabs
├── Scenes/            # MainMenu, DungeonScene, ShopScene, etc.
├── Art/               # Sprites, Tilesets, Animations
└── Audio/             # Music, SFX
```

## Quy tắc Làm việc với Claude Code

> **Mỗi lượt tối đa 4–5 file.** Sau khi hoàn thành một lượt, Claude phải **dừng lại, tóm tắt kết quả và xin phép** trước khi tiếp tục lượt tiếp theo.

- Chỉ dùng công cụ: `Read`, `Write`, `Edit`, `Glob`, `Grep`, `Bash`
- **Tuyệt đối không** dùng `TaskCreate`, `TaskUpdate`, `TaskList` hay bất kỳ `mcp__*` tool nào
- Trước khi viết code mới, luôn `Glob`/`Grep` để kiểm tra file đã tồn tại chưa
- Giải thích ngắn gọn lý do kỹ thuật của mọi quyết định thiết kế quan trọng

## Giai đoạn Hiện tại
Xem `PLAN.md` để biết trạng thái tiến độ.
