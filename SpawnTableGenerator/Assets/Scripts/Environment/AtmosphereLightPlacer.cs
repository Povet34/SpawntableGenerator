using System.Collections.Generic;
using SpawnSystem.Map;
using UnityEngine;

namespace SpawnSystem.Environment
{
    /// <summary>
    /// 맵 장애물 일부 위에 따뜻한 포인트 라이트를 배치하고(맵 재생성 시 갱신),
    /// 밤일수록 밝게 켠다(낮엔 꺼짐). MapGenerator.onGenerated + 낮/밤 두 시임을 모두 구독.
    /// (GameDesign.md §3.3 분위기 포인트 라이팅)
    /// </summary>
    public class AtmosphereLightPlacer : DayNightResponderBehaviour
    {
        [Tooltip("비우면 씬에서 자동 탐색")]
        public MapGenerator mapGenerator;

        [Range(0f, 1f)] public float obstacleFraction = 0.2f;
        public float maxIntensity = 2.5f;
        public Vector2 rangeMinMax = new Vector2(6f, 10f);
        public Color color = new Color(1f, 0.54f, 0.24f); // #FF8A3D 따뜻한 주황
        public int seed = 9001;

        readonly List<Light> _lights = new List<Light>();
        Transform _root;
        float _darkness;

        protected override void OnEnable()
        {
            if (mapGenerator == null)
                mapGenerator = Object.FindAnyObjectByType<MapGenerator>();
            if (mapGenerator != null)
                mapGenerator.onGenerated += OnMapGenerated;
            base.OnEnable();
        }

        void Start()
        {
            // 맵이 런타임에 재생성되지 않고 씬에 미리 생성돼 있는 경우를 위해 초기 1회 배치.
            if (_lights.Count == 0 && mapGenerator != null && mapGenerator.GeneratedRoot != null)
                Rebuild();
        }

        protected override void OnDisable()
        {
            if (mapGenerator != null)
                mapGenerator.onGenerated -= OnMapGenerated;
            base.OnDisable();
        }

        void OnMapGenerated(float width, float length)
        {
            Rebuild();
        }

        void Rebuild()
        {
            ClearLights();
            if (mapGenerator == null) return;

            var genRoot = mapGenerator.GeneratedRoot;
            if (genRoot == null) return;

            var obstacles = new List<Transform>();
            foreach (Transform child in genRoot)
                if (child.name.StartsWith("Obstacle_"))
                    obstacles.Add(child);
            if (obstacles.Count == 0) return;

            if (_root == null)
            {
                _root = new GameObject("AtmosphereLights").transform;
                _root.SetParent(transform, false);
            }

            var rng = new System.Random(seed);
            int target = Mathf.Max(1, Mathf.RoundToInt(obstacles.Count * obstacleFraction));
            // Fisher-Yates 일부 셔플로 무작위 N개 선택
            for (int i = 0; i < target && i < obstacles.Count; i++)
            {
                int j = i + rng.Next(obstacles.Count - i);
                (obstacles[i], obstacles[j]) = (obstacles[j], obstacles[i]);

                var ob = obstacles[i];
                var go = new GameObject($"AtmoLight_{i:00}");
                go.transform.SetParent(_root, false);
                go.transform.position = ob.position + Vector3.up * (ob.localScale.y * 0.5f + 1.5f);

                var lt = go.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = color;
                lt.range = Mathf.Lerp(rangeMinMax.x, rangeMinMax.y, (float)rng.NextDouble());
                lt.intensity = 0f; // 낮 기본값. OnDayNight에서 어둠에 비례해 밝아짐
                _lights.Add(lt);
            }

            ApplyDarkness();
        }

        public override void OnDayNight(in DayNightState state)
        {
            _darkness = state.Darkness01;
            ApplyDarkness();
        }

        void ApplyDarkness()
        {
            float intensity = maxIntensity * _darkness;
            for (int i = 0; i < _lights.Count; i++)
            {
                if (_lights[i] == null) continue;
                _lights[i].intensity = intensity;
                _lights[i].enabled = _darkness > 0.02f;
            }
        }

        void ClearLights()
        {
            foreach (var lt in _lights)
                if (lt != null) DestroyImmediateSafe(lt.gameObject);
            _lights.Clear();
        }

        static void DestroyImmediateSafe(GameObject go)
        {
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }
}
