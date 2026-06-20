using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 스폰 배치 보조(순수). 플레이어의 이동 방향·속도로 미래 위치를 예측해 그 근처에 스폰 →
    /// 군집이 플레이어를 자주 만나게 한다. 이 예측은 인지(FSM 시야 발견)와 무관한 '배치' 용도. XZ 평면.
    /// </summary>
    public static class SpawnPlacement
    {
        public static Vector3 Predict(Vector3 playerPos, Vector3 playerVel, float leadTime)
        {
            Vector3 p = new Vector3(playerPos.x, 0f, playerPos.z);
            Vector3 v = new Vector3(playerVel.x, 0f, playerVel.z);
            return p + v * leadTime;
        }
    }
}
