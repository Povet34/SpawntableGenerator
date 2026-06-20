namespace SpawnSystem.Monsters
{
    /// <summary>군집 인지 상태(설계 §5). 상태는 군집 단위 공유.</summary>
    public enum PackState { Patrol, Alert, Engage }

    /// <summary>한 틱의 감각 입력(군집 단위로 집계됨).</summary>
    public struct PackSenses
    {
        public bool SightContact;    // 한 멤버라도 플레이어를 봄 → 발각(공유)
        public bool NoiseHeard;      // 소음 감지(시야를 돌리는 트리거)
        public float TimeInState;    // 현재 상태에 머문 시간
        public float TimeSinceSight; // 마지막 시야 이후 경과
    }

    /// <summary>상태 전이 임계값.</summary>
    public struct PackPerception
    {
        public float investigateTimeout; // 경계: 조사 실패로 순찰 복귀까지
        public float loseSightTime;      // 교전: 시야 상실 인정까지

        public static PackPerception Default => new PackPerception { investigateTimeout = 5f, loseSightTime = 3f };
    }

    /// <summary>
    /// 군집 인지 상태머신의 순수 전이 로직(설계 §5 표). NavMesh/씬 무관 → EditMode 테스트.
    /// </summary>
    public static class PackFsm
    {
        public static PackState Next(PackState current, in PackSenses s, in PackPerception p)
        {
            switch (current)
            {
                case PackState.Patrol:
                    if (s.SightContact) return PackState.Engage; // 직접 시야 → 교전
                    if (s.NoiseHeard) return PackState.Alert;    // 소음 → 경계(조사)
                    return PackState.Patrol;

                case PackState.Alert:
                    if (s.SightContact) return PackState.Engage;             // 시야로 확인 → 교전
                    if (s.TimeInState >= p.investigateTimeout) return PackState.Patrol; // 조사 실패 → 순찰
                    return PackState.Alert;

                case PackState.Engage:
                    if (s.TimeSinceSight >= p.loseSightTime) return PackState.Alert; // 시야 상실 → 마지막 위치 수색
                    return PackState.Engage;

                default:
                    return current;
            }
        }
    }
}
