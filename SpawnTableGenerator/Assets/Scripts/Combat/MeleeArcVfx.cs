using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 근접 공격 시 부채꼴 범위 표시 메시(빔 셰이더, 발광). Init() 후 자동 페이드·소멸.
    /// 공격 방향으로 왼쪽에서 오른쪽으로 회전하며 나타나는 연출.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeleeArcVfx : MonoBehaviour
    {
        float _duration;
        float _timer;
        float _halfArc;
        Material _mat;
        float _baseIntensity;

        // 왼→오 스윕 애니메이션
        bool _sweep;
        float _sweepFrom;
        float _sweepTo;

        public void Init(float radius, float arcDeg, float duration, Color color, bool sweepAnim = true)
        {
            _duration = duration;
            _timer = duration;
            _halfArc = arcDeg * 0.5f;
            _sweep = sweepAnim;
            _sweepFrom = -_halfArc;
            _sweepTo = _halfArc;
            _baseIntensity = 10f;

            GetComponent<MeshFilter>().mesh = BuildArcMesh(radius, arcDeg, 16);

            var sh = Shader.Find("SpawnSystem/Beam");
            _mat = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Unlit"));
            if (sh != null)
            {
                _mat.SetColor("_Color", color);
                _mat.SetColor("_CoreColor", Color.Lerp(color, Color.white, 0.55f));
                _mat.SetFloat("_Intensity", _baseIntensity);
            }
            GetComponent<MeshRenderer>().sharedMaterial = _mat;
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - _timer / _duration); // 0→1

            if (_sweep)
            {
                // 왼쪽(-halfArc)에서 오른쪽(+halfArc)으로 전체 스윕
                float angle = Mathf.Lerp(_sweepFrom, _sweepTo, t);
                transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 뒤쪽 절반에서 페이드 아웃
            float fade = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
            if (_mat != null && Shader.Find("SpawnSystem/Beam") != null)
                _mat.SetFloat("_Intensity", _baseIntensity * fade);

            if (_timer <= 0f) Destroy(gameObject);
        }

        static Mesh BuildArcMesh(float radius, float arcDeg, int segments)
        {
            var mesh = new Mesh { name = "ArcMesh" };
            int vtxCount = segments + 2;
            var verts = new Vector3[vtxCount];
            var tris = new int[segments * 3];

            verts[0] = Vector3.zero;
            float halfRad = arcDeg * 0.5f * Mathf.Deg2Rad;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Lerp(-halfRad, halfRad, t);
                verts[i + 1] = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
            }
            for (int i = 0; i < segments; i++)
            {
                tris[i * 3]     = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>부채꼴 VFX 스폰 — forward 방향 기준 arcDeg° 부채꼴.</summary>
        public static MeleeArcVfx Spawn(Vector3 worldPos, Vector3 forward, float radius,
                                         float arcDeg, float duration, Color color)
        {
            var go = new GameObject("MeleeArcVfx");
            // 지면에서 살짝 위 + forward 방향을 바라봄 (sweep은 Init 후 localRotation으로 처리)
            go.transform.position = new Vector3(worldPos.x, worldPos.y + 0.05f, worldPos.z);
            if (forward.sqrMagnitude > 1e-4f)
                go.transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.z));

            // MeshFilter/MeshRenderer는 RequireComponent로 자동 추가됨
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            var vfx = go.AddComponent<MeleeArcVfx>();
            vfx.Init(radius, arcDeg, duration, color);
            return vfx;
        }
    }
}
