using UnityEngine;

namespace SpawnSystem.Player
{
    /// <summary>
    /// 클릭 이동의 '순수' 기하 로직. NavMesh / Input / MonoBehaviour 에 의존하지 않으므로
    /// EditMode 단위 테스트로 빠르게 검증할 수 있다(움직임 TDD의 핵심 대상).
    /// </summary>
    public static class MovementResolver
    {
        /// <summary>
        /// 카메라 클릭 레이가 맞은 표면(point, normal)을 NavMesh 샘플링에 넣기 좋은 '클릭 타깃'으로 변환한다.
        ///
        /// - 수직 벽면(법선이 거의 수평)을 찍은 경우: 그 면의 바닥 지점으로 내리고, 법선 바깥쪽
        ///   (걸을 수 있는 쪽)으로 hugOffset 만큼 밀어 그 벽면에 밀착하도록 한다.
        /// - 그 외(바닥/윗면, 법선이 거의 수직): 표면점을 그대로 사용한다.
        /// </summary>
        /// <param name="hitPoint">Physics 레이가 맞은 월드 표면점.</param>
        /// <param name="hitNormal">맞은 표면의 법선.</param>
        /// <param name="groundHeight">바닥 높이(월드 y).</param>
        /// <param name="hugOffset">벽면에서 밀어낼 거리(보통 에이전트 반경 + 약간).</param>
        /// <param name="verticalFaceThreshold">|normal.y| 가 이 값보다 작으면 '수직 벽면'으로 판정.</param>
        public static Vector3 ResolveClickTarget(
            Vector3 hitPoint,
            Vector3 hitNormal,
            float groundHeight,
            float hugOffset,
            float verticalFaceThreshold = 0.5f)
        {
            if (IsVerticalFace(hitNormal, verticalFaceThreshold))
            {
                Vector3 outward = new Vector3(hitNormal.x, 0f, hitNormal.z).normalized;
                return new Vector3(hitPoint.x, groundHeight, hitPoint.z) + outward * hugOffset;
            }
            return hitPoint;
        }

        /// <summary>법선이 거의 수평이면(=수직 벽면) true.</summary>
        public static bool IsVerticalFace(Vector3 normal, float verticalFaceThreshold = 0.5f)
        {
            return Mathf.Abs(normal.y) < verticalFaceThreshold;
        }
    }
}
