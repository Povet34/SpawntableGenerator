using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 몬스터 공격 키트(재사용 SO). 공격 여러 개 보유 가능(예: 포대형 = 폭발 + 기관총).
    /// 실행은 후속 전투 단계 — 지금은 데이터만.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackProfile", menuName = "SpawnSystem/Attack Profile")]
    public class AttackProfile : ScriptableObject
    {
        public AttackDef[] attacks = new AttackDef[0];
    }
}
