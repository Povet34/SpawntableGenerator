using System.Collections.Generic;
using SpawnSystem.Monsters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpawnSystem.Combat
{
    /// <summary>
    /// 플레이어 전투 (스타워즈 빔 느낌). WeaponSO 기반.
    /// 1키 = 광선검 부채꼴 근접(120°, 왼→오 스윕, Hovl 슬래시 VFX).
    /// 2키 = 블래스터 레이저 투사체(커서 조준, 캐릭터 순간 회전).
    /// 공격 시 캐릭터가 커서 방향으로 즉시 전향.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("무기 SO (Inspector에서 할당)")]
        public WeaponSO meleeWeapon;
        public WeaponSO rangedWeapon;

        float _meleeCd;
        float _rangedCd;

        Camera _cam;
        readonly List<Vector3> _posBuf = new List<Vector3>();
        readonly List<Monster> _monBuf = new List<Monster>();

        Player.PlayerController _ctrl;

        void Awake()
        {
            _cam = Camera.main;
            _ctrl = GetComponent<Player.PlayerController>();
            // 런타임에 WeaponSO 미할당 시 기본값
            if (meleeWeapon == null) meleeWeapon = MakeDefaultMelee();
            if (rangedWeapon == null) rangedWeapon = MakeDefaultRanged();
        }

        void Update()
        {
            if (_cam == null) _cam = Camera.main;
            _meleeCd -= Time.deltaTime;
            _rangedCd -= Time.deltaTime;

            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame && _meleeCd <= 0f) FireMelee();
            if (kb.digit2Key.wasPressedThisFrame && _rangedCd <= 0f) FireRanged();
        }

        public void FireMelee()
        {
            Vector3 aimDir = AimDir();
            FaceDir(aimDir);

            // 부채꼴 데미지
            GatherMonsters();
            var hits = AoeTargets.InArc(transform.position, aimDir,
                                         meleeWeapon.meleeRadius, meleeWeapon.meleeArcDeg, _posBuf);
            foreach (int i in hits)
            {
                var h = _monBuf[i].Health;
                if (h != null) h.TakeDamage(meleeWeapon.damage, meleeWeapon.damageType);
            }

            // 부채꼴 VFX (빔 셰이더)
            Color c = new Color(0.3f, 1f, 0.5f); // 연초록
            MeleeArcVfx.Spawn(transform.position, aimDir,
                               meleeWeapon.meleeRadius, meleeWeapon.meleeArcDeg, 0.22f, c);

            // Hovl Studio 슬래시 VFX (Inspector에서 SO에 할당됐을 때)
            if (meleeWeapon.slashVfxPrefab != null)
            {
                var vgo = Object.Instantiate(meleeWeapon.slashVfxPrefab, transform.position,
                                             Quaternion.LookRotation(aimDir));
                Object.Destroy(vgo, 3f);
            }

            _meleeCd = meleeWeapon.cooldown;
            NotifyViewPressure(aimDir);
        }

        public void FireRanged()
        {
            Vector3 aimDir = AimDir();
            FaceDir(aimDir);

            Vector3 origin = transform.position + Vector3.up * 1.1f + aimDir * 1.2f;
            LaserProjectile.Spawn(origin, aimDir, rangedWeapon, hitsPlayer: false);

            _rangedCd = rangedWeapon.cooldown;
            NotifyViewPressure(aimDir);
        }

        // 커서 → 지면 투영 조준 방향
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

        void FaceDir(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return;
            transform.rotation = Quaternion.LookRotation(dir);
            // PlayerController의 NavMeshAgent 수동 회전 모드에 알림
            if (_ctrl != null) _ctrl.OverrideRotation(dir);
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

        // 몬스터가 플레이어의 시야압 계산에 사용할 마지막 공격 방향을 MonsterPack에 브로드캐스트
        void NotifyViewPressure(Vector3 dir)
        {
            var packs = Object.FindObjectsByType<Monsters.MonsterPack>(FindObjectsSortMode.None);
            foreach (var pk in packs)
                pk.NotifyPlayerAttack(dir);
        }

        static WeaponSO MakeDefaultMelee()
        {
            var w = ScriptableObject.CreateInstance<WeaponSO>();
            w.kind = WeaponKind.Melee;
            w.damage = 40f;
            w.damageType = DamageType.Piercing;
            w.cooldown = 0.45f;
            w.meleeRadius = 4.5f;
            w.meleeArcDeg = 120f;
            return w;
        }

        static WeaponSO MakeDefaultRanged()
        {
            var w = ScriptableObject.CreateInstance<WeaponSO>();
            w.kind = WeaponKind.Ranged;
            w.damage = 14f;
            w.damageType = DamageType.Normal;
            w.cooldown = 0.25f;
            w.projectileSpeed = 45f;
            w.projectileRange = 35f;
            w.projectileColor = new Color(4f, 0.6f, 0.4f, 1f);
            return w;
        }
    }
}
