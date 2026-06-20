using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 플레이어가 한 지점을 '보고 있는 정도'(0~1)를 계산하는 순수 로직. 설계 §12.2.
    /// "바라본다" = 현재 forward 콘 + 직전 공격 방향 콘(시간 감쇠). 모든 계산 XZ 평면.
    /// </summary>
    public static class ViewPressure
    {
        /// <summary>
        /// viewerPos 에서 viewDir 로 보는 콘이 target 을 포착하는 정도(0~1).
        /// 콘 밖이거나 사거리 밖이면 0. 중심+가까울수록 1.
        /// </summary>
        public static float Cone(Vector3 viewerPos, Vector3 viewDir, Vector3 target, float coneAngleDeg, float range)
        {
            Vector3 to = target - viewerPos;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist < 1e-4f)
                return 1f;
            if (range <= 0f || dist > range)
                return 0f;

            Vector3 fwd = new Vector3(viewDir.x, 0f, viewDir.z);
            if (fwd.sqrMagnitude < 1e-6f)
                return 0f;
            fwd.Normalize();

            float ang = Vector3.Angle(fwd, to / dist); // 도
            float half = coneAngleDeg * 0.5f;
            if (half <= 0f || ang > half)
                return 0f;

            float angleFactor = 1f - ang / half;   // 1 중심, 0 가장자리
            float distFactor = 1f - dist / range;  // 1 가까움, 0 사거리 끝
            return Mathf.Clamp01(angleFactor * distFactor);
        }

        /// <summary>
        /// 현재 forward 콘과 직전 공격 방향 콘(나이에 따라 감쇠)을 합쳐 최대 압박(0~1) 반환.
        /// </summary>
        public static float Total(
            Vector3 playerPos, Vector3 playerForward,
            Vector3 lastAttackDir, float lastAttackAge,
            Vector3 target, float coneAngleDeg, float range, float lastAttackMemory)
        {
            float current = Cone(playerPos, playerForward, target, coneAngleDeg, range);

            float decay = lastAttackMemory > 1e-4f ? Mathf.Clamp01(1f - lastAttackAge / lastAttackMemory) : 0f;
            float lastAttack = (decay > 0f && lastAttackDir.sqrMagnitude > 1e-6f)
                ? Cone(playerPos, lastAttackDir, target, coneAngleDeg, range) * decay
                : 0f;

            return Mathf.Max(current, lastAttack);
        }
    }
}
