using UnityEngine;

namespace RelicAndCoin.AI.Enemies
{
    /// <summary>
    /// ScriptableObject lưu dữ liệu định danh và chỉ số của một loại quái vật.
    /// Designer điều chỉnh từ Inspector, không cần code lại khi thêm loại quái mới.
    /// </summary>
    [CreateAssetMenu(menuName = "RelicAndCoin/Enemy/Enemy Data", fileName = "EnemyData")]
    public sealed class EnemyDataSO : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [SerializeField] private string _enemyId = "enemy_001";
        [SerializeField] private string _enemyName = "Quái vật";

        [Header("Chỉ số chiến đấu")]
        [Min(0f)] public float MaxHP        = 50f;
        [Min(0f)] public float AttackDamage = 10f;
        [Min(0f)] public float MoveSpeed    = 3f;

        [Header("Tầm hoạt động")]
        [Min(0f)] public float DetectRange   = 8f;
        [Min(0f)] public float AttackRange   = 1.5f;
        [Min(0f)] public float AttackCooldown = 1.5f;

        [Header("Phần thưởng khi đánh bại")]
        [Min(0)] public int GoldDrop  = 5;
        [Min(0)] public int ExpDrop   = 10;

        public string EnemyId   => _enemyId;
        public string EnemyName => _enemyName;
    }
}
