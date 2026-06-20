using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 몬스터 방어 성격(재사용 SO). 장갑 등급 + 취약 데미지 타입 + 약점 규칙.
    /// "둔한 중장갑" 키트를 여러 몬스터가 공유. 실제 데미지 적용은 후속 전투 단계, 규칙은
    /// <see cref="DamageResolver"/> 에서 순수 계산.
    /// </summary>
    [CreateAssetMenu(fileName = "DefenseProfile", menuName = "SpawnSystem/Defense Profile")]
    public class DefenseProfile : ScriptableObject
    {
        public MonsterArmor armor = MonsterArmor.None;

        [Tooltip("Heavy 일 때 이 데미지 타입만 데미지가 들어감")]
        public DamageType vulnerableTo = DamageType.Normal | DamageType.Piercing | DamageType.Explosive;

        [Tooltip("약점 타격 시 데미지 배수")]
        public float weakPointMultiplier = 2f;

        [Tooltip("약점 타격만 데미지가 들어감(가장 단단)")]
        public bool requiresWeakPoint = false;
    }
}
