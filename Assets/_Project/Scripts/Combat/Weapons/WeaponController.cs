using System.Collections;
using System.Collections.Generic;
using RelicAndCoin.Core.Stats;
using UnityEngine;

namespace RelicAndCoin.Combat.Weapons
{
    /// <summary>
    /// MonoBehaviour mỏng gắn lên Player: trang bị WeaponDataSO, kiểm tra thời gian hồi đòn,
    /// tiêu hao Stamina/Mana qua CharacterStatsController, tính sát thương bằng DamageCalculator,
    /// xoay và kích hoạt WeaponHitbox chém theo hướng nhìn kèm hiệu ứng vệt chém trực quan 0.15s.
    /// </summary>
    [RequireComponent(typeof(CharacterStatsController))]
    public sealed class WeaponController : MonoBehaviour
    {
        [Header("Vũ khí")]
        [SerializeField] private WeaponDataSO _weaponData;

        [Header("Tham chiếu")]
        [SerializeField] private WeaponHitbox _hitbox;
        [SerializeField] private Transform    _hitOrigin; // điểm lấy hướng đòn (mặc định transform của chính weapon)

        private CharacterStatsController _stats;
        private readonly DamageCalculator _calculator = new();
        private float _nextAttackTime;

        // Visual Slash Renderer
        private SpriteRenderer _slashRenderer;
        private BoxCollider2D  _hitboxCollider;
        private readonly HashSet<IDamageable> _hitThisAttack = new();

        private void Awake()
        {
            _stats = GetComponent<CharacterStatsController>();
            if (_hitOrigin == null) _hitOrigin = transform;

            SetupHitbox();
        }

        private void SetupHitbox()
        {
            if (_hitbox == null)
            {
                var found = GetComponentInChildren<WeaponHitbox>();
                if (found != null)
                    _hitbox = found;
                else
                {
                    var hitboxGo = new GameObject("WeaponHitbox");
                    hitboxGo.transform.SetParent(transform);
                    hitboxGo.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                    _hitboxCollider = hitboxGo.AddComponent<BoxCollider2D>();
                    _hitboxCollider.isTrigger = true;
                    _hitboxCollider.size = new Vector2(1.2f, 1.2f);
                    _hitbox = hitboxGo.AddComponent<WeaponHitbox>();
                }
            }

            if (_hitbox != null)
            {
                _hitboxCollider = _hitbox.GetComponent<BoxCollider2D>();
                _hitbox.OnNewHit += OnNewHit;

                // Chuẩn bị SpriteRenderer cho vệt chém trực quan
                _slashRenderer = _hitbox.GetComponent<SpriteRenderer>();
                if (_slashRenderer == null)
                {
                    _slashRenderer = _hitbox.gameObject.AddComponent<SpriteRenderer>();
                }

                _slashRenderer.sprite = CreateSlashSprite();
                _slashRenderer.sortingOrder = 10;
                _slashRenderer.color = new Color(0.9f, 0.95f, 1f, 0f); // Ẩn ban đầu
                _slashRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_hitbox != null)
                _hitbox.OnNewHit -= OnNewHit;
        }

        /// <summary>
        /// Trang bị vũ khí mới từ DroppedItemPickup hoặc kho đồ.
        /// </summary>
        public void EquipWeapon(WeaponDataSO newWeapon)
        {
            if (newWeapon == null) return;
            _weaponData = newWeapon;

            if (_hitboxCollider != null)
            {
                float size = Mathf.Max(1.0f, newWeapon.AttackRange);
                _hitboxCollider.size = new Vector2(size, size);
            }

            Debug.Log($"<color=cyan>[WEAPON]</color> Đã trang bị vũ khí: <b>{newWeapon.DisplayName}</b> (Sát thương: {newWeapon.BaseDamage}, Tốc độ: {newWeapon.AttackSpeed}/s)");
        }

        /// <summary>
        /// Điểm vào tấn công theo hướng mặc định (sang phải).
        /// </summary>
        public bool TryAttack() => TryAttack(transform.right);

        /// <summary>
        /// Tấn công theo hướng xác định (hướng nhìn hoặc hướng click chuột).
        /// </summary>
        public bool TryAttack(Vector2 attackDirection)
        {
            // Kiểm tra an toàn tránh treo / lỗi sau khi reload Scene
            if (_weaponData == null) return false;
            if (_stats == null) return false;
            if (_hitbox == null) return false;
            if (Time.time < _nextAttackTime) return false;
            if (!_stats.TryUseStamina(_weaponData.StaminaCost)) return false;
            if (!_stats.TryUseMana(_weaponData.ManaCost)) return false;

            _nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, _weaponData.AttackSpeed);

            if (attackDirection.sqrMagnitude < 0.001f)
                attackDirection = Vector2.right;
            attackDirection.Normalize();

            // Định vị và xoay Hitbox theo hướng chém
            float attackRange = Mathf.Max(0.8f, _weaponData.AttackRange * 0.7f);
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

            if (_hitbox != null)
            {
                _hitbox.transform.position = _hitOrigin.position + (Vector3)(attackDirection * attackRange);
                _hitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                _hitbox.BeginSwing();
            }

            _hitThisAttack.Clear();

            // Kích hoạt vệt chém trực quan ngắn 0.15s
            StartCoroutine(ShowSlashEffect(0.15f, attackDirection));

            // Quét va chạm ngay lập tức với các mục tiêu trong vùng chém
            PerformAttackOverlap(attackDirection, angle);

            return true;
        }

        private void PerformAttackOverlap(Vector2 attackDirection, float angle)
        {
            Vector2 hitboxPos = _hitbox != null ? (Vector2)_hitbox.transform.position : (Vector2)_hitOrigin.position + attackDirection * 0.8f;
            Vector2 boxSize = _hitboxCollider != null ? _hitboxCollider.size : new Vector2(1.2f, 1.2f);

            var hits = Physics2D.OverlapBoxAll(hitboxPos, boxSize, angle);
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.gameObject == gameObject) continue; // Bỏ qua chính player

                if (hit.TryGetComponent<IDamageable>(out var target))
                {
                    OnNewHit(target);
                }
            }
        }

        private void OnNewHit(IDamageable target)
        {
            if (!target.IsAlive) return;
            if (!_hitThisAttack.Add(target)) return; // Tránh trúng lặp cùng 1 nhát chém

            float targetDefense = GetTargetDefense(target);
            var damage = _calculator.Calculate(
                rng: null,
                attackerDamage: _stats.AttackDamage,
                critChance:   _stats.CritChance,
                weaponBaseDamage: _weaponData.BaseDamage,
                element:   _weaponData.Element,
                targetDefense: targetDefense);

            Vector2 hitDirection = ((Vector2)target.Position - (Vector2)_hitOrigin.position).normalized;
            target.TakeDamage(damage, hitDirection);

            string targetName = (target is MonoBehaviour mb) ? mb.gameObject.name : "Mục tiêu";
            Debug.Log($"<color=red>[CHÉM TRÚNG]</color> Trúng <b>{targetName}</b>! Sát thương: <b>{damage.FinalDamage:F1}</b> {(damage.IsCritical ? "<color=yellow>[CHÍ MẠNG]</color>" : "")}");
        }

        private IEnumerator ShowSlashEffect(float duration, Vector2 dir)
        {
            if (_slashRenderer == null) yield break;

            _slashRenderer.enabled = true;
            float elapsed = 0f;
            Color slashColor = new Color(0.9f, 0.98f, 1f, 0.9f);

            // Bắt đầu phóng to nhẹ và làm mờ dần
            Vector3 startScale = new Vector3(0.7f, 0.5f, 1f);
            Vector3 endScale   = new Vector3(1.3f, 1.1f, 1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _slashRenderer.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                _slashRenderer.color = new Color(slashColor.r, slashColor.g, slashColor.b, Mathf.Lerp(0.9f, 0f, t));
                yield return null;
            }

            _slashRenderer.enabled = false;
        }

        private static Sprite CreateSlashSprite()
        {
            int width = 32;
            int height = 32;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            Color clear = new Color(0, 0, 0, 0);
            Color arc = new Color(1f, 1f, 1f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Vẽ hình vệt chém hình trăng khuyết (crescent arc)
                    float nx = (float)x / width - 0.5f;
                    float ny = (float)y / height - 0.5f;
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);

                    if (dist >= 0.25f && dist <= 0.48f && nx > -0.2f)
                    {
                        float alpha = Mathf.Sin((dist - 0.25f) / 0.23f * Mathf.PI);
                        tex.SetPixel(x, y, new Color(arc.r, arc.g, arc.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }

        private static float GetTargetDefense(IDamageable target)
        {
            var stats = GetStats(target);
            return stats != null ? stats.Defense : 0f;
        }

        private static CharacterStatsController GetStats(IDamageable target)
        {
            var mono = target as MonoBehaviour;
            return mono != null ? mono.GetComponent<CharacterStatsController>() : null;
        }

        // Expose cho các hệ thống khác cần biết weapon hiện tại (UI, tooltip, rune)
        public WeaponDataSO WeaponData => _weaponData;
        public float AttackRange => _weaponData != null ? _weaponData.AttackRange : 0f;
    }
}
