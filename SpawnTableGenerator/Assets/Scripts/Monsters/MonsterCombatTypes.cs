using System;

namespace SpawnSystem.Monsters
{
    /// <summary>데미지 타입(설계 Monsters.md §5). 장갑 취약성 판정에 사용.</summary>
    [Flags]
    public enum DamageType
    {
        None = 0,
        Normal = 1 << 0,
        Piercing = 1 << 1,   // 관통
        Explosive = 1 << 2,  // 폭발
        WeakPoint = 1 << 3,  // 약점 전용
    }

    /// <summary>장갑 등급. Heavy 는 지정 타입(또는 약점)만 데미지.</summary>
    public enum MonsterArmor { None, Light, Heavy }

    /// <summary>특수 능력(실행은 후속 단계). Monsters.md 의 점프/잠복/소환.</summary>
    [Flags]
    public enum MonsterAbility
    {
        None = 0,
        Leap = 1 << 0,    // 도약 접근 공격
        Burrow = 1 << 1,  // 잠복 후 플레이어 근처 재출현
        Summon = 1 << 2,  // 하위 스폰 테이블 소환
    }

    /// <summary>공격 종류.</summary>
    public enum AttackKind { Melee, Projectile, AoE, Sustained, Leap }

    /// <summary>몬스터 공격 1종의 데이터. 실행(투사체/AoE/데미지 적용)은 후속 전투 단계.</summary>
    [Serializable]
    public struct AttackDef
    {
        public string name;
        public AttackKind kind;
        public float damage;
        public DamageType damageType;
        public float range;
        public float cooldown;
        public float telegraph;       // 예고 시간
        public float aoeRadius;       // AoE 반경(해당 시)
        public float projectileSpeed; // 투사체 속도(해당 시)
    }
}
