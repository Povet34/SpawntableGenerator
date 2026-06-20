using SpawnSystem.Map;
using SpawnSystem.Player;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SpawnSystem.EditorTools
{
    /// <summary>
    /// 구현 로드맵 1번 "테스트 씬 스켈레톤" 자동 구성:
    /// 평면 NavMesh + 큐브 벽(+장애물) + 클릭 이동 플레이어(캡슐) + 시네머신 탑다운 카메라.
    /// Tools/Spawn System/Build Test Scene 메뉴로 실행.
    /// </summary>
    public static class TestSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SpawnTestScene.unity";

        [MenuItem("Tools/Spawn System/Build Test Scene", priority = 0)]
        public static void BuildTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupEnvironmentLighting();
            CreateDirectionalLight();

            var map = CreateMap();
            var player = CreatePlayer();
            CreateCinemachineCamera(player.transform);

            // 저장
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            Selection.activeGameObject = player;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();

            Debug.Log($"[TestSceneBuilder] 테스트 씬 생성 완료 → {ScenePath}. " +
                      "MapGenerator 인스펙터에서 크기/복잡도를 바꾸고 Generate 를 누르면 맵이 재생성됩니다.");
        }

        static void SetupEnvironmentLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.62f);
        }

        static void CreateDirectionalLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static MapGenerator CreateMap()
        {
            var go = new GameObject("MapGenerator");
            // NavMeshSurface 는 MapGenerator 의 [RequireComponent] 로 자동 추가됨.
            var map = go.AddComponent<MapGenerator>();
            map.Generate();
            return map;
        }

        static GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.tag = "Player";

            var mat = new Material(GetUrpLitShader()) { name = "PlayerMat" };
            mat.color = new Color(0.2f, 0.6f, 1f);
            player.GetComponent<Renderer>().sharedMaterial = mat;

            var agent = player.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.baseOffset = 1f;       // 캡슐 피벗(중심)을 바닥 위로 올림
            agent.speed = 6f;
            agent.angularSpeed = 720f;
            agent.acceleration = 40f;
            agent.stoppingDistance = 0.1f;

            player.AddComponent<PlayerController>();
            return player;
        }

        static void CreateCinemachineCamera(Transform follow)
        {
            // Main Camera + CinemachineBrain
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CinemachineBrain>();

            // 시네머신 탑다운 가상 카메라 (각도 있는 탑다운)
            var vcamGo = new GameObject("CM TopDown");
            vcamGo.transform.rotation = Quaternion.Euler(56f, 0f, 0f);
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            vcam.Follow = follow;

            // 위치 추종: 월드공간 고정 오프셋 → 플레이어가 회전해도 카메라는 흔들리지 않음.
            var body = vcamGo.AddComponent<CinemachineFollow>();
            body.FollowOffset = new Vector3(0f, 18f, -12f);
            body.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
            body.TrackerSettings.PositionDamping = new Vector3(0.5f, 0.5f, 0.5f);

            // 회전 제어 컴포넌트를 두지 않으면 vcam 은 자기 트랜스폼 회전을 유지 → 고정 탑다운 각.
        }

        static Shader GetUrpLitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            return shader != null ? shader : Shader.Find("Standard");
        }
    }
}
