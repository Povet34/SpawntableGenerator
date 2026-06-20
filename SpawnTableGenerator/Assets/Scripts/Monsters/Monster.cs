using System.Collections.Generic;
using UnityEngine;

namespace SpawnSystem.Monsters
{
    /// <summary>
    /// 군집 멤버 1마리. 길찾기는 하지 않고(앵커만 길찾기), 앵커 추종 boids(응집+분리)로만 움직인다.
    /// 실제 이동 한 스텝은 <see cref="MonsterPack"/> 이 중앙에서 호출(StepFollow)하여 순서를 결정한다.
    /// </summary>
    public class Monster : MonoBehaviour
    {
        [System.NonSerialized] public MonsterPack Pack;

        /// <summary>
        /// 한 스텝 추종. 이웃 위치 목록(자기 자신 포함 가능 — 거리 0이라 분리에서 자동 무시됨)을 받아
        /// BoidsSteering 으로 desired velocity 를 구하고 XZ 평면에서 이동한다.
        /// </summary>
        public void StepFollow(Vector3 anchorPos, IReadOnlyList<Vector3> memberPositions, BoidsSettings settings, float dt)
        {
            Vector3 v = BoidsSteering.DesiredVelocity(transform.position, anchorPos, memberPositions, settings);

            Vector3 pos = transform.position;
            pos += v * dt;
            pos.y = transform.position.y; // 높이 고정(평면 이동)
            transform.position = pos;

            Vector3 flat = new Vector3(v.x, 0f, v.z);
            if (flat.sqrMagnitude > 1e-4f)
                transform.forward = flat.normalized;
        }
    }
}
