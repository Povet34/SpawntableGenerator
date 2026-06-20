using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 레이저 투사체. SphereCollider(trigger) + Rigidbody(kinematic) 물리 피격판정.
    /// OnTriggerEnter: 벽·장애물·대상 모두 소멸. 맞는 팀이면 데미지 추가.
    /// </summary>
    public class LaserProjectile : MonoBehaviour
    {
        [Tooltip("탄에 붙는 글로우 라이트. 프리팹에 미리 넣어두면 그걸 쓰고, 없으면 런타임에 부착.")]
        [SerializeField] Light glowLight;
        [Tooltip("글로우 라이트 강도/반경(프리팹 라이트가 없을 때 기본값).")]
        [SerializeField] float glowIntensity = 4f;
        [SerializeField] float glowRange = 6f;

        bool _hitsPlayer;
        float _speed;
        float _range;
        float _damage;
        DamageType _damageType;
        Vector3 _dir;
        float _traveled;
        LineRenderer _lr;
        Rigidbody _rb;

        public void Init(Vector3 dir, float speed, float range, float damage, DamageType type,
                         Color color, bool hitsPlayer)
        {
            _dir = dir.normalized;
            _speed = speed;
            _range = range;
            _damage = damage;
            _damageType = type;
            _hitsPlayer = hitsPlayer;
            _traveled = 0f;
            BuildVisual(color);
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.12f;
        }

        void BuildVisual(Color c)
        {
            _lr = gameObject.AddComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            _lr.positionCount = 2;
            _lr.numCapVertices = 4;

            var wc = new AnimationCurve();
            wc.AddKey(0f, 0.06f);
            wc.AddKey(1f, 0.22f);
            _lr.widthCurve = wc;

            var sh = Shader.Find("SpawnSystem/Beam");
            var mat = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Unlit"));
            if (sh != null)
            {
                mat.SetColor("_Color", c);
                mat.SetColor("_CoreColor", Color.Lerp(c, Color.white, 0.6f));
                mat.SetFloat("_Intensity", 10f);
            }
            _lr.sharedMaterial = mat;
            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, transform.position);

            ConfigureGlow(c);
        }

        /// <summary>탄 색에 맞춰 글로우 라이트를 켠다(프리팹에 라이트가 없으면 부착).</summary>
        void ConfigureGlow(Color c)
        {
            if (glowLight == null)
            {
                glowLight = gameObject.GetComponent<Light>();
                if (glowLight == null) glowLight = gameObject.AddComponent<Light>();
            }
            glowLight.type = LightType.Point;
            glowLight.range = glowRange;
            glowLight.intensity = glowIntensity;
            // 색은 HDR일 수 있으니 최대 채널로 정규화해 라이트 색으로 사용.
            float mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            glowLight.color = mx > 1f ? new Color(c.r / mx, c.g / mx, c.b / mx, 1f) : c;
        }

        void FixedUpdate()
        {
            float step = _speed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + _dir * step);
            _traveled += step;
            if (_traveled >= _range) Destroy(gameObject);
        }

        void Update()
        {
            if (_lr == null) return;
            float tail = Mathf.Min(2f, _traveled);
            Vector3 head = transform.position;
            _lr.SetPosition(0, head - _dir * tail);
            _lr.SetPosition(1, head);
        }

        void OnTriggerEnter(Collider other)
        {
            // 다른 투사체 무시
            if (other.GetComponent<LaserProjectile>() != null) return;

            var hp = other.GetComponentInParent<Health>();
            if (hp != null && !hp.IsDead)
            {
                bool isMonster = other.GetComponentInParent<Monster>() != null;
                bool correctTarget = (_hitsPlayer && !isMonster) || (!_hitsPlayer && isMonster);
                if (correctTarget)
                {
                    hp.TakeDamage(_damage, _damageType);
                    if (_hitsPlayer)
                    {
                        var ctrl = other.GetComponentInParent<Player.PlayerController>();
                        if (ctrl != null) ctrl.AddKnockback(_dir * 5f);
                    }
                }
            }
            // 벽이든 대상이든 맞으면 소멸
            Destroy(gameObject);
        }

        /// <summary>프리팹에서 발사(라이트 등 프리팹 구성 그대로 사용). 프리팹이 null이면 코드 생성으로 폴백.</summary>
        public static LaserProjectile Spawn(GameObject prefab, Vector3 origin, Vector3 dir,
                                            RangedWeaponSO weapon, bool hitsPlayer)
        {
            if (prefab == null)
                return Spawn(origin, dir, weapon, hitsPlayer);

            var go = Object.Instantiate(prefab, origin, Quaternion.LookRotation(dir));
            var proj = go.GetComponent<LaserProjectile>();
            if (proj == null) proj = go.AddComponent<LaserProjectile>();
            proj.Init(dir, weapon.projectileSpeed, weapon.projectileRange,
                      weapon.damage, weapon.damageType, weapon.projectileColor, hitsPlayer);
            return proj;
        }

        /// <summary>플레이어 원거리 발사(코드 생성).</summary>
        public static LaserProjectile Spawn(Vector3 origin, Vector3 dir, RangedWeaponSO weapon, bool hitsPlayer)
        {
            var go = new GameObject("LaserProjectile");
            go.transform.position = origin;
            go.AddComponent<Rigidbody>();
            go.AddComponent<SphereCollider>();
            var proj = go.AddComponent<LaserProjectile>();
            proj.Init(dir, weapon.projectileSpeed, weapon.projectileRange,
                      weapon.damage, weapon.damageType, weapon.projectileColor, hitsPlayer);
            return proj;
        }

        /// <summary>몬스터 등 파라미터 직접 지정.</summary>
        public static LaserProjectile SpawnRaw(Vector3 origin, Vector3 dir, float speed, float range,
                                               float damage, DamageType type, Color color, bool hitsPlayer)
        {
            var go = new GameObject("LaserProjectile");
            go.transform.position = origin;
            go.AddComponent<Rigidbody>();
            go.AddComponent<SphereCollider>();
            var proj = go.AddComponent<LaserProjectile>();
            proj.Init(dir, speed, range, damage, type, color, hitsPlayer);
            return proj;
        }
    }
}
