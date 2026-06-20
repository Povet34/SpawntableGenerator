using UnityEngine;

namespace SpawnSystem.Monsters
{
    public enum MonsterSizeClass { Small, Medium, Large }   // §11.4 크기별 navmesh 타입과 연결

    [System.Flags]
    public enum MonsterTag
    {
        None = 0,
        Melee = 1 << 0,   // 근접
        Ranged = 1 << 1,  // 원거리
        Elite = 1 << 2,   // 엘리트
        Swarm = 1 << 3,   // 스웜
    }

    /// <summary>
    /// 몬스터 아키타입 "이 몬스터는 무엇인가". 정체성 + 몸체 + 이동 능력 + (최소)전투 stub + 행동 프로필 참조.
    /// 전투/활력은 후속 CombatProfile 로 분리 예정(현재는 린하게). 설계 §2/§11.1/§12 참조.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDef", menuName = "SpawnSystem/Monster Def")]
    public class MonsterDef : ScriptableObject
    {
        [Header("정체성")]
        public string id;
        public string displayName;
        public MonsterTag tags = MonsterTag.Melee;

        [Header("몸체")]
        public PrimitiveType bodyPrimitive = PrimitiveType.Capsule;
        public MonsterSizeClass sizeClass = MonsterSizeClass.Medium;
        [Tooltip("월드 지름(대략). 에이전트 반경 = 절반")]
        public float scale = 1f;
        public Color color = Color.red;

        [Header("이동 능력 (§12.3)")]
        public float moveSpeed = 4f;
        public float turnSpeedDeg = 360f;
        [Tooltip("바라보지 않고 옆으로 갈 수 있는가(좌우걸음)")]
        public bool canStrafe = false;
        [Tooltip("뒤로 빠질 수 있는가(뒷걸음)")]
        public bool canBackstep = false;

        [Header("활력/방어/공격")]
        public float maxHP = 10f;
        [Tooltip("선호 교전 거리 (min, max) — 이동이 소비")]
        public Vector2 preferredRange = new Vector2(0f, 2f);
        [Tooltip("방어 성격(장갑/약점) — 재사용 SO")]
        public DefenseProfile defense;
        [Tooltip("공격 키트(공격 여러 개 가능) — 재사용 SO")]
        public AttackProfile attack;

        [Header("특수 능력 (Leap/Burrow/Summon)")]
        public MonsterAbility abilities = MonsterAbility.None;

        [Header("행동 (이동 성격)")]
        public MovementProfile movement;

        [Header("지능 (§12.3)")]
        [Tooltip("체력 낮으면 도망(이 플래그가 있을 때만)")]
        public bool canFlee = false;
        [Range(0f, 1f)] public float fleeHpRatio = 0.25f;
    }
}
