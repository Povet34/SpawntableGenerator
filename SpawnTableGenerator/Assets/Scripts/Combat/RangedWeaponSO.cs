using UnityEngine;

namespace SpawnSystem.Combat
{
    [CreateAssetMenu(menuName = "SpawnSystem/Combat/RangedWeapon", fileName = "WP_Ranged")]
    public class RangedWeaponSO : WeaponSO
    {
        public float projectileSpeed = 75f;
        public float projectileRange = 40f;
        [ColorUsage(true, true)] public Color projectileColor = new Color(4f, 0.6f, 0.4f, 1f);

        [Tooltip("발사할 레이저 프리팹. 비우면 코드로 즉석 생성한다(라이트는 런타임 부착).")]
        public GameObject projectilePrefab;
    }
}
