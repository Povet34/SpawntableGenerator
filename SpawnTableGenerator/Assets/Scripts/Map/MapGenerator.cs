using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace SpawnSystem.Map
{
    /// <summary>
    /// 평면 바닥 + 경계 벽 + 복잡도 기반 내부 장애물을 절차적으로 생성하고 NavMesh를 베이크한다.
    /// 설계 문서 7장(맵/스폰 지점)의 "평면 Plane + 큐브 벽, NavMeshSurface 베이크"를 구현.
    /// 에디터 인스펙터(<see cref="MapGenerator"/> 커스텀 에디터)의 버튼 또는 런타임에서 Generate()를 호출.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public class MapGenerator : MonoBehaviour
    {
        [Header("맵 크기 (월드 유닛)")]
        [Min(10f)] public float width = 40f;
        [Min(10f)] public float length = 40f;

        [Header("벽")]
        [Min(0.5f)] public float wallHeight = 3f;
        [Min(0.2f)] public float wallThickness = 1f;

        [Header("복잡도")]
        [Tooltip("0 = 빈 평지, 1 = 최대 장애물 밀도")]
        [Range(0f, 1f)] public float complexity = 0.5f;
        [Tooltip("complexity = 1 일 때 시도하는 내부 장애물 개수")]
        [Min(0)] public int maxObstacles = 30;
        [Tooltip("내부 장애물 한 변 길이 범위 (min, max)")]
        public Vector2 obstacleSizeRange = new Vector2(2f, 6f);

        [Header("재현성")]
        public int seed = 12345;
        [Tooltip("맵 중앙에 비워둘 반경 (플레이어 스폰/시작 안전지대)")]
        [Min(0f)] public float centerClearRadius = 6f;

        [Header("NavMesh")]
        [Tooltip("Generate 시 NavMesh를 자동 베이크")]
        public bool bakeOnGenerate = true;

        const string GeneratedRootName = "Generated";

        Material _floorMat;
        Material _wallMat;
        Material _obstacleMat;

        /// <summary>생성된 지오메트리가 들어가는 자식 루트(재생성 시 통째로 교체).</summary>
        public Transform GeneratedRoot => transform.Find(GeneratedRootName);

        public void Generate()
        {
            Clear();

            var root = new GameObject(GeneratedRootName).transform;
            root.SetParent(transform, false);

            EnsureMaterials();
            BuildFloor(root);
            BuildBoundaryWalls(root);
            BuildObstacles(root);

            if (bakeOnGenerate)
                Bake();
        }

        public void Clear()
        {
            var existing = GeneratedRoot;
            if (existing != null)
                DestroyImmediateSafe(existing.gameObject);
        }

        public void Bake()
        {
            var surface = GetComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.BuildNavMesh();
        }

        void BuildFloor(Transform root)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(root, false);
            // 윗면이 정확히 y=0 에 오도록 두께 0.1 큐브를 -0.05 만큼 내림.
            floor.transform.localScale = new Vector3(width, 0.1f, length);
            floor.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            ApplyMaterial(floor, _floorMat);
        }

        void BuildBoundaryWalls(Transform root)
        {
            float halfW = width * 0.5f;
            float halfL = length * 0.5f;
            float y = wallHeight * 0.5f;
            float t = wallThickness;

            // 4면. 길이 방향으로 두께만큼 살짝 더 길게 빼서 모서리 빈틈 제거.
            CreateWall(root, "Wall_North", new Vector3(0f, y, halfL + t * 0.5f), new Vector3(width + t * 2f, wallHeight, t));
            CreateWall(root, "Wall_South", new Vector3(0f, y, -halfL - t * 0.5f), new Vector3(width + t * 2f, wallHeight, t));
            CreateWall(root, "Wall_East", new Vector3(halfW + t * 0.5f, y, 0f), new Vector3(t, wallHeight, length));
            CreateWall(root, "Wall_West", new Vector3(-halfW - t * 0.5f, y, 0f), new Vector3(t, wallHeight, length));
        }

        void CreateWall(Transform root, string name, Vector3 localPos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(root, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = scale;
            ApplyMaterial(wall, _wallMat);
        }

        void BuildObstacles(Transform root)
        {
            int target = Mathf.RoundToInt(complexity * maxObstacles);
            if (target <= 0)
                return;

            var rng = new System.Random(seed);
            float halfW = width * 0.5f - wallThickness;
            float halfL = length * 0.5f - wallThickness;

            var placed = new List<(Vector2 pos, float radius)>();
            int attempts = 0;
            int maxAttempts = target * 12;

            while (placed.Count < target && attempts < maxAttempts)
            {
                attempts++;
                float sx = Mathf.Lerp(obstacleSizeRange.x, obstacleSizeRange.y, (float)rng.NextDouble());
                float sz = Mathf.Lerp(obstacleSizeRange.x, obstacleSizeRange.y, (float)rng.NextDouble());
                float margin = Mathf.Max(sx, sz) * 0.5f;

                float x = Mathf.Lerp(-halfW + margin, halfW - margin, (float)rng.NextDouble());
                float z = Mathf.Lerp(-halfL + margin, halfL - margin, (float)rng.NextDouble());
                var p = new Vector2(x, z);
                float radius = Mathf.Max(sx, sz) * 0.5f;

                // 중앙 안전지대 회피.
                if (p.magnitude < centerClearRadius + radius)
                    continue;

                // 기존 장애물과 겹침 회피(통로 확보용 여유 1.5 유닛).
                bool overlaps = false;
                foreach (var (other, r) in placed)
                {
                    if (Vector2.Distance(p, other) < radius + r + 1.5f)
                    {
                        overlaps = true;
                        break;
                    }
                }
                if (overlaps)
                    continue;

                placed.Add((p, radius));

                var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = $"Obstacle_{placed.Count:00}";
                obstacle.transform.SetParent(root, false);
                obstacle.transform.localScale = new Vector3(sx, wallHeight, sz);
                obstacle.transform.localPosition = new Vector3(x, wallHeight * 0.5f, z);
                obstacle.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                ApplyMaterial(obstacle, _obstacleMat);
            }
        }

        void EnsureMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard"); // URP 미사용 폴백

            _floorMat = new Material(shader) { name = "FloorMat" };
            _floorMat.color = new Color(0.32f, 0.34f, 0.38f);

            _wallMat = new Material(shader) { name = "WallMat" };
            _wallMat.color = new Color(0.18f, 0.20f, 0.24f);

            _obstacleMat = new Material(shader) { name = "ObstacleMat" };
            _obstacleMat.color = new Color(0.45f, 0.30f, 0.25f);
        }

        static void ApplyMaterial(GameObject go, Material mat)
        {
            if (mat == null)
                return;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = mat;
        }

        static void DestroyImmediateSafe(GameObject go)
        {
            if (Application.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * wallHeight * 0.5f,
                new Vector3(width, wallHeight, length));

            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.5f);
            DrawWireCircle(transform.position, centerClearRadius);
        }

        static void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float ang = i / (float)segments * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}
