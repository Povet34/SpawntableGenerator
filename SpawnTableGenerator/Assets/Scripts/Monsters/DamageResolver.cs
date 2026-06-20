namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 장갑/약점 데미지 규칙의 순수 로직(설계 Monsters.md §5). NavMesh/씬 무관 → EditMode 테스트.
    /// 전투 실행 전이라도 "중장갑은 특정 공격만 데미지" 규칙을 데이터-레벨로 고정한다.
    /// </summary>
    public static class DamageResolver
    {
        /// <summary>이 데미지가 방어에 의해 완전히 막히는가(데미지 0).</summary>
        public static bool IsImmune(DefenseProfile defense, DamageType incoming, bool hitWeakPoint)
        {
            if (defense == null)
                return false;
            if (hitWeakPoint)
                return false;                       // 약점 타격은 항상 통함
            if (defense.armor != MonsterArmor.Heavy)
                return false;                       // 비중장갑은 다 통함
            if (defense.requiresWeakPoint)
                return true;                        // 약점만 데미지 → 몸통은 면역
            return (defense.vulnerableTo & incoming) == 0; // 취약 타입 외 면역
        }

        /// <summary>최종 데미지 배수(면역=0, 약점=multiplier, 그 외=1).</summary>
        public static float Multiplier(DefenseProfile defense, DamageType incoming, bool hitWeakPoint)
        {
            if (IsImmune(defense, incoming, hitWeakPoint))
                return 0f;
            if (hitWeakPoint && defense != null)
                return defense.weakPointMultiplier;
            return 1f;
        }
    }
}
