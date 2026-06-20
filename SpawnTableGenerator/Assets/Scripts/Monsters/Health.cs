using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 체력/데미지. 장갑·약점 규칙은 <see cref="DamageResolver"/>(설계 Monsters.md §5)에 위임.
    /// 풀 재사용을 위해 Init 로 재초기화 가능.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public float maxHP = 10f;
        public DefenseProfile defense;

        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;

        void Awake()
        {
            if (Current <= 0f) Current = maxHP;
        }

        public void Init(float max, DefenseProfile def)
        {
            maxHP = max;
            defense = def;
            Current = max;
        }

        /// <summary>데미지 적용. 장갑 면역/약점 배수 반영. 실제로 입힌 데미지를 반환.</summary>
        public float TakeDamage(float amount, DamageType type, bool hitWeakPoint = false)
        {
            if (IsDead || amount <= 0f) return 0f;
            float dealt = amount * DamageResolver.Multiplier(defense, type, hitWeakPoint);
            Current = Mathf.Max(0f, Current - dealt);
            return dealt;
        }
    }
}
