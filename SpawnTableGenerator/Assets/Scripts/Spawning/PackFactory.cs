using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// MonsterDef 로부터 군집(가상 앵커 + 멤버 N마리)을 만드는 재사용 팩토리. 디렉터/스포너가 공유.
    /// 캡슐 멤버는 pool 로 재사용(설계 §3); 그 외 프리미티브(예: 큰 큐브)는 비풀링 생성. 멤버는
    /// NavMeshAgent.Move(AgentMover)로 navmesh 구속, 길찾기는 앵커만(설계 §4, §12). 멤버는 Health 보유.
    /// </summary>
    public static class PackFactory
    {
        // navmesh 가 반경 0.5 로 베이크돼 있어, 덩치 큰 몹도 에이전트 반경은 캡(시각 크기와 분리).
        const float MaxAgentRadius = 0.6f;

        public static MonsterPack BuildFromDef(MonsterDef def, int count, Vector3 center, Transform player, Transform parent, MonsterPool pool = null)
        {
            float diameter = def != null ? Mathf.Max(0.2f, def.scale) : 1f;
            Color color = def != null ? def.color : Color.red;
            float speed = def != null ? def.moveSpeed : 4f;
            Vector2 preferredRange = def != null ? def.preferredRange : new Vector2(1.5f, 4f);
            MovementProfile profile = def != null ? def.movement : null;
            PrimitiveType body = def != null ? def.bodyPrimitive : PrimitiveType.Capsule;
            float maxHP = def != null ? def.maxHP : 10f;
            DefenseProfile defense = def != null ? def.defense : null;
            AttackProfile attack = def != null ? def.attack : null;

            return Build(parent, center, count, diameter, color, speed, preferredRange, profile, player, pool, body, maxHP, defense, attack: attack);
        }

        public static MonsterPack Build(
            Transform parent, Vector3 center, int count,
            float memberDiameter, Color color, float moveSpeed, Vector2 preferredRange,
            MovementProfile profile, Transform player, MonsterPool pool = null,
            PrimitiveType bodyPrimitive = PrimitiveType.Capsule,
            float maxHP = 10f, DefenseProfile defense = null,
            float spawnRadius = 3f, float anchorSpeed = 3.5f,
            AttackProfile attack = null)
        {
            Vector3 anchorPos = Snap(center, 12f);

            var anchorGo = new GameObject("PackAnchor");
            if (parent != null) anchorGo.transform.SetParent(parent, false);
            anchorGo.transform.position = anchorPos;
            var agent = anchorGo.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.speed = anchorSpeed;
            agent.acceleration = 30f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 1f;

            var packGo = new GameObject("MonsterPack");
            if (parent != null) packGo.transform.SetParent(parent, false);
            var pack = packGo.AddComponent<MonsterPack>();
            pack.anchor = anchorGo.transform;
            pack.anchorAgent = agent;
            pack.player = player;
            pack.profile = profile;
            pack.memberMoveSpeed = moveSpeed;
            pack.preferredRange = preferredRange;
            pack.MemberReleaser = pool != null ? (System.Action<GameObject>)pool.Release : null;

            float ring = Mathf.Max(spawnRadius, memberDiameter * 1.2f);
            Material mat = null;
            for (int i = 0; i < count; i++)
            {
                float ang = i / Mathf.Max(1f, count) * Mathf.PI * 2f;
                Vector3 mpos = anchorPos + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * ring;

                Monster m;
                if (pool != null && bodyPrimitive == PrimitiveType.Capsule)
                {
                    var go = pool.Get(mpos, packGo.transform, memberDiameter, color, moveSpeed, maxHP, defense);
                    go.name = $"Monster_{i:00}";
                    m = go.GetComponent<Monster>();
                }
                else
                {
                    if (mat == null) mat = MakeMaterial(color);
                    m = CreateMemberNonPooled(packGo.transform, mpos, i, bodyPrimitive, memberDiameter, moveSpeed, mat, maxHP, defense);
                }
                pack.RegisterMember(m);

                // 공격 프로필이 있으면 MonsterAttack 부착 (풀 재사용 시 중복 방지)
                var existingMa = m.gameObject.GetComponent<Combat.MonsterAttack>();
                if (attack != null && attack.attacks != null && attack.attacks.Length > 0)
                {
                    var ma = existingMa != null ? existingMa : m.gameObject.AddComponent<Combat.MonsterAttack>();
                    ma.Init(attack, player);
                }
                else if (existingMa != null)
                {
                    existingMa.enabled = false;
                }
            }
            return pack;
        }

        static Monster CreateMemberNonPooled(Transform parent, Vector3 center, int index, PrimitiveType primitive, float diameter, float moveSpeed, Material mat, float maxHP, DefenseProfile defense)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = $"Monster_{index:00}";
            go.transform.SetParent(parent, true);

            float halfHeight = HalfHeight(primitive, diameter);
            Vector3 p = Snap(center, 4f);
            p.y = halfHeight;
            go.transform.position = p;
            go.transform.localScale = Vector3.one * diameter;

            var r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;

            var m = go.AddComponent<Monster>();
            m.SetGroundY(halfHeight); // XZ 평면 고정

            var hp = go.AddComponent<Health>();
            hp.Init(maxHP, defense);
            m.Health = hp;

            var ag = go.AddComponent<NavMeshAgent>();
            // NavMeshAgent 치수는 transform.scale(=diameter)로 곱해지므로 로컬값으로 환산(÷diameter).
            ag.radius = Mathf.Min(diameter * 0.5f, MaxAgentRadius) / diameter;
            ag.height = primitive == PrimitiveType.Capsule ? 2f : 1f;
            ag.baseOffset = halfHeight / diameter;
            ag.speed = moveSpeed;
            ag.acceleration = 9999f;
            ag.angularSpeed = 0f;
            ag.updateRotation = false;
            ag.autoBraking = false;
            ag.avoidancePriority = 60;
            m.Mover = new AgentMover(ag);
            return m;
        }

        static float HalfHeight(PrimitiveType primitive, float diameter)
            => primitive == PrimitiveType.Capsule ? diameter : diameter * 0.5f;

        static Vector3 Snap(Vector3 p, float radius)
            => NavMesh.SamplePosition(p, out var hit, radius, NavMesh.AllAreas) ? hit.position : p;

        static Material MakeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader) { name = "MonsterMat", color = color };
        }
    }
}
