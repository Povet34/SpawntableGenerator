using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SpawnSystem.Tests
{
    /// <summary>
    /// 통합 테스트용 커스텀 도구. 바닥 + (가운데) 벽을 코드로 만들고 런타임 NavMesh 를 베이크한다.
    /// 스크린샷 없이 길찾기/에이전트 동작을 검증하기 위한 결정적(deterministic) 환경.
    /// using 선언으로 감싸면 테스트 종료 시 navmesh 데이터 제거 + 오브젝트 파괴까지 자동 정리.
    ///
    /// 좌표 규약(WithCenterWall 기본값): 벽은 x∈[-5,5], z∈[-0.5,0.5], 높이 3.
    /// → 벽 앞(z<0)에서 벽 뒤(z>0)로 가려면 벽 끝(x=±5)을 우회해야 한다.
    /// </summary>
    public sealed class NavTestEnvironment : System.IDisposable
    {
        public GameObject Root { get; private set; }
        public GameObject Floor { get; private set; }
        public GameObject Wall { get; private set; }
        public NavMeshSurface Surface { get; private set; }

        public static NavTestEnvironment WithCenterWall(
            float floorSize = 40f,
            Vector3? wallCenter = null,
            Vector3? wallSize = null)
        {
            var env = new NavTestEnvironment();
            env.Root = new GameObject("NavTestEnv");

            env.Floor = CreateBox("Floor", env.Root.transform,
                new Vector3(0f, -0.05f, 0f), new Vector3(floorSize, 0.1f, floorSize));

            Vector3 wc = wallCenter ?? new Vector3(0f, 1.5f, 0f);
            Vector3 ws = wallSize ?? new Vector3(10f, 3f, 1f);
            env.Wall = CreateBox("Wall", env.Root.transform, wc, ws);

            env.Surface = env.Root.AddComponent<NavMeshSurface>();
            env.Surface.collectObjects = CollectObjects.Children;
            env.Surface.BuildNavMesh();
            return env;
        }

        static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            return go;
        }

        public bool HasNavMesh()
        {
            var tri = NavMesh.CalculateTriangulation();
            return tri.indices != null && tri.indices.Length >= 3;
        }

        public void Dispose()
        {
            if (Surface != null)
                Surface.RemoveData();
            if (Root != null)
                Object.DestroyImmediate(Root);
        }
    }
}
