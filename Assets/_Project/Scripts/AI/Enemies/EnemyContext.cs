using System.Collections.Generic;
using RelicAndCoin.Core.StateMachine;
using RelicAndCoin.Core.Stats;
using RelicAndCoin.Combat.StatusEffects;
using RelicAndCoin.Dungeon.Pathfinding;
using UnityEngine;

namespace RelicAndCoin.AI.Enemies
{
    /// <summary>
    /// Dùng chung cho mọi State của quái vật: các thành phần gameplay +
    /// StateMachine để state có thể yêu cầu chuyển trạng thái.
    /// Pure C# apart from Rigidbody2D (Unity engine handle).
    /// </summary>
    public sealed class EnemyContext
    {
        public Rigidbody2D Rigidbody     { get; }
        public CharacterStatsModel Stats { get; }
        public StatusEffectSystem StatusEffects { get; }
        public EnemyDataSO Data          { get; }
        public StateMachine StateMachine { get; }

        public Transform PlayerTransform { get; set; }
        public Vector2   TargetPosition  { get; set; }
        public float     DistanceToPlayer { get; set; }
        public List<Vector2Int> CurrentPath { get; set; }
        public int       PathIndex       { get; set; }

        // ── Chỉ số AI đã tính toán theo độ khó ─────────────────────────
        public float PatrolSpeed        { get; set; }
        public float ChaseSpeed         { get; set; }
        public float DetectRange        { get; set; }
        public float AttackCooldown     { get; set; }
        public bool  CanDodgeAndPunish  { get; set; }

        // ── Nhận biết trạng thái Player (được cập nhật từ EnemyController) ──
        public bool  IsPlayerAttacking  { get; set; }
        public bool  IsPlayerFatigued   { get; set; }

        // ── Trạng thái né đòn tạm thời ─────────────────────────────────
        public float DodgeTimer         { get; set; }
        public Vector2 DodgeDirection   { get; set; }

        public EnemyContext(Rigidbody2D rigidbody, CharacterStatsModel stats,
            StatusEffectSystem statusEffects, EnemyDataSO data, StateMachine stateMachine)
        {
            Rigidbody      = rigidbody;
            Stats          = stats;
            StatusEffects  = statusEffects;
            Data           = data;
            StateMachine   = stateMachine;
            CurrentPath    = new List<Vector2Int>();

            PatrolSpeed    = data.MoveSpeed;
            ChaseSpeed     = data.MoveSpeed;
            DetectRange    = data.DetectRange;
            AttackCooldown = data.AttackCooldown;
        }
    }
}
