using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// MonsterDef 로부터 군집(가상 앵커 + 멤버 N마리)을 만드는 재사용 팩토리. 디렉터/스포너가 공유.
    /// pool 을 주면 멤버를 풀에서 재사용(설계 §3), 없으면 즉석 생성. 멤버는 NavMeshAgent.Move(AgentMover)로
    /// navmesh 구속 + 회피, 길찾기는 앵커만(설계 §4, §12).
    /// </summary>
    public static class PackFactory
    {
        public static MonsterPack BuildFromDef(MonsterDef def, int count, Vector3 center, Transform player, Transform parent, MonsterPool pool = null)
        {
            float diameter = def != null ? Mathf.Max(0.2f, def.scale) : 1f;
            Color color = def != null ? def.color : Color.red;
            float speed = def != null ? def.moveSpeed : 4f;
            Vector2 preferredRange = def != null ? def.preferredRange : new Vector2(1.5f, 4f);
            MovementProfile profile = def != null ? def.movement : null;

            return Build(parent, center, count, diameter, color, speed, preferredRange, profile, player, pool);
        }

        public static MonsterPack Build(
            Transform parent, Vector3 center, int count,
            float memberDiameter, Color color, float moveSpeed, Vector2 preferredRange,
            MovementProfile profile, Transform player, MonsterPool pool = null,
            float spawnRadius = 3f, float anchorSpeed = 3.5f)
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

            Material mat = pool == null ? MakeMaterial(color) : null;
            for (int i = 0; i < count; i++)
            {
                float ang = i / Mathf.Max(1f, count) * Mathf.PI * 2f;
                Vector3 mpos = anchorPos + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * spawnRadius;

                Monster m;
                if (pool != null)
                {
                    var go = pool.Get(mpos, packGo.transform, memberDiameter, color, moveSpeed);
                    go.name = $"Monster_{i:00}";
                    m = go.GetComponent<Monster>();
                }
                else
                {
                    m = CreateMemberNonPooled(packGo.transform, mpos, i, memberDiameter, color, moveSpeed, mat);
                }
                pack.RegisterMember(m);
            }
            return pack;
        }

        static Monster CreateMemberNonPooled(Transform parent, Vector3 center, int index, float diameter, Color color, float moveSpeed, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = $"Monster_{index:00}";
            go.transform.SetParent(parent, true);
            Vector3 p = Snap(center, 4f);
            p.y = diameter;
            go.transform.position = p;
            go.transform.localScale = Vector3.one * diameter;

            var r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;

            var m = go.AddComponent<Monster>();
            var ag = go.AddComponent<NavMeshAgent>();
            ag.radius = diameter * 0.5f;
            ag.height = diameter * 2f;
            ag.baseOffset = diameter;
            ag.speed = moveSpeed;
            ag.acceleration = 9999f;
            ag.angularSpeed = 0f;
            ag.updateRotation = false;
            ag.autoBraking = false;
            ag.avoidancePriority = 60;
            m.Mover = new AgentMover(ag);
            return m;
        }

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
