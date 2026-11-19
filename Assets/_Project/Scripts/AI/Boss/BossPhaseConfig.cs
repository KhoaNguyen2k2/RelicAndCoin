using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelicAndCoin.AI.Boss
{
    /// <summary>
    /// Cau hinh mot giai doan cua boss: nguong % MAP khoi dong pha moi,
    /// he so tang toc do / sat thuong, va danh sach ky nang dac biet cua pha do.
    /// </summary>
    [Serializable]
    public class BossPhaseConfig
    {
        [Header("Kich hoat pha (ty le HP con lai, 1 = bat dau, 0.6 = con 60%)")]
        [Range(0f, 1f)] public float HealthThresholdPercent = 1f;

        [Header("He so cua pha")]
        [Min(0f)] public float SpeedMultiplier    = 1f;
        [Min(0f)] public float DamageMultiplier   = 1f;

        [Header("Ky nang dac biet cua pha")]
        public string[] SpecialAbilities = Array.Empty<string>();
    }
}
