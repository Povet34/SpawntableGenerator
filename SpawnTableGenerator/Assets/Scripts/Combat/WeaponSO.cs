using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>무기 데이터 공통 베이스. 근접/원거리 각 SO가 상속.</summary>
    public abstract class WeaponSO : ScriptableObject
    {
        public float damage = 30f;
        public DamageType damageType = DamageType.Normal;
        public float cooldown = 0.45f;
    }
}
