using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Spawning
{
    /// <summary>
    /// 몬스터 멤버 GameObject 풀(설계 §3). 디스폰 시 파괴하지 않고 비활성·재사용 → 생성/GC 비용 회피
    /// (300마리 규모 대비). 머티리얼은 색상별 캐시.
    /// </summary>
    public class MonsterPool
    {
        readonly Pool<GameObject> _pool;
        readonly Transform _root;
        readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();

        public int CreatedCount => _pool.CreatedCount;
        public int ActiveCount => _pool.ActiveCount;

        public MonsterPool(Transform root)
        {
            _root = root;
            _pool = new Pool<GameObject>(CreateGO, onRelease: Deactivate);
        }

        GameObject CreateGO()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.AddComponent<NavMeshAgent>();
            go.AddComponent<Monster>();
            go.AddComponent<Health>();
            Deactivate(go);
            return go;
        }

        /// <summary>풀에서 멤버 하나를 꺼내 def 사양으로 재구성하고 navmesh 위에 둔다.</summary>
        public GameObject Get(Vector3 pos, Transform parent, float diameter, Color color, float moveSpeed, float maxHP, DefenseProfile defense)
        {
            var go = _pool.Get();

            Vector3 p = Snap(pos, 4f);
            p.y = diameter;
            go.transform.SetParent(parent, true);
            go.transform.position = p;
            go.transform.localScale = Vector3.one * diameter;

            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = Mat(color);

            go.SetActive(true); // NavMeshAgent 가 여기서 navmesh 에 매핑됨(위치가 navmesh 위라야 함)

            var ag = go.GetComponent<NavMeshAgent>();
            // NavMeshAgent 치수는 transform.scale(=diameter)로 곱해지므로 로컬값으로 환산.
            ag.radius = Mathf.Min(diameter * 0.5f, 0.6f) / diameter;
            ag.height = 2f;
            ag.baseOffset = 1f;
            ag.speed = moveSpeed;
            ag.acceleration = 9999f;
            ag.angularSpeed = 0f;
            ag.updateRotation = false;
            ag.autoBraking = false;
            ag.avoidancePriority = 60;
            if (ag.isOnNavMesh) ag.Warp(p);

            var m = go.GetComponent<Monster>();
            m.ResetForReuse();
            m.SetGroundY(p.y); // XZ 평면 고정(스폰 높이)
            m.Mover = new AgentMover(ag);

            var hp = go.GetComponent<Health>();
            hp.Init(maxHP, defense);
            m.Health = hp;
            return go;
        }

        public void Release(GameObject go) => _pool.Release(go);

        void Deactivate(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            if (_root != null) go.transform.SetParent(_root, false);
        }

        static Vector3 Snap(Vector3 p, float radius)
            => NavMesh.SamplePosition(p, out var hit, radius, NavMesh.AllAreas) ? hit.position : p;

        Material Mat(Color c)
        {
            if (!_mats.TryGetValue(c, out var m))
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                m = new Material(shader) { name = "MonsterMat", color = c };
                _mats[c] = m;
            }
            return m;
        }
    }
}
