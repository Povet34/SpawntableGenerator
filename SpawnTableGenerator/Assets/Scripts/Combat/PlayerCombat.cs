using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 플레이어 공격(스타워즈 빔 느낌). 1키 = 광선검 범위 근접(관통 데미지 + 발광 링),
    /// 2키 = 블래스터 히트스캔(커서 조준, LineRenderer 빔, 첫 몬스터에 데미지). 발광은 Bloom + 빔 셰이더.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("근접 (1) — 광선검")]
        public float meleeRadius = 4.5f;
        public float meleeDamage = 40f;
        public DamageType meleeDamageType = DamageType.Piercing; // 광선검 = 장갑 관통
        public Color meleeColor = new Color(0.25f, 1f, 0.45f);

        [Header("원거리 (2) — 블래스터 히트스캔")]
        public float rangedRange = 35f;
        public float rangedDamage = 14f;
        public DamageType rangedDamageType = DamageType.Normal;
        public Color rangedColor = new Color(1f, 0.3f, 0.2f);

        Camera _cam;
        LineRenderer _beam;
        float _beamTimer;
        Transform _ring;
        float _ringTimer;
        readonly List<Vector3> _posBuf = new List<Vector3>();
        readonly List<Monster> _monBuf = new List<Monster>();

        void Awake()
        {
            _cam = Camera.main;
            BuildBeam();
            BuildRing();
        }

        void Update()
        {
            if (_cam == null) _cam = Camera.main;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) FireMelee();
                if (kb.digit2Key.wasPressedThisFrame) FireRanged();
            }
            TickVisuals(Time.deltaTime);
        }

        /// <summary>광선검 휘두르기 — 반경 내 몬스터에 관통 데미지.</summary>
        public void FireMelee()
        {
            GatherMonsters();
            var hits = AoeTargets.InRadius(transform.position, meleeRadius, _posBuf);
            foreach (int i in hits)
            {
                var h = _monBuf[i].Health;
                if (h != null) h.TakeDamage(meleeDamage, meleeDamageType);
            }
            _ringTimer = 0.2f;
        }

        public void FireRanged() => FireRangedDir(AimDir());

        /// <summary>블래스터 히트스캔 — dir 방향 첫 몬스터에 데미지 + 빔 표시.</summary>
        public void FireRangedDir(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) dir = transform.forward;
            dir.Normalize();

            Vector3 origin = transform.position + Vector3.up * 1f + dir * 1.2f; // 자기 콜라이더 회피
            Vector3 end = origin + dir * rangedRange;
            if (Physics.Raycast(origin, dir, out var hit, rangedRange))
            {
                end = hit.point;
                var m = hit.collider.GetComponentInParent<Monster>();
                if (m != null && m.Health != null) m.Health.TakeDamage(rangedDamage, rangedDamageType);
            }
            ShowBeam(origin, end);
        }

        Vector3 AimDir()
        {
            if (_cam != null && Mouse.current != null)
            {
                Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
                if (plane.Raycast(ray, out float e))
                {
                    Vector3 d = ray.GetPoint(e) - transform.position;
                    d.y = 0f;
                    if (d.sqrMagnitude > 1e-4f) return d.normalized;
                }
            }
            return transform.forward;
        }

        void GatherMonsters()
        {
            _posBuf.Clear();
            _monBuf.Clear();
            var all = Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
            foreach (var m in all)
            {
                if (m == null || m.Health == null || m.Health.IsDead) continue;
                _monBuf.Add(m);
                _posBuf.Add(m.transform.position);
            }
        }

        // ---------- 비주얼 ----------

        void BuildBeam()
        {
            var go = new GameObject("Beam");
            go.transform.SetParent(transform, false);
            _beam = go.AddComponent<LineRenderer>();
            _beam.useWorldSpace = true;
            _beam.positionCount = 2;
            _beam.widthMultiplier = 0.18f;
            _beam.numCapVertices = 4;
            _beam.sharedMaterial = BeamMaterial(rangedColor, 6f);
            _beam.enabled = false;
        }

        void BuildRing()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "MeleeRing";
            go.transform.SetParent(transform, false);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(meleeRadius * 2f, 0.04f, meleeRadius * 2f);
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = BeamMaterial(meleeColor, 5f);
            _ring = go.transform;
            go.SetActive(false);
        }

        static Material BeamMaterial(Color c, float intensity)
        {
            var sh = Shader.Find("SpawnSystem/Beam");
            var m = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Unlit"));
            if (sh != null)
            {
                m.SetColor("_Color", c);
                m.SetColor("_CoreColor", Color.Lerp(c, Color.white, 0.6f));
                m.SetFloat("_Intensity", intensity);
            }
            else
            {
                m.color = c;
            }
            return m;
        }

        void ShowBeam(Vector3 a, Vector3 b)
        {
            _beam.SetPosition(0, a);
            _beam.SetPosition(1, b);
            _beam.enabled = true;
            _beamTimer = 0.07f;
        }

        void TickVisuals(float dt)
        {
            if (_beamTimer > 0f)
            {
                _beamTimer -= dt;
                if (_beamTimer <= 0f) _beam.enabled = false;
            }
            if (_ringTimer > 0f && _ring != null)
            {
                _ringTimer -= dt;
                if (!_ring.gameObject.activeSelf) _ring.gameObject.SetActive(true);
                float t = 1f - Mathf.Clamp01(_ringTimer / 0.2f);
                float s = Mathf.Lerp(meleeRadius * 0.4f, meleeRadius * 2f, t);
                _ring.localScale = new Vector3(s, 0.04f, s);
                if (_ringTimer <= 0f) _ring.gameObject.SetActive(false);
            }
        }
    }
}
