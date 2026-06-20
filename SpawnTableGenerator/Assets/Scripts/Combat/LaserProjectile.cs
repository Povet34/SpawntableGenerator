using SpawnSystem.Monsters;
using UnityEngine;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 레이저 총알 투사체. LineRenderer 트레일로 빠르게 날아가는 빔 느낌.
    /// 몬스터 또는 플레이어에 닿으면 데미지 후 소멸. Init() 호출 필수.
    /// </summary>
    public class LaserProjectile : MonoBehaviour
    {
        bool _hitsPlayer;
        float _speed;
        float _range;
        float _damage;
        DamageType _damageType;

        Vector3 _dir;
        float _traveled;
        LineRenderer _lr;

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

        void BuildVisual(Color c)
        {
            _lr = gameObject.AddComponent<LineRenderer>();
            _lr.useWorldSpace = true;
            _lr.positionCount = 2;

            var wc = new AnimationCurve();
            wc.AddKey(0f, 0.08f);
            wc.AddKey(1f, 0.2f);
            _lr.widthCurve = wc;
            _lr.numCapVertices = 4;

            var sh = Shader.Find("SpawnSystem/Beam");
            var mat = new Material(sh != null ? sh : Shader.Find("Universal Render Pipeline/Unlit"));
            if (sh != null)
            {
                mat.SetColor("_Color", c);
                mat.SetColor("_CoreColor", Color.Lerp(c, Color.white, 0.6f));
                mat.SetFloat("_Intensity", 8f);
            }
            _lr.sharedMaterial = mat;

            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, transform.position);
        }

        void Update()
        {
            float step = _speed * Time.deltaTime;
            Vector3 prev = transform.position;
            transform.position += _dir * step;
            _traveled += step;

            // 트레일: 뒤쪽 꼬리
            float tail = Mathf.Min(1.8f, _traveled);
            _lr.SetPosition(0, transform.position - _dir * tail);
            _lr.SetPosition(1, transform.position);

            // 충돌 감지
            if (Physics.Raycast(prev, _dir, out var hit, step + 0.15f))
            {
                var hp = hit.collider.GetComponentInParent<Health>();
                if (hp != null && !hp.IsDead)
                {
                    bool isMonster = hit.collider.GetComponentInParent<Monster>() != null;
                    bool shouldHit = (_hitsPlayer && !isMonster) || (!_hitsPlayer && isMonster);
                    if (shouldHit)
                    {
                        hp.TakeDamage(_damage, _damageType);
                        // 넉백 (플레이어에게 닿았을 때만)
                        if (_hitsPlayer)
                        {
                            var pc = hit.collider.GetComponentInParent<Player.PlayerController>();
                            if (pc != null) pc.AddKnockback(_dir * 6f);
                        }
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            if (_traveled >= _range)
                Destroy(gameObject);
        }

        public static LaserProjectile Spawn(Vector3 origin, Vector3 dir, WeaponSO weapon, bool hitsPlayer)
        {
            var go = new GameObject("LaserProjectile");
            go.transform.position = origin;
            var proj = go.AddComponent<LaserProjectile>();
            proj.Init(dir, weapon.projectileSpeed, weapon.projectileRange,
                      weapon.damage, weapon.damageType, weapon.projectileColor, hitsPlayer);
            return proj;
        }

        /// <summary>속도·색상·범위를 직접 지정하는 간편 오버로드 (몬스터 원거리용).</summary>
        public static LaserProjectile SpawnRaw(Vector3 origin, Vector3 dir, float speed, float range,
                                               float damage, DamageType type, Color color, bool hitsPlayer)
        {
            var go = new GameObject("LaserProjectile");
            go.transform.position = origin;
            var proj = go.AddComponent<LaserProjectile>();
            proj.Init(dir, speed, range, damage, type, color, hitsPlayer);
            return proj;
        }
    }
}
