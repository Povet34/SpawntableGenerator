using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 몬스터의 '행동 성격'(어떻게 움직이는가). 여러 몬스터가 공유할 수 있는 재사용 ScriptableObject.
    /// 설계 §12(시야 회피 기반 유틸리티 스코어링)의 노브들을 담는 그릇 — 실제 스코어링 구현은 후속(A).
    /// </summary>
    [CreateAssetMenu(fileName = "MovementProfile", menuName = "SpawnSystem/Movement Profile")]
    public class MovementProfile : ScriptableObject
    {
        [Header("시야 인지 (§12.2)")]
        [Tooltip("플레이어 시야 콘 각도(도)")]
        public float viewConeAngle = 70f;
        [Tooltip("시야 콘 사거리")]
        public float viewConeRange = 18f;
        [Tooltip("직전 공격 방향을 의식하는 시간(초). 시간이 지나면 감쇠")]
        public float lastAttackMemory = 2.5f;

        [Header("반응 둔감 (§12.1 — 즉각 반응 금지)")]
        [Tooltip("이 압박(0~1) 이상이어야 회피를 '고려'한다")]
        [Range(0f, 1f)] public float reactionThreshold = 0.35f;
        [Tooltip("재결정 주기(초) min~max. 길수록 느긋하게 반응")]
        public Vector2 repositionInterval = new Vector2(0.4f, 1.2f);
        [Tooltip("고려했을 때 실제로 움직일 확률")]
        [Range(0f, 1f)] public float actChance = 0.6f;

        [Header("후보 목적지 스코어 가중치 (§12.4)")]
        public float wViewAvoid = 1.5f;        // 시야 회피 (핵심 동인)
        public float wPreferredDist = 1f;      // 선호 교전 거리 유지
        public float wNeighborSpacing = 0.6f;  // 몬스터 간 소프트 간격
        public float wInertia = 0.4f;          // 현재 위치 유지(떨림 억제)

        [Header("행동 비용/패널티 (§12.3)")]
        public float costTurnThenMove = 1f;    // 바라보고 전진(둔함)
        public float costStrafe = 0.3f;        // 좌우걸음(능력 있을 때)
        public float costBackstep = 0.6f;      // 뒷걸음(능력 있을 때)
    }
}
