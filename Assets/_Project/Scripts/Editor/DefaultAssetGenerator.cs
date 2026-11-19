#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using RelicAndCoin.Core.Events;
using RelicAndCoin.Core.Time;
using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Player;
using RelicAndCoin.Combat.Weapons;
using RelicAndCoin.AI.Enemies;
using RelicAndCoin.Inventory;
using RelicAndCoin.Combat;
using RelicAndCoin.Dungeon;

namespace RelicAndCoin.Editor
{
    /// <summary>
    /// Công cụ sinh sẵn các ScriptableObject mẫu (default assets) cho dự án Relic & Coin.
    /// Mở menu: RelicAndCoin > Generate Default Assets.
    /// </summary>
    public static class DefaultAssetGenerator
    {
        private const string Root = "Assets/_Project/ScriptableObjects";
        private static readonly string[] SubFolders =
        {
            "Events", "Configs", "Classes", "Weapons", "Runes", "Enemies", "Items", "Curses"
        };

        [MenuItem("RelicAndCoin/Generate Default Assets")]
        public static void GenerateDefaultAssets()
        {
            EnsureFolders();

            CreateEventChannels();
            CreateConfigs();
            CreateClasses();
            CreateWeapons();
            CreateEnemies();
            CreateItems();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[DefaultAssetGenerator] Đã tạo/cập nhật xong các default asset trong '{Root}'.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(Root))
            {
                Directory.CreateDirectory(Root);
                AssetDatabase.Refresh();
            }

            foreach (var folder in SubFolders)
            {
                string path = $"{Root}/{folder}";
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(Root, folder);
            }
        }

        private static T LoadOrCreate<T>(string relativePath) where T : ScriptableObject
        {
            string fullPath = $"{Root}/{relativePath}".Replace("\\", "/");
            T existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (existing != null)
                return existing;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            return asset;
        }

        private static void SetSerialized<T>(T asset, string fieldName, object value) where T : ScriptableObject
        {
            SerializedObject so = new SerializedObject(asset);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[DefaultAssetGenerator] Không tìm thấy trường '{fieldName}' trên {asset.name}");
                return;
            }

            if (value is string s) prop.stringValue = s;
            else if (value is int i) prop.intValue = i;
            else if (value is float f) prop.floatValue = f;
            else if (value is bool b) prop.boolValue = b;
            else if (value is Enum e) prop.enumValueIndex = Convert.ToInt32(e);
            else if (value is Vector2Int v2i) prop.vector2IntValue = v2i;
            else
            {
                Debug.LogWarning($"[DefaultAssetGenerator] Kiểu không hỗ trợ gánSerialized: {fieldName} ({value.GetType()})");
                return;
            }

            so.ApplyModifiedProperties();
        }

        private static void MarkDirty<T>(T asset) where T : ScriptableObject =>
            EditorUtility.SetDirty(asset);

        // ── Events ──────────────────────────────────────────────

        private static void CreateEventChannels()
        {
            CreateAssetAndMark<FloatEventChannel>("Events/OnPlayerHealthChanged.asset");
            CreateAssetAndMark<FloatEventChannel>("Events/OnPlayerStaminaChanged.asset");
            CreateAssetAndMark<FloatEventChannel>("Events/OnPlayerManaChanged.asset");
            CreateAssetAndMark<IntEventChannel>("Events/OnGoldChanged.asset");
            CreateAssetAndMark<DayPhaseEventChannel>("Events/OnDayPhaseChanged.asset");
            CreateAssetAndMark<VoidEventChannel>("Events/OnFatigueEntered.asset");
            CreateAssetAndMark<VoidEventChannel>("Events/OnFatigueExited.asset");
        }

        private static void CreateAssetAndMark<T>(string relativePath) where T : ScriptableObject
        {
            T asset = LoadOrCreate<T>(relativePath);
            MarkDirty(asset);
        }

        // ── Configs ─────────────────────────────────────────────

        private static void CreateConfigs()
        {
            DayCycleConfigSO dayCycle = LoadOrCreate<DayCycleConfigSO>("Configs/DefaultDayCycleConfig.asset");
            dayCycle.MorningPrepDuration = 60f;
            dayCycle.DayShopOpenDuration = 180f;
            dayCycle.EveningClosingDuration = 60f;
            dayCycle.NightDungeonDuration = 120f;
            MarkDirty(dayCycle);

            BaseStatsConfigSO baseStats = LoadOrCreate<BaseStatsConfigSO>("Configs/DefaultBaseStats.asset");
            baseStats.MaxHP = 100f;
            baseStats.MaxStamina = 100f;
            baseStats.MaxMana = 100f;
            baseStats.StaminaRegenPerSecond = 10f;
            baseStats.ManaRegenPerSecond = 5f;
            baseStats.FatigueDuration = 3f;
            baseStats.AttackDamage = 10f;
            baseStats.Defense = 0f;
            baseStats.MoveSpeed = 5f;
            baseStats.CritChance = 0.05f;
            MarkDirty(baseStats);

            PlayerMovementConfigSO movement = LoadOrCreate<PlayerMovementConfigSO>("Configs/DefaultPlayerMovement.asset");
            movement.WalkSpeed = 5f;
            movement.SprintSpeed = 8f;
            movement.DashSpeed = 15f;
            movement.DashDuration = 0.3f;
            movement.DashCooldown = 1.5f;
            movement.SprintStaminaCostPerSecond = 10f;
            movement.DashStaminaCost = 20f;
            MarkDirty(movement);

            DifficultyConfigSO normal = LoadOrCreate<DifficultyConfigSO>("Configs/Difficulty_Normal.asset");
            normal.Mode = DifficultyMode.Normal;
            normal.EnemyHealthMultiplier = 1f;
            normal.EnemyDamageMultiplier = 1f;
            normal.PatrolSpeedMultiplier = 1f;
            normal.DetectRangeMultiplier = 1f;
            normal.AttackCooldownMultiplier = 1f;
            normal.CanDodgeAndPunish = false;
            normal.ShopPriceMultiplier = 1f;
            normal.CurseChanceMultiplier = 1f;
            MarkDirty(normal);

            DifficultyConfigSO hard = LoadOrCreate<DifficultyConfigSO>("Configs/Difficulty_Hard.asset");
            hard.Mode = DifficultyMode.Hard;
            hard.EnemyHealthMultiplier = 1f;
            hard.EnemyDamageMultiplier = 1.3f;
            hard.PatrolSpeedMultiplier = 1.15f;
            hard.DetectRangeMultiplier = 1.25f;
            hard.AttackCooldownMultiplier = 0.85f;
            hard.CanDodgeAndPunish = false;
            hard.ShopPriceMultiplier = 1.2f;
            hard.CurseChanceMultiplier = 1.2f;
            MarkDirty(hard);

            DifficultyConfigSO nightmare = LoadOrCreate<DifficultyConfigSO>("Configs/Difficulty_Nightmare.asset");
            nightmare.Mode = DifficultyMode.Nightmare;
            nightmare.EnemyHealthMultiplier = 1f;
            nightmare.EnemyDamageMultiplier = 1.8f;
            nightmare.PatrolSpeedMultiplier = 1.3f;
            nightmare.DetectRangeMultiplier = 1.5f;
            nightmare.AttackCooldownMultiplier = 0.7f;
            nightmare.CanDodgeAndPunish = true;
            nightmare.ShopPriceMultiplier = 1.5f;
            nightmare.CurseChanceMultiplier = 1.5f;
            MarkDirty(nightmare);

            DungeonConfigSO dungeon = LoadOrCreate<DungeonConfigSO>("Configs/DefaultDungeonConfig.asset");
            dungeon.GridWidth = 20;
            dungeon.GridHeight = 15;
            dungeon.MinRoomCount = 6;
            dungeon.MaxRoomCount = 12;
            dungeon.MinRoomSize = new Vector2Int(4, 4);
            dungeon.MaxRoomSize = new Vector2Int(8, 6);
            dungeon.TreasureRoomChance = 0.15f;
            dungeon.ShopRoomChance = 0.10f;
            MarkDirty(dungeon);
        }

        // ── Classes ─────────────────────────────────────────────

        private static void CreateClasses()
        {
            PlayerClassConfigSO warrior = LoadOrCreate<PlayerClassConfigSO>("Classes/Class_Warrior.asset");
            warrior.ClassType = PlayerClassType.Warrior;
            warrior.ClassName = "Chiến binh";
            warrior.Description = "Sức mạnh và phòng thủ, ưu tiên Stamina.";
            warrior.StartingWeaponPrefab = null;
            warrior.StatModifiers = new[]
            {
                new StatModifier(StatType.MaxHP, ModifierType.Flat, 30f, null),
                new StatModifier(StatType.Defense, ModifierType.Flat, 3f, null),
                new StatModifier(StatType.AttackDamage, ModifierType.Flat, 3f, null),
            };
            warrior.ManaPriority = 0.5f;
            warrior.StaminaPriority = 1.5f;
            MarkDirty(warrior);

            PlayerClassConfigSO mage = LoadOrCreate<PlayerClassConfigSO>("Classes/Class_Mage.asset");
            mage.ClassType = PlayerClassType.Mage;
            mage.ClassName = "Phù thủy";
            mage.Description = "Sát thương phép và Mana, ưu tiên Mana.";
            mage.StartingWeaponPrefab = null;
            mage.StatModifiers = new[]
            {
                new StatModifier(StatType.MaxMana, ModifierType.Flat, 30f, null),
                new StatModifier(StatType.AttackDamage, ModifierType.Percent, 0.1f, null),
            };
            mage.ManaPriority = 1.5f;
            mage.StaminaPriority = 0.5f;
            MarkDirty(mage);
        }

        // ── Weapons ─────────────────────────────────────────────

        private static void CreateWeapons()
        {
            WeaponDataSO ironSword = LoadOrCreate<WeaponDataSO>("Weapons/Weapon_IronSword.asset");
            ironSword.Id = "weapon_iron_sword";
            ironSword.DisplayName = "Kiếm Sắt";
            ironSword.Description = "Vũ khí khởi đầu của Chiến binh, độ bền tốt và sát thương ổn định.";
            ironSword.Category = WeaponCategory.Sword;
            ironSword.Element = ElementType.None;
            ironSword.BaseDamage = 12f;
            ironSword.AttackRange = 1.5f;
            ironSword.AttackSpeed = 1f;
            ironSword.StaminaCost = 5f;
            ironSword.ManaCost = 0f;
            ironSword.BasePrice = 60f;
            MarkDirty(ironSword);

            WeaponDataSO staff = LoadOrCreate<WeaponDataSO>("Weapons/Weapon_ApprenticeStaff.asset");
            staff.Id = "weapon_apprentice_staff";
            staff.DisplayName = "Trượng Tập Sự";
            staff.Description = "Trượng pháp cấp initiates, tiêu hao Mana nhưng tầm xa hơn.";
            staff.Category = WeaponCategory.Staff;
            staff.Element = ElementType.None;
            staff.BaseDamage = 8f;
            staff.AttackRange = 2f;
            staff.AttackSpeed = 0.8f;
            staff.StaminaCost = 0f;
            staff.ManaCost = 5f;
            staff.BasePrice = 80f;
            MarkDirty(staff);
        }

        // ── Enemies ─────────────────────────────────────────────

        private static void CreateEnemies()
        {
            EnemyDataSO slime = LoadOrCreate<EnemyDataSO>("Enemies/Enemy_Slime.asset");
            SetSerialized(slime, "_enemyId", "enemy_slime");
            SetSerialized(slime, "_enemyName", "Slime");
            slime.MaxHP = 30f;
            slime.AttackDamage = 8f;
            slime.MoveSpeed = 2.5f;
            slime.DetectRange = 6f;
            slime.AttackRange = 1.5f;
            slime.AttackCooldown = 1.5f;
            slime.GoldDrop = 5;
            slime.ExpDrop = 10;
            MarkDirty(slime);

            EnemyDataSO skeleton = LoadOrCreate<EnemyDataSO>("Enemies/Enemy_Skeleton.asset");
            SetSerialized(skeleton, "_enemyId", "enemy_skeleton");
            SetSerialized(skeleton, "_enemyName", "Cốt Lăng");
            skeleton.MaxHP = 60f;
            skeleton.AttackDamage = 15f;
            skeleton.MoveSpeed = 3f;
            skeleton.DetectRange = 8f;
            skeleton.AttackRange = 1.5f;
            skeleton.AttackCooldown = 2f;
            skeleton.GoldDrop = 12;
            skeleton.ExpDrop = 25;
            MarkDirty(skeleton);
        }

        // ── Items ───────────────────────────────────────────────

        private static void CreateItems()
        {
            ItemDataSO coin = LoadOrCreate<ItemDataSO>("Items/Item_AncientCoin.asset");
            coin.Id = "item_ancient_coin";
            coin.DisplayName = "Tiền Cổ";
            coin.Description = "Vestige tuấn purtat-c, không mang lời nguyền.";
            coin.Icon = null;
            coin.Category = ItemCategory.Relic;
            coin.GridSize = Vector2Int.one;
            coin.MaxStack = 99;
            coin.BaseValue = 25f;
            coin.CurseType = ItemCurseType.None;
            MarkDirty(coin);

            ItemDataSO idol = LoadOrCreate<ItemDataSO>("Items/Item_CursedIdol.asset");
            idol.Id = "item_cursed_idol";
            idol.DisplayName = "Tùy Lời Nguyền";
            idol.Description = "Phá hủy ô cạnh khi rời hầm ngục.";
            idol.Icon = null;
            idol.Category = ItemCategory.Relic;
            idol.GridSize = Vector2Int.one;
            idol.MaxStack = 1;
            idol.BaseValue = 60f;
            idol.CurseType = ItemCurseType.DestroyAdjacent;
            MarkDirty(idol);

            ItemDataSO potion = LoadOrCreate<ItemDataSO>("Items/Item_HealthPotion.asset");
            potion.Id = "item_health_potion";
            potion.DisplayName = "Bình Máu";
            potion.Description = "Hồi phục một phần HP khi sử dụng.";
            potion.Icon = null;
            potion.Category = ItemCategory.Potion;
            potion.GridSize = Vector2Int.one;
            potion.MaxStack = 10;
            potion.BaseValue = 40f;
            potion.CurseType = ItemCurseType.None;
            MarkDirty(potion);
        }
    }
}
#endif