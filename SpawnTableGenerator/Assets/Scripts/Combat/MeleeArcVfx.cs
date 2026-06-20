using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 근접 공격 부채꼴 VFX. 왼쪽 끝에서 오른쪽 끝으로 메시를 점진적으로 키워
    /// 휙 느낌을 냄 — 오브젝트 자체는 회전 없음(localRotation 버그 방지).
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeleeArcVfx : MonoBehaviour
    {
        float _totalArcRad;
        float _startAngleRad; // 왼쪽 시작각(forward 기준 -halfArc)
        float _radius;
        float _duration;
        float _timer;
        Material _mat;
        MeshFilter _mf;
        const float BaseIntensity = 12f;

        public void Init(float radius, float arcDeg, float duration, Color color)
        {
            _radius = radius;
            _totalArcRad = arcDeg * Mathf.Deg2Rad;
            _startAngleRad = -arcDeg * 0.5f * Mathf.Deg2Rad;
            _duration = duration;
            _timer = duration;

            _mf = GetComponent<MeshFilter>();
            _mf.mesh = new Mesh { name = "ArcMesh" };

            var sh = Shader.Find("SpawnSystem/Beam");
            _mat = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Unlit"));
            if (sh != null)
            {
                _mat.SetColor("_Color", color);
                _mat.SetColor("_CoreColor", Color.Lerp(color, Color.white, 0.6f));
                _mat.SetFloat("_Intensity", BaseIntensity);
            }
            GetComponent<MeshRenderer>().sharedMaterial = _mat;
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) { Destroy(gameObject); return; }

            float t = 1f - (_timer / _duration); // 0→1

            // 메시를 왼쪽에서 오른쪽으로 점점 확장 (오브젝트 회전 없음!)
            float currentArcRad = Mathf.Max(0.05f, _totalArcRad * t);
            _mf.mesh = BuildArc(_radius, _startAngleRad, currentArcRad, 12);

            // 후반 페이드
            float fade = t < 0.45f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.45f) / 0.55f);
            if (_mat != null) _mat.SetFloat("_Intensity", BaseIntensity * fade);
        }

        static Mesh BuildArc(float radius, float startRad, float arcRad, int segs)
        {
            var mesh = new Mesh { name = "ArcMesh" };
            var verts = new Vector3[segs + 2];
            var tris = new int[segs * 3];
            verts[0] = Vector3.zero;
            for (int i = 0; i <= segs; i++)
            {
                float a = startRad + arcRad * ((float)i / segs);
                verts[i + 1] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
            }
            for (int i = 0; i < segs; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>forward 방향 기준 arcDeg° 부채꼴을 duration 초간 휙 그리고 소멸.</summary>
        public static MeleeArcVfx Spawn(Vector3 worldPos, Vector3 forward, float radius,
                                         float arcDeg, float duration, Color color)
        {
            var go = new GameObject("MeleeArcVfx");
            go.transform.position = new Vector3(worldPos.x, worldPos.y + 0.05f, worldPos.z);
            Vector3 fwd = new Vector3(forward.x, 0f, forward.z);
            // 오브젝트 방향 = 공격 forward 방향 (이후 localRotation 건드리지 않음)
            if (fwd.sqrMagnitude > 1e-4f)
                go.transform.rotation = Quaternion.LookRotation(fwd);

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            var vfx = go.AddComponent<MeleeArcVfx>();
            vfx.Init(radius, arcDeg, duration, color);
            return vfx;
        }
    }
}
