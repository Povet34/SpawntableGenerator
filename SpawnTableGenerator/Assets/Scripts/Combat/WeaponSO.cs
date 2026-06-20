using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    public enum WeaponKind { Melee, Ranged }

    /// <summary>
    /// 무기 데이터 SO. 플레이어/몬스터 공격 설정. 근접=부채꼴 데미지+Hovl 슬래시 VFX,
    /// 원거리=레이저 투사체(LineRenderer, 고속).
    /// </summary>
    [CreateAssetMenu(menuName = "SpawnSystem/Combat/Weapon", fileName = "WP_New")]
    public class WeaponSO : ScriptableObject
    {
        public WeaponKind kind = WeaponKind.Melee;
        public float damage = 30f;
        public DamageType damageType = DamageType.Normal;
        public float cooldown = 0.45f;

        [Header("Melee")]
        public float meleeRadius = 4.5f;
        [Range(30f, 180f)] public float meleeArcDeg = 120f;
        public GameObject slashVfxPrefab; // Hovl Studio slash prefab

        [Header("Ranged")]
        public float projectileSpeed = 45f;
        public float projectileRange = 35f;
        [ColorUsage(true, true)] public Color projectileColor = new Color(4f, 0.6f, 0.4f, 1f); // HDR
    }
}
