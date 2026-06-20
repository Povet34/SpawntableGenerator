using System;
using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 스폰 테이블의 한 항목(설계 §2). 어떤 몬스터를 몇 마리(군집), 가중치·비용·최소난이도로.
    /// 디렉터가 예산을 cost 로 소비하며 weight 로 가중 선택한다.
    /// </summary>
    [Serializable]
    public class SpawnEntry
    {
        public MonsterDef monster;
        [Min(0f)] public float weight = 1f;
        [Min(0f)] public float cost = 1f;
        [Tooltip("한 번에 스폰할 군집 크기(min,max)")]
        public Vector2Int groupSize = new Vector2Int(3, 6);
        [Tooltip("이 난이도 이상에서만 등장")]
        public float minDifficulty = 0f;
        public MonsterTag tags;
    }
}
