using UnityEngine;

namespace SpawnSystem.Combat
{
    [CreateAssetMenu(menuName = "SpawnSystem/Combat/MeleeWeapon", fileName = "WP_Melee")]
    public class MeleeWeaponSO : WeaponSO
    {
        public float radius = 4.5f;
        [Range(30f, 180f)] public float arcDeg = 120f;
        public GameObject slashVfxPrefab;
    }
}
